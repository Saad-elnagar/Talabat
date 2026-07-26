using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

[AttributeUsage(AttributeTargets.Method)]
public class CachAttribute : Attribute, IAsyncActionFilter
{
    private readonly int _timeToLiveInSeconds;

    public CachAttribute(int timeToLiveInSeconds)
    {
        _timeToLiveInSeconds = timeToLiveInSeconds;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cacheService = context.HttpContext.RequestServices
            .GetRequiredService<ICachService>();

        var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);
        var cachedResponse = await cacheService.GetCachedResponseAsync(cacheKey);

        // ========== CACHE HIT ==========
        if (!string.IsNullOrEmpty(cachedResponse))
        {

            context.Result = new ContentResult
            {
                Content = cachedResponse,
                ContentType = "application/json",
                StatusCode = 200
            };
            return;
        }

        // ========== CACHE MISS ==========
        var executedContext = await next();

        if (executedContext.Result is OkObjectResult okObjectResult
            && okObjectResult.Value != null)
        {

            await cacheService.CacheResponseAsync(
                cacheKey,
                okObjectResult.Value,
                TimeSpan.FromSeconds(_timeToLiveInSeconds));
        }
    }

    private string GenerateCacheKeyFromRequest(HttpRequest request)
    {
        var keyBuilder = new StringBuilder();
        keyBuilder.Append(request.Path);

        foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
        {
            keyBuilder.Append($"|{key}={value}");
        }

        return keyBuilder.ToString();
    }
}
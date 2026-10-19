using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using Talabat.Application.Abstractions;

namespace Talabat.Api.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class CacheAttribute : Attribute, IAsyncActionFilter
{
    private readonly int _timeToLiveInSeconds;

    public CacheAttribute(int timeToLiveInSeconds)
    {
        _timeToLiveInSeconds = timeToLiveInSeconds;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cacheService = context.HttpContext.RequestServices
            .GetRequiredService<ICacheService>();

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
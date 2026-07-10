using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace Talabat.Helper
{
    public class CachAttribute : Attribute, IAsyncActionFilter
    {
        public int _time_ToLive { get; }
        public CachAttribute(int time_ToLive)
        {
            _time_ToLive = time_ToLive;
        }

        
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get the cache service from the request services use Dependency Injection
            var cachService = context.HttpContext.RequestServices.GetRequiredService<Talabat.Core.Service.ICachService>();
            // Generate a cache key based on the request
            var cachKey = GenerateCachKeyFromRequest(context.HttpContext.Request);
            // Check if the response is already cached
            var cachedResponse = await cachService.GetCachAsync(cachKey);

            if (!string.IsNullOrEmpty(cachedResponse))
            {
                var contentResult = new ContentResult
                {
                    Content = cachedResponse,
                    ContentType = "application/json",
                    StatusCode = 200
                };


                context.Result = contentResult;

                return; // Important: do not call next() when Result is set
            }
            var ExecutedContext = await next();
            // Execute the action and get the result
            if (ExecutedContext.Result is OkObjectResult okObjectResult)
            {
                // Cache the response for future requests
                await cachService.CachRepoAsync(cachKey, okObjectResult.Value!, TimeSpan.FromSeconds(_time_ToLive)); 
            }

        }

        private string GenerateCachKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}");
            foreach (var (key, value) in request.Query)
            {
                keyBuilder.Append($"|{key}-{value}");
            }
            return keyBuilder.ToString();

        }
    }
}

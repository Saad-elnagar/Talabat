using System.Net;
using System.Text.Json;
using Talabat.Erorr;

namespace Talabat.Middleware
{
    /// <summary>
    /// Sits at the very top of the pipeline and catches anything that wasn't handled further
    /// down (unexpected exceptions from controllers, services, EF Core, etc.). Without this,
    /// an unhandled exception returns ASP.NET Core's default developer exception page (in dev)
    /// or an empty 500 with no body (in production) — neither is useful to API clients, and the
    /// dev page can leak stack traces / connection strings if it's ever hit in production.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Only include the real exception message/stack trace in development — in
                // production this would leak internals (SQL details, file paths, etc.) to callers.
                var response = _env.IsDevelopment()
                    ? new APIRespose(500, $"{ex.Message}")
                    : new APIRespose(500);

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
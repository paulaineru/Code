using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace AuthenticationService.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetCorrelationId(context);
            context.Items["CorrelationId"] = correlationId;

            // Add correlation ID to response headers
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
                return Task.CompletedTask;
            });

            // Set correlation ID in the current activity for distributed tracing
            Activity.Current?.SetTag("correlation.id", correlationId);

            await _next(context);
        }

        private string GetCorrelationId(HttpContext context)
        {
            // Try to get correlation ID from request headers
            if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
            {
                return correlationId.ToString();
            }

            // Generate new correlation ID if not present
            return Guid.NewGuid().ToString();
        }
    }

    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
} 
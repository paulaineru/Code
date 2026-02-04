using Microsoft.AspNetCore.Builder;

namespace SharedKernel.Middleware
{
    public static class JwtTokenLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtTokenLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtTokenLoggingMiddleware>();
        }
    }
} 
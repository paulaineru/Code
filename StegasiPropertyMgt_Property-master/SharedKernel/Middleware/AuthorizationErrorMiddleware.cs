using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using SharedKernel.Dto;
using System.Net;

namespace SharedKernel.Middleware
{
    public class AuthorizationErrorMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizationErrorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            // Handle authorization errors after the request has been processed
            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                context.Response.ContentType = "application/json";
                var response = ApiResponse.Unauthorized("Authentication required. Please provide a valid JWT token.");
                await context.Response.WriteAsJsonAsync(response);
            }
            else if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
            {
                context.Response.ContentType = "application/json";
                var response = ApiResponse.Forbidden("Access denied. You do not have permission to perform this action.");
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }

    public static class AuthorizationErrorMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthorizationErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthorizationErrorMiddleware>();
        }
    }
} 
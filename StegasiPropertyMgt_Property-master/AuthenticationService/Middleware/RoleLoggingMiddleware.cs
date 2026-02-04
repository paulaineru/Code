using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AuthenticationService.Middleware
{
    public class RoleLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleLoggingMiddleware> _logger;

        public RoleLoggingMiddleware(RequestDelegate next, ILogger<RoleLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", "");
                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(token);

                        var roles = jwtToken.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();

                        var userId = jwtToken.Claims
                            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                        var email = jwtToken.Claims
                            .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                        _logger.LogInformation(
                            "Token Roles - CorrelationId: {CorrelationId}, UserId: {UserId}, Email: {Email}, Roles: {Roles}",
                            correlationId,
                            userId,
                            email,
                            string.Join(", ", roles)
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing JWT token - CorrelationId: {CorrelationId}", correlationId);
                    }
                }
            }

            await _next(context);
        }
    }

    public static class RoleLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RoleLoggingMiddleware>();
        }
    }
} 
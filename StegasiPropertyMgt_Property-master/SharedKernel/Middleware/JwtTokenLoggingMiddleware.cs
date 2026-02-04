using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace SharedKernel.Middleware
{
    public class JwtTokenLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtTokenLoggingMiddleware> _logger;

        public JwtTokenLoggingMiddleware(RequestDelegate next, ILogger<JwtTokenLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                
                if (!string.IsNullOrEmpty(token))
                {
                    // Basic JWT format validation before attempting to parse
                    if (IsValidJwtFormat(token))
                    {
                        try
                        {
                            var handler = new JwtSecurityTokenHandler();
                            var jwtToken = handler.ReadJwtToken(token);

                            _logger.LogInformation("JWT Token Claims:");
                            _logger.LogInformation("Subject: {Subject}", jwtToken.Subject);
                            _logger.LogInformation("Issuer: {Issuer}", jwtToken.Issuer);
                            _logger.LogInformation("Audience: {Audience}", jwtToken.Audiences.FirstOrDefault());
                            _logger.LogInformation("Expiration: {Expiration}", jwtToken.ValidTo);

                            var roles = jwtToken.Claims
                                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                                .Select(c => c.Value);

                            if (roles.Any())
                            {
                                _logger.LogInformation("Roles: {Roles}", string.Join(", ", roles));
                            }
                            else
                            {
                                _logger.LogWarning("No roles found in the token");
                            }

                            // Log all claims for debugging
                            foreach (var claim in jwtToken.Claims)
                            {
                                _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Error parsing JWT token: {Error}", ex.Message);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Malformed JWT token format detected. Token: {TokenPrefix}...", 
                            token.Length > 20 ? token.Substring(0, 20) : token);
                    }
                }
                else
                {
                    _logger.LogWarning("Empty Bearer token found in the request");
                }
            }
            else if (!string.IsNullOrEmpty(authHeader))
            {
                _logger.LogInformation("Non-Bearer authorization header found: {AuthHeader}", 
                    authHeader.Length > 50 ? authHeader.Substring(0, 50) + "..." : authHeader);
            }
            else
            {
                _logger.LogDebug("No authorization header found in the request");
            }

            await _next(context);
        }

        private bool IsValidJwtFormat(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            // JWT tokens should have exactly 2 dots (header.payload.signature)
            var parts = token.Split('.');
            if (parts.Length != 3)
                return false;

            // Each part should be a valid base64url string
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                    return false;

                // Check if the part contains only valid base64url characters
                if (!part.All(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || 
                                   (c >= '0' && c <= '9') || c == '-' || c == '_'))
                    return false;
            }

            return true;
        }
    }
} 
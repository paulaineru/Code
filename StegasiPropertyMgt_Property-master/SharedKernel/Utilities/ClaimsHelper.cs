// SharedKernel/Utilities/ClaimsHelper.cs
using System;
using System.Linq; // Add this line for LINQ methods
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SharedKernel.Utilities
{
    public static class ClaimsHelper
    {
        /// <summary>
        /// Extracts the user ID from the authenticated user's claims.
        /// </summary>
        public static Guid GetUserIdFromClaims(HttpContext httpContext)
        {
            if (httpContext == null || !httpContext.User.Identity.IsAuthenticated)
            {
                throw new InvalidOperationException("User is not authenticated.");
            }

            var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier); // Standard claim for user ID

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new InvalidOperationException("User ID claim is missing or invalid.");
            }

            return userId;
        }
    }
}
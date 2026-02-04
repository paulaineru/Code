using SharedKernel.Models;
using SharedKernel.Services;
using SharedKernel.Dto;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace TenantManagementService.Services
{
    public interface ITenantNotificationManager
    {
        Task NotifyTenantAsync(NotificationRequest request);
    }

    public class TenantNotificationManager : ITenantNotificationManager
    {
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TenantNotificationManager> _logger;

        public TenantNotificationManager(
            INotificationService notificationService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TenantNotificationManager> logger)
        {
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task NotifyTenantAsync(NotificationRequest request)
        {
            try
            {
                // Validate request
                if (request.TenantId == Guid.Empty || string.IsNullOrEmpty(request.Message))
                    throw new ArgumentException("Invalid notification request.");

                // Fetch additional details from claims (if needed)
                var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString))
                    throw new InvalidOperationException("User ID not found in claims.");

                var userId = Guid.Parse(userIdString);

                // Use the shared NotificationService to send the notification
                var subject = request.Subject ?? "Important Notification";
                await _notificationService.SendCriticalActionNotificationAsync(
                    userId.ToString(),
                    subject,
                    request.Message,
                    "CRITICAL"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify tenant {TenantId}", request.TenantId);
                throw new InvalidOperationException("Notification failed", ex);
            }
        }
    }
}

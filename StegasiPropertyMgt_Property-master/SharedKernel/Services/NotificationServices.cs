// SharedKernel/Services/NotificationService.cs
using SharedKernel.Services;
using Nest;
using SharedKernel.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace SharedKernel.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ElasticClient _elasticClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly string _notificationServiceUrl;

        public NotificationService(IEmailService emailService, ElasticClient elasticClient, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IConfiguration configuration)
        {
            _emailService = emailService;
            _elasticClient = elasticClient;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _notificationServiceUrl = configuration["Services:NotificationService"] ?? "http://localhost:5033";
        }

        public async Task SendCriticalActionNotificationAsync(string adminEmail, string tenantEmail, string subject, string body)
        {
            try
            {
                if (!string.IsNullOrEmpty(tenantEmail))
                {
                    await _emailService.SendEmailAsync(tenantEmail, subject, body);
                }

                if (!string.IsNullOrEmpty(adminEmail))
                {
                    await _emailService.SendEmailAsync(adminEmail, subject, body);
                }

                // Log notification in Elasticsearch
                var userId = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "System";
                var log = new AuditLog
                {
                    Action = "NotificationSent",
                    UserId = Guid.TryParse(userId, out var guid) ? guid : (Guid?)null,
                    Details = $"Notification sent: Subject={subject}, Body={body}",
                    Timestamp = DateTime.UtcNow
                };
                await _elasticClient.IndexDocumentAsync(log);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send notification: {ex.Message}", ex);
            }
        }

        public async Task SendNotificationAsync(Guid userId, string title, string message, string type)
        {
            var notification = new
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type
            };

            var response = await _httpClient.PostAsJsonAsync($"{_notificationServiceUrl}/api/notifications", notification);
            response.EnsureSuccessStatusCode();
        }

        public async Task SendEmailAsync(string email, string subject, string body)
        {
            var emailRequest = new
            {
                To = email,
                Subject = subject,
                Body = body
            };

            var response = await _httpClient.PostAsJsonAsync($"{_notificationServiceUrl}/api/notifications/email", emailRequest);
            response.EnsureSuccessStatusCode();
        }

        public async Task SendSMSAsync(string phoneNumber, string message)
        {
            var smsRequest = new
            {
                To = phoneNumber,
                Message = message
            };

            var response = await _httpClient.PostAsJsonAsync($"{_notificationServiceUrl}/api/notifications/sms", smsRequest);
            response.EnsureSuccessStatusCode();
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires JWT authentication
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid notification request");
            }

            try
            {
                await _notificationService.SendNotificationAsync(
                    request.UserId,
                    request.Title,
                    request.Message,
                    request.Type
                );

                _logger.LogInformation("Notification request queued for Type: {Type}", request.Type);
                return Ok(new { message = "Notification sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification");
                return StatusCode(500, new { error = "Failed to send notification", details = ex.Message });
            }
        }

        [HttpPost("critical")]
        public async Task<IActionResult> SendCriticalNotification([FromBody] CriticalNotificationRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid notification request");
            }

            try
            {
                await _notificationService.SendCriticalActionNotificationAsync(
                    request.AdminEmail,
                    request.ManagerEmail,
                    request.Subject,
                    request.Message
                );

                return Ok(new { message = "Critical notification sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to send critical notification", details = ex.Message });
            }
        }

        [HttpPost("email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid email request");
            }

            try
            {
                await _notificationService.SendEmailAsync(
                    request.Email,
                    request.Subject,
                    request.Body
                );

                return Ok(new { message = "Email sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to send email", details = ex.Message });
            }
        }

        [HttpPost("sms")]
        public async Task<IActionResult> SendSMS([FromBody] SMSRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid SMS request");
            }

            try
            {
                await _notificationService.SendSMSAsync(
                    request.PhoneNumber,
                    request.Message
                );

                return Ok(new { message = "SMS sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to send SMS", details = ex.Message });
            }
        }
    }
}
using System;

namespace NotificationService.Models
{
    public class NotificationRequest
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }

    public class CriticalNotificationRequest
    {
        public string AdminEmail { get; set; }
        public string ManagerEmail { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public class EmailRequest
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class SMSRequest
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
    }
} 
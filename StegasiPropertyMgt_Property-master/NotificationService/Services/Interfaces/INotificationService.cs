using System;
using System.Threading.Tasks;

namespace NotificationService.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(Guid userId, string title, string message, string type);
        Task SendCriticalActionNotificationAsync(string adminEmail, string managerEmail, string subject, string message);
        Task SendEmailAsync(string email, string subject, string body);
        Task SendSMSAsync(string phoneNumber, string message);
    }
}
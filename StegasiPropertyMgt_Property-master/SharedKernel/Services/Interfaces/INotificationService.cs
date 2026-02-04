// SharedKernel/Services/INotificationService.cs
using System;
using System.Threading.Tasks;

namespace SharedKernel.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(Guid userId, string title, string message, string type);
        Task SendCriticalActionNotificationAsync(string adminEmail, string managerEmail, string subject, string message);
        Task SendEmailAsync(string email, string subject, string body);
        Task SendSMSAsync(string phoneNumber, string message);
    }
}
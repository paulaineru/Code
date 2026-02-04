using System.Threading.Tasks;
using SharedKernel.Models;

namespace BillingService.Repository
{
    public interface INotificationRepository
    {
        Task LogNotificationAsync(NotificationLog notification);
    }
} 
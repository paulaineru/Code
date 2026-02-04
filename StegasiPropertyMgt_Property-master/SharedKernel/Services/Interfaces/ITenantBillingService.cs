using SharedKernel.Models;

namespace SharedKernel.Services
{
    public interface ITenantBillingService
    {
        Task<Invoice> GenerateInvoiceForBookingAsync(Guid bookingId);
        Task<List<Invoice>> GetInvoicesByTenantAsync(Guid tenantId);
        Task UpdateBookingStatusToBilledAsync(Guid bookingId);
    }
}
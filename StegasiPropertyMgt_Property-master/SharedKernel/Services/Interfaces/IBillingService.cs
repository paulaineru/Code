// SharedKernel/Services/IBillingService.cs
using SharedKernel.Models;
using System.Threading.Tasks;
using SharedKernel.Models;
using SharedKernel.Dto;
using System.Collections.Generic;
namespace SharedKernel.Services
{
    public interface IBillingService
    {
        Task<Invoice> GenerateInvoiceAsync(Guid bookingId, Guid tenantId, GenerateInvoiceDto dto);
        Task<Invoice> GetInvoiceByIdAsync(Guid id);
        Task<Payment> MakePaymentAsync(Guid invoiceId, MakePaymentDto dto);
        Task<List<Payment>> GetPaymentsByInvoiceIdAsync(Guid invoiceId);
        Task CancelInvoiceAsync(Guid invoiceId);
    }
}
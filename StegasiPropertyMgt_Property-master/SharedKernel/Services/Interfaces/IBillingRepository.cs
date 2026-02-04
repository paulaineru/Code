// SharedKernel/Services/IBillingRepository.cs
using SharedKernel.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedKernel.Services
{
    /*public interface IBillingRepository
    {
        Task AddInvoiceAsync(Invoice invoice);
        Task<Invoice> GetInvoiceByIdAsync(Guid id);
        Task<List<Payment>> GetPaymentsByInvoiceIdAsync(Guid invoiceId);
        Task AddPaymentAsync(Payment payment);
        Task UpdateInvoiceAsync(Invoice invoice);
    }*/
    public interface IBillingRepository
    {
        //Task<Invoice> AddAsync(Invoice invoice);
        Task AddInvoiceAsync(Invoice invoice);
        Task<Invoice> GetInvoiceByIdAsync(Guid id, bool includePayments = false);
        Task<List<Payment>> GetPaymentsByInvoiceIdAsync(Guid invoiceId);
        Task AddPaymentAsync(Payment payment);
        Task UpdateInvoiceAsync(Invoice invoice);
        Task<Invoice> AddAsync(Invoice invoice);
        Task<List<Invoice>> GetInvoicesByTenantAsync(Guid tenantId);
        Task<List<Invoice>> GetInvoicesByTenantIdAsync(Guid tenantId);

    }
}
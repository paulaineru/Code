
using SharedKernel.Dto;
namespace SharedKernel.Services
{
    public interface IBillingClient
    {
        Task GenerateInvoiceAsync(GenerateInvoiceRequest dto);
        Task ProcessPaymentAsync(ProcessPaymentRequest request);
        Task<InvoiceResponse> GetInvoiceAsync(Guid invoiceId);
        Task<List<InvoiceResponse>> GetInvoicesByTenantAsync(Guid tenantId);
        
    }
}
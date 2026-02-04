// SharedKernel/Dto/GenerateInvoiceRequest.cs
using SharedKernel.Models;
namespace SharedKernel.Dto
{
    public class GenerateInvoiceRequest
    {
        public Guid BookingId { get; set; }
        public Guid TenantId { get; set; }
        public Guid PropertyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // SharedKernel/Dto/InvoiceResponse.cs
    public class InvoiceResponse
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
    }

    // SharedKernel/Enums/InvoiceStatus.cs
   
}

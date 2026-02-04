using SharedKernel.Models;
namespace SharedKernel.Dto.Tenants
{
    public class BookingPropertyDto
    {
        public Guid PropertyId { get; set; }
        public decimal DownPaymentAmount { get; set; }
        public DateTime StartDate { get; set; } // Add this
        public DateTime EndDate { get; set; }
    }
    public class BookPropertyDto
    {
        public Guid PropertyId { get; set; }
        public decimal DownPaymentAmount { get; set; }
        public DateTime StartDate { get; set; } // Add this
        public DateTime EndDate { get; set; }
    }
    public class BookPropertyRequest
    {
        public Guid PropertyId { get; set; }
        public Guid TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class UpdateBookingStatusRequest
    {
        public BookingStatus NewStatus { get; set; }
    }
}
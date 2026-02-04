using System;
using SharedKernel.Models;

namespace SharedKernel.Models
{
    public class Booking
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PropertyId { get; set; }
        public Guid TenantId { get; set; }
        public decimal DownPaymentAmount { get; set; }
        public DateTime BookedOn { get; set; } = DateTime.UtcNow;
        public BookingStatus Status { get; set; } = BookingStatus.Reserved;
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }
        // Navigation properties
        // public virtual Property Property { get; set; }
        public virtual Tenant Tenant { get; set; }
    }

    public enum BookingStatus
    {
        Reserved,
        Finalized,
        Released, Pending,
        Confirmed,
        Billed, // Add this value
        Completed,
        Cancelled
    }
}
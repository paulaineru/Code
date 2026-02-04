// SharedKernel/Models/Billing/Invoice.cs
using System;
using SharedKernel.Models;
namespace SharedKernel.Models
{
    public class Invoice
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BookingId { get; set; }
        public Guid TenantId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }

        // Navigation property for Booking
        public virtual Booking Booking { get; set; }
        public virtual List<Payment> Payments { get; set; } = new();
    }

    public enum InvoiceStatus
    {
        Pending,
        Paid,
        PartiallyPaid,
        Cancelled,
        Overdue
    }
}
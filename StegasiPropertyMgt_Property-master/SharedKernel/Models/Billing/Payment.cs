// SharedKernel/Models/Billing/Payment.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedKernel.Models;

namespace SharedKernel.Models
{
    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid InvoiceId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaidOn { get; set; } = DateTime.UtcNow;
        public PaymentStatus Status { get; set; } = PaymentStatus.Processing;
        public string PaymentMethod { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? Reference { get; set; }
        public DateTime PaymentDate { get; set; }

        // Navigation property for Invoice
        public virtual Invoice Invoice { get; set; }
    }

    public enum PaymentStatus
    {
        Processing,
        Processed,
        Successful,
        Failed
    }
}
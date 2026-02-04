using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Models.Billing
{
    public class Bill
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PropertyId { get; set; }
        public Guid TenantId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; } = false;
        public string Status { get; set; }
        public string Description { get; set; }
        public List<Payment> Payments { get; set; } = new();
        public List<CustomBillingField> CustomFields { get; set; } = new();
    }
    public class BillingDetails
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PropertyId { get; set; }
        public decimal TotalArea { get; set; }
        public decimal BillableArea { get; set; }
        public decimal BaseRate { get; set; }
        public List<CustomBillingField> CustomFields { get; set; } = new();
    }



    public class BillingSchedule
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PropertyId { get; set; } // Foreign key to Property
        public DateTime StartDate { get; set; } // Start of the financial year
        public DateTime EndDate { get; set; } // End of the financial year
        public Frequency Frequency { get; set; } // Billing frequency (e.g., Quarterly)
        public decimal BaseRate { get; set; } // Base rate per unit area
        public List<CustomBillingField> CustomFields { get; set; } = new(); // Additional custom billing fields
    }

    public enum Frequency
    {
        Monthly,
        Quarterly,
        Annually
    }
}
// SharedKernel/Models/LeaseAgreement.cs
using System;
using System.Collections.Generic;
using SharedKernel.Models;
using SharedKernel.Models.Tenants;

namespace SharedKernel.Models
{
    public class LeaseAgreement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PropertyId { get; set; }
        public Guid TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public string Terms { get; set; }
        public string Status { get; set; } // Active, Terminated, etc.
        public Guid? ApproverId { get; set; }

        // Navigation properties
       // public virtual Property Property { get; set; }
        public virtual Tenant Tenant { get; set; }
        public virtual TerminationProcess TerminationProcess { get; set; } // Optional: Link to termination process
    }
}
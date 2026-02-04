using System;

namespace SharedKernel.Models.Tenants
{
    public class RenewalRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid LeaseAgreementId { get; set; }
        public Guid TenantId { get; set; }
        public string NewTerms { get; set; }
        public decimal? NewMonthlyRent { get; set; }
        public RenewalStatus Status { get; set; } = RenewalStatus.Pending;
        public virtual Tenant Tenant { get; set; } // Add this line
    }

    public enum RenewalStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
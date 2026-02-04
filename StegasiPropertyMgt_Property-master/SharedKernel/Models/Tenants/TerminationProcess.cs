using System;

namespace SharedKernel.Models.Tenants
{
    public class TerminationProcess
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid LeaseAgreementId { get; set; }
        public Guid TenantId { get; set; }
        public DateTime InitiatedOn { get; set; } = DateTime.UtcNow;
        public TerminationStatus Status { get; set; } = TerminationStatus.Initiated;
        
        public decimal OutstandingAmount { get; set; }
        public decimal SecurityDepositDeduction { get; set; }

        // Navigation properties
        public virtual Tenant Tenant { get; set; } // Add this line
        public virtual LeaseAgreement LeaseAgreement { get; set; } // Add this line
        public List<InspectionReport> InspectionReports { get; set; } = new();
    }

    public enum TerminationStatus
    {
        Initiated,
        Inspected,
        Finalized
    }

    public class InspectionReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TerminationProcessId { get; set; }
        public string ReportDetails { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual TerminationProcess TerminationProcess { get; set; }
    }
}
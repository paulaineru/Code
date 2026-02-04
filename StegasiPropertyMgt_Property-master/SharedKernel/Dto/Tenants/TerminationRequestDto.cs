namespace SharedKernel.Dto.Tenants
{
    public class TerminationRequestDto
    {
        public Guid PropertyId { get; set; }
        
        public string Reason { get; set; } // Reason for termination
        public decimal OutstandingAmount { get; set; } // Outstanding amount to be settled
        public decimal SecurityDepositDeduction { get; set; } // Deduction from security deposit
        public Guid LeaseAgreementId { get; set; } // Add this field to reference the lease agreement

    }
}
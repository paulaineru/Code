namespace SharedKernel.Dto.Tenants
{
    public class SubmitRenewalRequestDto
    {
        public Guid PropertyId { get; set; } // Add this field to reference the property
        public Guid TenantId { get; set; }
        public string NewTerms { get; set; }
        public decimal? NewMonthlyRent { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Dto
{
    public class LeaseResponse
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public Guid TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Terms { get; set; }
        public string Status { get; set; } // Lease status (e.g., "Active", "Expired")
        public Guid? ApproverId { get; set; }
    }


    public class CreateLeaseRequest
    {
        [Required]
        public Guid PropertyId { get; set; } // Property being leased

        [Required]
        public Guid TenantId { get; set; } // Tenant leasing the property

        [Required]
        public DateTime StartDate { get; set; } // Lease start date

        [Required]
        public DateTime EndDate { get; set; } // Lease end date

        public string Terms { get; set; } // Optional: Lease terms
    }
    public class UpdateLeaseStatusRequest
    {
        [Required]
        public string NewStatus { get; set; }
    }
}
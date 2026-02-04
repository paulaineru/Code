using System;
using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Dto
{
    public class ApproveLeaseRequest
    {
        [Required]
        public string ApprovalStatus { get; set; } // e.g., "Approved", "Rejected"

        public Guid? ApproverId { get; set; } // Optional: ID of the approver
    }
}
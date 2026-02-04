using System;
using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Models
{
    public class PropertyDocument
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid DocumentTypeId { get; set; }
        
        [Required]
        public Guid DocumentCategoryId { get; set; }
        
        public Guid? DocumentSubCategoryId { get; set; }
        
        // Entity associations - at least one should be set
        public Guid? PropertyId { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? LeaseId { get; set; }
        public Guid? MaintenanceTicketId { get; set; }
        public Guid? BookingId { get; set; }
        public Guid? RenewalRequestId { get; set; }
        public Guid? TerminationProcessId { get; set; }
        public Guid? BillId { get; set; }
        public Guid? PaymentId { get; set; }
        
        // File information
        [Required]
        public string FileName { get; set; }
        
        [Required]
        public string FileUrl { get; set; }
        
        [Required]
        public string ContentType { get; set; }
        
        public long FileSize { get; set; }
        
        // Metadata
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public Guid UploadedBy { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        public bool IsRequired { get; set; } = false;
        
        public bool IsVerified { get; set; } = false;
        
        public DateTime? VerifiedAt { get; set; }
        
        public Guid? VerifiedBy { get; set; }
        
        [Required]
        public string Status { get; set; } = "Active"; // Active, Expired, Archived, Pending, Rejected
        
        public string Version { get; set; } = "1.0";
        
        public string? Tags { get; set; } // JSON array for searchable tags
        
        public string? Description { get; set; }
        
        public string? Comments { get; set; }
        
        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual DocumentType DocumentType { get; set; } = null!;
        public virtual DocumentCategory DocumentCategory { get; set; } = null!;
        public virtual DocumentSubCategory? DocumentSubCategory { get; set; }
        public virtual Property? Property { get; set; }
        public virtual Tenant? Tenant { get; set; }
        public virtual LeaseAgreement? LeaseAgreement { get; set; }
    }
} 
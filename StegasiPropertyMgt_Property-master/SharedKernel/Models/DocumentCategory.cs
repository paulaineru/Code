using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Models
{
    public class DocumentType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // Property, Tenant, Lease, Maintenance, etc.
        
        [MaxLength(200)]
        public string? Description { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        public int DisplayOrder { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual ICollection<DocumentCategory> Categories { get; set; } = new List<DocumentCategory>();
        public virtual ICollection<PropertyDocument> Documents { get; set; } = new List<PropertyDocument>();
    }

    public class DocumentCategory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid DocumentTypeId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Ownership, Construction, Application, etc.
        
        [MaxLength(200)]
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string? Code { get; set; } // Short code for programmatic access
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        public bool IsRequired { get; set; } = false;
        
        public int DisplayOrder { get; set; } = 0;
        
        [MaxLength(1000)]
        public string? ValidationRules { get; set; } // JSON for validation rules
        
        public int? ExpiryWarningDays { get; set; } // Days before expiry to warn
        
        [MaxLength(500)]
        public string? AllowedFileTypes { get; set; } // Comma-separated file extensions
        
        public long? MaxFileSizeInBytes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual DocumentType DocumentType { get; set; } = null!;
        public virtual ICollection<DocumentSubCategory> SubCategories { get; set; } = new List<DocumentSubCategory>();
        public virtual ICollection<PropertyDocument> Documents { get; set; } = new List<PropertyDocument>();
    }

    public class DocumentSubCategory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid DocumentCategoryId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(200)]
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string? Code { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        public bool IsRequired { get; set; } = false;
        
        public int DisplayOrder { get; set; } = 0;
        
        [MaxLength(1000)]
        public string? ValidationRules { get; set; }
        
        public int? ExpiryWarningDays { get; set; }
        
        [MaxLength(500)]
        public string? AllowedFileTypes { get; set; }
        
        public long? MaxFileSizeInBytes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual DocumentCategory DocumentCategory { get; set; } = null!;
        public virtual ICollection<PropertyDocument> Documents { get; set; } = new List<PropertyDocument>();
    }

    public class DocumentRequirement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(100)]
        public string ProcessType { get; set; } // LeaseCreation, TenantApplication, PropertyRegistration, etc.
        
        [MaxLength(100)]
        public string? SubProcess { get; set; }
        
        [Required]
        public Guid DocumentTypeId { get; set; }
        
        [Required]
        public Guid DocumentCategoryId { get; set; }
        
        public Guid? DocumentSubCategoryId { get; set; }
        
        [Required]
        public bool IsRequired { get; set; } = false;
        
        public int DisplayOrder { get; set; } = 0;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(1000)]
        public string? ValidationRules { get; set; }
        
        public int? ExpiryWarningDays { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual DocumentType DocumentType { get; set; } = null!;
        public virtual DocumentCategory DocumentCategory { get; set; } = null!;
        public virtual DocumentSubCategory? DocumentSubCategory { get; set; }
    }

    // Static helper class for common document types (for backward compatibility)
    public static class DocumentTypes
    {
        public const string Property = "Property";
        public const string Tenant = "Tenant";
        public const string Lease = "Lease";
        public const string Maintenance = "Maintenance";
        public const string Financial = "Financial";
        public const string Legal = "Legal";
        public const string Insurance = "Insurance";
        public const string Compliance = "Compliance";
        public const string Marketing = "Marketing";
        public const string Operations = "Operations";
        public const string HR = "HR";
    }

    public static class DocumentStatus
    {
        public const string Active = "Active";
        public const string Expired = "Expired";
        public const string Archived = "Archived";
        public const string Pending = "Pending";
        public const string Rejected = "Rejected";
        public const string UnderReview = "UnderReview";
    }
} 
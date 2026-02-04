using System;
using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Dto
{
    // Document upload request
    public class DocumentUploadRequest
    {
        [Required]
        public Guid DocumentTypeId { get; set; }
        
        [Required]
        public Guid DocumentCategoryId { get; set; }
        
        public Guid? DocumentSubCategoryId { get; set; }
        
        // Entity associations - at least one should be provided
        public Guid? PropertyId { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? LeaseId { get; set; }
        public Guid? MaintenanceTicketId { get; set; }
        public Guid? BookingId { get; set; }
        public Guid? RenewalRequestId { get; set; }
        public Guid? TerminationProcessId { get; set; }
        public Guid? BillId { get; set; }
        public Guid? PaymentId { get; set; }
        
        public bool IsRequired { get; set; } = false;
        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
        public string? Tags { get; set; } // JSON array
        public string? Comments { get; set; }
    }

    // Document response
    public class DocumentResponse
    {
        public Guid Id { get; set; }
        public Guid DocumentTypeId { get; set; }
        public Guid DocumentCategoryId { get; set; }
        public Guid? DocumentSubCategoryId { get; set; }
        
        // Document type and category names for display
        public string DocumentTypeName { get; set; } = string.Empty;
        public string DocumentCategoryName { get; set; } = string.Empty;
        public string? DocumentSubCategoryName { get; set; }
        
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public Guid UploadedBy { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsRequired { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedBy { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string? Tags { get; set; }
        public string? Description { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Entity associations
        public Guid? PropertyId { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? LeaseId { get; set; }
        public Guid? MaintenanceTicketId { get; set; }
        public Guid? BookingId { get; set; }
        public Guid? RenewalRequestId { get; set; }
        public Guid? TerminationProcessId { get; set; }
        public Guid? BillId { get; set; }
        public Guid? PaymentId { get; set; }
    }

    // Document search request
    public class DocumentSearchRequest
    {
        public string? SearchTerm { get; set; }
        public Guid? DocumentTypeId { get; set; }
        public Guid? DocumentCategoryId { get; set; }
        public Guid? DocumentSubCategoryId { get; set; }
        public string? Status { get; set; }
        public Guid? PropertyId { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? LeaseId { get; set; }
        public Guid? MaintenanceTicketId { get; set; }
        public Guid? BookingId { get; set; }
        public Guid? RenewalRequestId { get; set; }
        public Guid? TerminationProcessId { get; set; }
        public Guid? BillId { get; set; }
        public Guid? PaymentId { get; set; }
        public bool? IsRequired { get; set; }
        public bool? IsVerified { get; set; }
        public DateTime? UploadedFrom { get; set; }
        public DateTime? UploadedTo { get; set; }
        public DateTime? ExpiresFrom { get; set; }
        public DateTime? ExpiresTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Document verification request
    public class DocumentVerificationRequest
    {
        [Required]
        public Guid DocumentId { get; set; }
        
        [Required]
        public bool IsVerified { get; set; }
        
        public string? Comments { get; set; }
    }

    // Document update request
    public class DocumentUpdateRequest
    {
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public string? Comments { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool? IsRequired { get; set; }
        public string? Status { get; set; }
    }

    // Document type management
    public class DocumentTypeRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }

    public class DocumentTypeResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CategoryCount { get; set; }
        public int DocumentCount { get; set; }
    }

    // Document category management
    public class DocumentCategoryRequest
    {
        [Required]
        public Guid DocumentTypeId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string? Code { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool IsRequired { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public string? ValidationRules { get; set; }
        public int? ExpiryWarningDays { get; set; }
        public string? AllowedFileTypes { get; set; }
        public long? MaxFileSizeInBytes { get; set; }
    }

    public class DocumentCategoryResponse
    {
        public Guid Id { get; set; }
        public Guid DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public string? ValidationRules { get; set; }
        public int? ExpiryWarningDays { get; set; }
        public string? AllowedFileTypes { get; set; }
        public long? MaxFileSizeInBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int SubCategoryCount { get; set; }
        public int DocumentCount { get; set; }
    }

    // Document sub-category management
    public class DocumentSubCategoryRequest
    {
        [Required]
        public Guid DocumentCategoryId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string? Code { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool IsRequired { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public string? ValidationRules { get; set; }
        public int? ExpiryWarningDays { get; set; }
        public string? AllowedFileTypes { get; set; }
        public long? MaxFileSizeInBytes { get; set; }
    }

    public class DocumentSubCategoryResponse
    {
        public Guid Id { get; set; }
        public Guid DocumentCategoryId { get; set; }
        public string DocumentCategoryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public string? ValidationRules { get; set; }
        public int? ExpiryWarningDays { get; set; }
        public string? AllowedFileTypes { get; set; }
        public long? MaxFileSizeInBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int DocumentCount { get; set; }
    }

    // Document requirements for different processes
    public class DocumentRequirement
    {
        public Guid Id { get; set; }
        public string ProcessType { get; set; } = string.Empty;
        public string? SubProcess { get; set; }
        public Guid DocumentTypeId { get; set; }
        public Guid DocumentCategoryId { get; set; }
        public Guid? DocumentSubCategoryId { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public string DocumentCategoryName { get; set; } = string.Empty;
        public string? DocumentSubCategoryName { get; set; }
        public bool IsRequired { get; set; }
        public string? Description { get; set; }
        public string? ValidationRules { get; set; }
        public int? ExpiryWarningDays { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    // Document completeness check response
    public class DocumentCompletenessResponse
    {
        public bool IsComplete { get; set; }
        public List<DocumentRequirement> RequiredDocuments { get; set; } = new();
        public List<DocumentRequirement> MissingDocuments { get; set; } = new();
        public List<DocumentRequirement> ExpiredDocuments { get; set; } = new();
        public List<DocumentRequirement> PendingVerification { get; set; } = new();
    }

    // Document statistics
    public class DocumentStatistics
    {
        public int TotalDocuments { get; set; }
        public int ActiveDocuments { get; set; }
        public int ExpiredDocuments { get; set; }
        public int PendingVerification { get; set; }
        public int RequiredDocuments { get; set; }
        public long TotalStorageUsed { get; set; } // in bytes
        public Dictionary<string, int> DocumentsByType { get; set; } = new();
        public Dictionary<string, int> DocumentsByCategory { get; set; } = new();
    }

    // Document hierarchy response
    public class DocumentHierarchyResponse
    {
        public List<DocumentTypeResponse> DocumentTypes { get; set; } = new();
    }
} 
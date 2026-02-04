using SharedKernel.Dto;
using SharedKernel.Models;

namespace SharedKernel.Services.Interfaces
{
    public interface IDocumentRepository
    {
        // Document CRUD operations
        Task<PropertyDocument> CreateDocumentAsync(PropertyDocument document);
        Task<PropertyDocument?> GetDocumentByIdAsync(Guid documentId);
        Task<List<PropertyDocument>> GetDocumentsAsync(DocumentSearchRequest request);
        Task<PropertyDocument> UpdateDocumentAsync(PropertyDocument document);
        Task<bool> DeleteDocumentAsync(Guid documentId);
        
        // Document type management
        Task<DocumentType> CreateDocumentTypeAsync(DocumentType documentType);
        Task<DocumentType?> GetDocumentTypeByIdAsync(Guid documentTypeId);
        Task<List<DocumentType>> GetAllDocumentTypesAsync();
        Task<DocumentType> UpdateDocumentTypeAsync(DocumentType documentType);
        Task<bool> DeleteDocumentTypeAsync(Guid documentTypeId);
        Task<bool> IsDocumentTypeExistsAsync(Guid documentTypeId);
        Task<string> GetDocumentTypeNameAsync(Guid documentTypeId);
        
        // Document category management
        Task<DocumentCategory> CreateDocumentCategoryAsync(DocumentCategory category);
        Task<DocumentCategory?> GetDocumentCategoryByIdAsync(Guid categoryId);
        Task<List<DocumentCategory>> GetCategoriesByTypeAsync(Guid documentTypeId);
        Task<DocumentCategory> UpdateDocumentCategoryAsync(DocumentCategory category);
        Task<bool> DeleteDocumentCategoryAsync(Guid categoryId);
        Task<bool> IsDocumentCategoryExistsAsync(Guid categoryId);
        Task<string> GetDocumentCategoryNameAsync(Guid categoryId);
        
        // Document sub-category management
        Task<DocumentSubCategory> CreateDocumentSubCategoryAsync(DocumentSubCategory subCategory);
        Task<DocumentSubCategory?> GetDocumentSubCategoryByIdAsync(Guid subCategoryId);
        Task<List<DocumentSubCategory>> GetSubCategoriesByCategoryAsync(Guid categoryId);
        Task<DocumentSubCategory> UpdateDocumentSubCategoryAsync(DocumentSubCategory subCategory);
        Task<bool> DeleteDocumentSubCategoryAsync(Guid subCategoryId);
        Task<bool> IsDocumentSubCategoryExistsAsync(Guid subCategoryId);
        Task<string?> GetDocumentSubCategoryNameAsync(Guid subCategoryId);
        
        // Document requirements management
        Task<SharedKernel.Models.DocumentRequirement> CreateDocumentRequirementAsync(SharedKernel.Models.DocumentRequirement requirement);
        Task<SharedKernel.Models.DocumentRequirement?> GetDocumentRequirementByIdAsync(Guid requirementId);
        Task<List<SharedKernel.Models.DocumentRequirement>> GetDocumentRequirementsAsync(string processType, string? subProcess = null);
        Task<SharedKernel.Models.DocumentRequirement> UpdateDocumentRequirementAsync(SharedKernel.Models.DocumentRequirement requirement);
        Task<bool> DeleteDocumentRequirementAsync(Guid requirementId);
        
        // Document statistics and reporting
        Task<int> GetDocumentCountAsync();
        Task<int> GetDocumentCountByTypeAsync(Guid documentTypeId);
        Task<int> GetDocumentCountByCategoryAsync(Guid categoryId);
        Task<int> GetDocumentCountBySubCategoryAsync(Guid subCategoryId);
        Task<long> GetTotalStorageUsedAsync();
        Task<Dictionary<string, int>> GetDocumentsByTypeStatisticsAsync();
        Task<Dictionary<string, int>> GetDocumentsByCategoryStatisticsAsync();
        
        // Document validation and completeness
        Task<List<PropertyDocument>> GetExpiringDocumentsAsync(int daysThreshold);
        Task<List<PropertyDocument>> GetDocumentsRequiringVerificationAsync();
        Task<List<PropertyDocument>> GetDocumentsByEntityAsync(Guid entityId, string entityType);
        Task<List<PropertyDocument>> GetRequiredDocumentsAsync(Guid entityId, string processType, string? subProcess = null);
        
        // Utility methods
        Task<bool> IsDocumentExistsAsync(Guid documentId);
        Task<List<PropertyDocument>> GetDocumentsByUserAsync(Guid userId);
        Task<List<PropertyDocument>> GetDocumentsByDateRangeAsync(DateTime fromDate, DateTime toDate);
    }
} 
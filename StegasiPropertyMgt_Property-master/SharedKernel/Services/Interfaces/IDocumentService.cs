using SharedKernel.Dto;
using SharedKernel.Models;
using Microsoft.AspNetCore.Http;

namespace SharedKernel.Services.Interfaces
{
    public interface IDocumentService
    {
        // Document CRUD operations
        Task<DocumentResponse> UploadDocumentAsync(DocumentUploadRequest request, IFormFile file);
        Task<DocumentResponse> GetDocumentByIdAsync(Guid documentId);
        Task<List<DocumentResponse>> GetDocumentsAsync(DocumentSearchRequest request);
        Task<DocumentResponse> UpdateDocumentAsync(Guid documentId, DocumentUpdateRequest request);
        Task<bool> DeleteDocumentAsync(Guid documentId);
        Task<DocumentResponse> VerifyDocumentAsync(DocumentVerificationRequest request);
        
        // Document type management
        Task<DocumentTypeResponse> CreateDocumentTypeAsync(DocumentTypeRequest request);
        Task<DocumentTypeResponse> GetDocumentTypeByIdAsync(Guid documentTypeId);
        Task<List<DocumentTypeResponse>> GetAllDocumentTypesAsync();
        Task<DocumentTypeResponse> UpdateDocumentTypeAsync(Guid documentTypeId, DocumentTypeRequest request);
        Task<bool> DeleteDocumentTypeAsync(Guid documentTypeId);
        
        // Document category management
        Task<DocumentCategoryResponse> CreateDocumentCategoryAsync(DocumentCategoryRequest request);
        Task<DocumentCategoryResponse> GetDocumentCategoryByIdAsync(Guid categoryId);
        Task<List<DocumentCategoryResponse>> GetCategoriesByTypeAsync(Guid documentTypeId);
        Task<DocumentCategoryResponse> UpdateDocumentCategoryAsync(Guid categoryId, DocumentCategoryRequest request);
        Task<bool> DeleteDocumentCategoryAsync(Guid categoryId);
        
        // Document sub-category management
        Task<DocumentSubCategoryResponse> CreateDocumentSubCategoryAsync(DocumentSubCategoryRequest request);
        Task<DocumentSubCategoryResponse> GetDocumentSubCategoryByIdAsync(Guid subCategoryId);
        Task<List<DocumentSubCategoryResponse>> GetSubCategoriesByCategoryAsync(Guid categoryId);
        Task<DocumentSubCategoryResponse> UpdateDocumentSubCategoryAsync(Guid subCategoryId, DocumentSubCategoryRequest request);
        Task<bool> DeleteDocumentSubCategoryAsync(Guid subCategoryId);
        
        // Document requirements management
        Task<SharedKernel.Dto.DocumentRequirement> CreateDocumentRequirementAsync(SharedKernel.Dto.DocumentRequirement request);
        Task<List<SharedKernel.Dto.DocumentRequirement>> GetDocumentRequirementsAsync(string processType, string? subProcess = null);
        Task<SharedKernel.Dto.DocumentRequirement> UpdateDocumentRequirementAsync(Guid requirementId, SharedKernel.Dto.DocumentRequirement request);
        Task<bool> DeleteDocumentRequirementAsync(Guid requirementId);
        
        // Document completeness and validation
        Task<DocumentCompletenessResponse> CheckDocumentCompletenessAsync(string processType, string? subProcess, Guid entityId);
        Task<bool> ValidateDocumentAsync(Guid documentId);
        Task<List<DocumentResponse>> GetExpiringDocumentsAsync(int daysThreshold = 30);
        Task<List<DocumentResponse>> GetDocumentsRequiringVerificationAsync();
        
        // Document statistics and reporting
        Task<DocumentStatistics> GetDocumentStatisticsAsync();
        Task<DocumentHierarchyResponse> GetDocumentHierarchyAsync();
        
        // Utility methods
        Task<bool> IsDocumentTypeExistsAsync(Guid documentTypeId);
        Task<bool> IsDocumentCategoryExistsAsync(Guid categoryId);
        Task<bool> IsDocumentSubCategoryExistsAsync(Guid subCategoryId);
        Task<string> GetDocumentTypeNameAsync(Guid documentTypeId);
        Task<string> GetDocumentCategoryNameAsync(Guid categoryId);
        Task<string?> GetDocumentSubCategoryNameAsync(Guid subCategoryId);
    }
} 
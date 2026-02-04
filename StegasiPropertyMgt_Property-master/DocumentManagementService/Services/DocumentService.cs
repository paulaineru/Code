using Microsoft.AspNetCore.Http;
using SharedKernel.Dto;
using SharedKernel.Models;
using SharedKernel.Services.Interfaces;
using DocumentManagementService.Repository;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.Text.Json;

namespace DocumentManagementService.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;

        public DocumentService(IDocumentRepository documentRepository, IAmazonS3 s3Client, IConfiguration configuration)
        {
            _documentRepository = documentRepository;
            _s3Client = s3Client;
            _configuration = configuration;
            _bucketName = _configuration["AWS:S3:BucketName"] ?? "stegasi-documents";
        }

        // Document CRUD operations
        public async Task<DocumentResponse> UploadDocumentAsync(DocumentUploadRequest request, IFormFile file)
        {
            // Validate file
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required");

            // Validate document type and category
            if (!await _documentRepository.IsDocumentTypeExistsAsync(request.DocumentTypeId))
                throw new ArgumentException("Invalid document type");

            if (!await _documentRepository.IsDocumentCategoryExistsAsync(request.DocumentCategoryId))
                throw new ArgumentException("Invalid document category");

            if (request.DocumentSubCategoryId.HasValue && 
                !await _documentRepository.IsDocumentSubCategoryExistsAsync(request.DocumentSubCategoryId.Value))
                throw new ArgumentException("Invalid document sub-category");

            // Upload to S3
            var fileName = await UploadToS3Async(file, request);
            
            // Create document record
            var document = new PropertyDocument
            {
                DocumentTypeId = request.DocumentTypeId,
                DocumentCategoryId = request.DocumentCategoryId,
                DocumentSubCategoryId = request.DocumentSubCategoryId,
                PropertyId = request.PropertyId,
                TenantId = request.TenantId,
                LeaseId = request.LeaseId,
                MaintenanceTicketId = request.MaintenanceTicketId,
                BookingId = request.BookingId,
                RenewalRequestId = request.RenewalRequestId,
                TerminationProcessId = request.TerminationProcessId,
                BillId = request.BillId,
                PaymentId = request.PaymentId,
                FileName = file.FileName,
                FileUrl = fileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = Guid.NewGuid(), // TODO: Get from JWT token
                ExpiryDate = request.ExpiryDate,
                IsRequired = request.IsRequired,
                IsVerified = false,
                Status = "Active",
                Version = "1.0",
                Tags = request.Tags,
                Description = request.Description,
                Comments = request.Comments
            };

            var createdDocument = await _documentRepository.CreateDocumentAsync(document);
            return await MapToDocumentResponseAsync(createdDocument);
        }

        public async Task<DocumentResponse> GetDocumentByIdAsync(Guid documentId)
        {
            var document = await _documentRepository.GetDocumentByIdAsync(documentId);
            if (document == null)
                throw new ArgumentException("Document not found");

            return await MapToDocumentResponseAsync(document);
        }

        public async Task<List<DocumentResponse>> GetDocumentsAsync(DocumentSearchRequest request)
        {
            var documents = await _documentRepository.GetDocumentsAsync(request);
            var responses = new List<DocumentResponse>();

            foreach (var document in documents)
            {
                responses.Add(await MapToDocumentResponseAsync(document));
            }

            return responses;
        }

        public async Task<DocumentResponse> UpdateDocumentAsync(Guid documentId, DocumentUpdateRequest request)
        {
            var document = await _documentRepository.GetDocumentByIdAsync(documentId);
            if (document == null)
                throw new ArgumentException("Document not found");

            if (!string.IsNullOrEmpty(request.Description))
                document.Description = request.Description;

            if (!string.IsNullOrEmpty(request.Tags))
                document.Tags = request.Tags;

            if (!string.IsNullOrEmpty(request.Comments))
                document.Comments = request.Comments;

            if (request.ExpiryDate.HasValue)
                document.ExpiryDate = request.ExpiryDate;

            if (request.IsRequired.HasValue)
                document.IsRequired = request.IsRequired.Value;

            if (!string.IsNullOrEmpty(request.Status))
                document.Status = request.Status;

            var updatedDocument = await _documentRepository.UpdateDocumentAsync(document);
            return await MapToDocumentResponseAsync(updatedDocument);
        }

        public async Task<bool> DeleteDocumentAsync(Guid documentId)
        {
            return await _documentRepository.DeleteDocumentAsync(documentId);
        }

        public async Task<DocumentResponse> VerifyDocumentAsync(DocumentVerificationRequest request)
        {
            var document = await _documentRepository.GetDocumentByIdAsync(request.DocumentId);
            if (document == null)
                throw new ArgumentException("Document not found");

            document.IsVerified = request.IsVerified;
            document.VerifiedAt = request.IsVerified ? DateTime.UtcNow : null;
            document.VerifiedBy = request.IsVerified ? Guid.NewGuid() : null; // TODO: Get from JWT token
            document.Comments = request.Comments;

            var updatedDocument = await _documentRepository.UpdateDocumentAsync(document);
            return await MapToDocumentResponseAsync(updatedDocument);
        }

        // Document type management
        public async Task<DocumentTypeResponse> CreateDocumentTypeAsync(DocumentTypeRequest request)
        {
            var documentType = new DocumentType
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            };

            var createdType = await _documentRepository.CreateDocumentTypeAsync(documentType);
            return MapToDocumentTypeResponse(createdType);
        }

        public async Task<DocumentTypeResponse> GetDocumentTypeByIdAsync(Guid documentTypeId)
        {
            var documentType = await _documentRepository.GetDocumentTypeByIdAsync(documentTypeId);
            if (documentType == null)
                throw new ArgumentException("Document type not found");

            return MapToDocumentTypeResponse(documentType);
        }

        public async Task<List<DocumentTypeResponse>> GetAllDocumentTypesAsync()
        {
            var documentTypes = await _documentRepository.GetAllDocumentTypesAsync();
            return documentTypes.Select(MapToDocumentTypeResponse).ToList();
        }

        public async Task<DocumentTypeResponse> UpdateDocumentTypeAsync(Guid documentTypeId, DocumentTypeRequest request)
        {
            var documentType = await _documentRepository.GetDocumentTypeByIdAsync(documentTypeId);
            if (documentType == null)
                throw new ArgumentException("Document type not found");

            documentType.Name = request.Name;
            documentType.Description = request.Description;
            documentType.IsActive = request.IsActive;
            documentType.DisplayOrder = request.DisplayOrder;
            documentType.UpdatedAt = DateTime.UtcNow;

            var updatedType = await _documentRepository.UpdateDocumentTypeAsync(documentType);
            return MapToDocumentTypeResponse(updatedType);
        }

        public async Task<bool> DeleteDocumentTypeAsync(Guid documentTypeId)
        {
            return await _documentRepository.DeleteDocumentTypeAsync(documentTypeId);
        }

        // Document category management
        public async Task<DocumentCategoryResponse> CreateDocumentCategoryAsync(DocumentCategoryRequest request)
        {
            if (!await _documentRepository.IsDocumentTypeExistsAsync(request.DocumentTypeId))
                throw new ArgumentException("Invalid document type");

            var category = new DocumentCategory
            {
                DocumentTypeId = request.DocumentTypeId,
                Name = request.Name,
                Description = request.Description,
                Code = request.Code,
                IsActive = request.IsActive,
                IsRequired = request.IsRequired,
                DisplayOrder = request.DisplayOrder,
                ValidationRules = request.ValidationRules,
                ExpiryWarningDays = request.ExpiryWarningDays,
                AllowedFileTypes = request.AllowedFileTypes,
                MaxFileSizeInBytes = request.MaxFileSizeInBytes,
                CreatedAt = DateTime.UtcNow
            };

            var createdCategory = await _documentRepository.CreateDocumentCategoryAsync(category);
            return await MapToDocumentCategoryResponseAsync(createdCategory);
        }

        public async Task<DocumentCategoryResponse> GetDocumentCategoryByIdAsync(Guid categoryId)
        {
            var category = await _documentRepository.GetDocumentCategoryByIdAsync(categoryId);
            if (category == null)
                throw new ArgumentException("Document category not found");

            return await MapToDocumentCategoryResponseAsync(category);
        }

        public async Task<List<DocumentCategoryResponse>> GetCategoriesByTypeAsync(Guid documentTypeId)
        {
            var categories = await _documentRepository.GetCategoriesByTypeAsync(documentTypeId);
            var responses = new List<DocumentCategoryResponse>();

            foreach (var category in categories)
            {
                responses.Add(await MapToDocumentCategoryResponseAsync(category));
            }

            return responses;
        }

        public async Task<DocumentCategoryResponse> UpdateDocumentCategoryAsync(Guid categoryId, DocumentCategoryRequest request)
        {
            var category = await _documentRepository.GetDocumentCategoryByIdAsync(categoryId);
            if (category == null)
                throw new ArgumentException("Document category not found");

            if (!await _documentRepository.IsDocumentTypeExistsAsync(request.DocumentTypeId))
                throw new ArgumentException("Invalid document type");

            category.DocumentTypeId = request.DocumentTypeId;
            category.Name = request.Name;
            category.Description = request.Description;
            category.Code = request.Code;
            category.IsActive = request.IsActive;
            category.IsRequired = request.IsRequired;
            category.DisplayOrder = request.DisplayOrder;
            category.ValidationRules = request.ValidationRules;
            category.ExpiryWarningDays = request.ExpiryWarningDays;
            category.AllowedFileTypes = request.AllowedFileTypes;
            category.MaxFileSizeInBytes = request.MaxFileSizeInBytes;
            category.UpdatedAt = DateTime.UtcNow;

            var updatedCategory = await _documentRepository.UpdateDocumentCategoryAsync(category);
            return await MapToDocumentCategoryResponseAsync(updatedCategory);
        }

        public async Task<bool> DeleteDocumentCategoryAsync(Guid categoryId)
        {
            return await _documentRepository.DeleteDocumentCategoryAsync(categoryId);
        }

        // Document sub-category management
        public async Task<DocumentSubCategoryResponse> CreateDocumentSubCategoryAsync(DocumentSubCategoryRequest request)
        {
            if (!await _documentRepository.IsDocumentCategoryExistsAsync(request.DocumentCategoryId))
                throw new ArgumentException("Invalid document category");

            var subCategory = new DocumentSubCategory
            {
                DocumentCategoryId = request.DocumentCategoryId,
                Name = request.Name,
                Description = request.Description,
                Code = request.Code,
                IsActive = request.IsActive,
                IsRequired = request.IsRequired,
                DisplayOrder = request.DisplayOrder,
                ValidationRules = request.ValidationRules,
                ExpiryWarningDays = request.ExpiryWarningDays,
                AllowedFileTypes = request.AllowedFileTypes,
                MaxFileSizeInBytes = request.MaxFileSizeInBytes,
                CreatedAt = DateTime.UtcNow
            };

            var createdSubCategory = await _documentRepository.CreateDocumentSubCategoryAsync(subCategory);
            return await MapToDocumentSubCategoryResponseAsync(createdSubCategory);
        }

        public async Task<DocumentSubCategoryResponse> GetDocumentSubCategoryByIdAsync(Guid subCategoryId)
        {
            var subCategory = await _documentRepository.GetDocumentSubCategoryByIdAsync(subCategoryId);
            if (subCategory == null)
                throw new ArgumentException("Document sub-category not found");

            return await MapToDocumentSubCategoryResponseAsync(subCategory);
        }

        public async Task<List<DocumentSubCategoryResponse>> GetSubCategoriesByCategoryAsync(Guid categoryId)
        {
            var subCategories = await _documentRepository.GetSubCategoriesByCategoryAsync(categoryId);
            var responses = new List<DocumentSubCategoryResponse>();

            foreach (var subCategory in subCategories)
            {
                responses.Add(await MapToDocumentSubCategoryResponseAsync(subCategory));
            }

            return responses;
        }

        public async Task<DocumentSubCategoryResponse> UpdateDocumentSubCategoryAsync(Guid subCategoryId, DocumentSubCategoryRequest request)
        {
            var subCategory = await _documentRepository.GetDocumentSubCategoryByIdAsync(subCategoryId);
            if (subCategory == null)
                throw new ArgumentException("Document sub-category not found");

            if (!await _documentRepository.IsDocumentCategoryExistsAsync(request.DocumentCategoryId))
                throw new ArgumentException("Invalid document category");

            subCategory.DocumentCategoryId = request.DocumentCategoryId;
            subCategory.Name = request.Name;
            subCategory.Description = request.Description;
            subCategory.Code = request.Code;
            subCategory.IsActive = request.IsActive;
            subCategory.IsRequired = request.IsRequired;
            subCategory.DisplayOrder = request.DisplayOrder;
            subCategory.ValidationRules = request.ValidationRules;
            subCategory.ExpiryWarningDays = request.ExpiryWarningDays;
            subCategory.AllowedFileTypes = request.AllowedFileTypes;
            subCategory.MaxFileSizeInBytes = request.MaxFileSizeInBytes;
            subCategory.UpdatedAt = DateTime.UtcNow;

            var updatedSubCategory = await _documentRepository.UpdateDocumentSubCategoryAsync(subCategory);
            return await MapToDocumentSubCategoryResponseAsync(updatedSubCategory);
        }

        public async Task<bool> DeleteDocumentSubCategoryAsync(Guid subCategoryId)
        {
            return await _documentRepository.DeleteDocumentSubCategoryAsync(subCategoryId);
        }

        // Document requirements management
        public async Task<SharedKernel.Dto.DocumentRequirement> CreateDocumentRequirementAsync(SharedKernel.Dto.DocumentRequirement request)
        {
            if (!await _documentRepository.IsDocumentTypeExistsAsync(request.DocumentTypeId))
                throw new ArgumentException("Invalid document type");

            if (!await _documentRepository.IsDocumentCategoryExistsAsync(request.DocumentCategoryId))
                throw new ArgumentException("Invalid document category");

            if (request.DocumentSubCategoryId.HasValue && 
                !await _documentRepository.IsDocumentSubCategoryExistsAsync(request.DocumentSubCategoryId.Value))
                throw new ArgumentException("Invalid document sub-category");

            var requirement = new SharedKernel.Models.DocumentRequirement
            {
                ProcessType = request.ProcessType,
                SubProcess = request.SubProcess,
                DocumentTypeId = request.DocumentTypeId,
                DocumentCategoryId = request.DocumentCategoryId,
                DocumentSubCategoryId = request.DocumentSubCategoryId,
                IsRequired = request.IsRequired,
                DisplayOrder = request.DisplayOrder,
                Description = request.Description,
                ValidationRules = request.ValidationRules,
                ExpiryWarningDays = request.ExpiryWarningDays,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var createdRequirement = await _documentRepository.CreateDocumentRequirementAsync(requirement);
            return await MapToDocumentRequirementDtoAsync(createdRequirement);
        }

        public async Task<List<SharedKernel.Dto.DocumentRequirement>> GetDocumentRequirementsAsync(string processType, string? subProcess = null)
        {
            var requirements = await _documentRepository.GetDocumentRequirementsAsync(processType, subProcess);
            var responses = new List<SharedKernel.Dto.DocumentRequirement>();

            foreach (var requirement in requirements)
            {
                responses.Add(await MapToDocumentRequirementDtoAsync(requirement));
            }

            return responses;
        }

        public async Task<SharedKernel.Dto.DocumentRequirement> UpdateDocumentRequirementAsync(Guid requirementId, SharedKernel.Dto.DocumentRequirement request)
        {
            var requirement = await _documentRepository.GetDocumentRequirementByIdAsync(requirementId);
            if (requirement == null)
                throw new ArgumentException("Document requirement not found");

            requirement.ProcessType = request.ProcessType;
            requirement.SubProcess = request.SubProcess;
            requirement.DocumentTypeId = request.DocumentTypeId;
            requirement.DocumentCategoryId = request.DocumentCategoryId;
            requirement.DocumentSubCategoryId = request.DocumentSubCategoryId;
            requirement.IsRequired = request.IsRequired;
            requirement.DisplayOrder = request.DisplayOrder;
            requirement.Description = request.Description;
            requirement.ValidationRules = request.ValidationRules;
            requirement.ExpiryWarningDays = request.ExpiryWarningDays;
            requirement.IsActive = request.IsActive;
            requirement.UpdatedAt = DateTime.UtcNow;

            var updatedRequirement = await _documentRepository.UpdateDocumentRequirementAsync(requirement);
            return await MapToDocumentRequirementDtoAsync(updatedRequirement);
        }

        public async Task<bool> DeleteDocumentRequirementAsync(Guid requirementId)
        {
            return await _documentRepository.DeleteDocumentRequirementAsync(requirementId);
        }

        // Document completeness and validation
        public async Task<DocumentCompletenessResponse> CheckDocumentCompletenessAsync(string processType, string? subProcess, Guid entityId)
        {
            var requirements = await GetDocumentRequirementsAsync(processType, subProcess);
            var existingDocuments = await _documentRepository.GetDocumentsByEntityAsync(entityId, processType);

            var requiredDocuments = requirements.Where(r => r.IsRequired).ToList();
            var missingDocuments = new List<SharedKernel.Dto.DocumentRequirement>();
            var expiredDocuments = new List<SharedKernel.Dto.DocumentRequirement>();
            var pendingVerification = new List<SharedKernel.Dto.DocumentRequirement>();

            foreach (var requirement in requiredDocuments)
            {
                var existingDoc = existingDocuments.FirstOrDefault(d => 
                    d.DocumentTypeId == requirement.DocumentTypeId && 
                    d.DocumentCategoryId == requirement.DocumentCategoryId &&
                    d.DocumentSubCategoryId == requirement.DocumentSubCategoryId);

                if (existingDoc == null)
                {
                    missingDocuments.Add(requirement);
                }
                else if (existingDoc.ExpiryDate.HasValue && existingDoc.ExpiryDate < DateTime.UtcNow)
                {
                    expiredDocuments.Add(requirement);
                }
                else if (!existingDoc.IsVerified)
                {
                    pendingVerification.Add(requirement);
                }
            }

            return new DocumentCompletenessResponse
            {
                IsComplete = !missingDocuments.Any() && !expiredDocuments.Any() && !pendingVerification.Any(),
                RequiredDocuments = requiredDocuments,
                MissingDocuments = missingDocuments,
                ExpiredDocuments = expiredDocuments,
                PendingVerification = pendingVerification
            };
        }

        public async Task<bool> ValidateDocumentAsync(Guid documentId)
        {
            var document = await _documentRepository.GetDocumentByIdAsync(documentId);
            if (document == null) return false;

            // Add validation logic here based on document type/category rules
            return true;
        }

        public async Task<List<DocumentResponse>> GetExpiringDocumentsAsync(int daysThreshold = 30)
        {
            var documents = await _documentRepository.GetExpiringDocumentsAsync(daysThreshold);
            var responses = new List<DocumentResponse>();

            foreach (var document in documents)
            {
                responses.Add(await MapToDocumentResponseAsync(document));
            }

            return responses;
        }

        public async Task<List<DocumentResponse>> GetDocumentsRequiringVerificationAsync()
        {
            var documents = await _documentRepository.GetDocumentsRequiringVerificationAsync();
            var responses = new List<DocumentResponse>();

            foreach (var document in documents)
            {
                responses.Add(await MapToDocumentResponseAsync(document));
            }

            return responses;
        }

        // Document statistics and reporting
        public async Task<DocumentStatistics> GetDocumentStatisticsAsync()
        {
            var totalDocuments = await _documentRepository.GetDocumentCountAsync();
            var totalStorageUsed = await _documentRepository.GetTotalStorageUsedAsync();
            var documentsByType = await _documentRepository.GetDocumentsByTypeStatisticsAsync();
            var documentsByCategory = await _documentRepository.GetDocumentsByCategoryStatisticsAsync();

            return new DocumentStatistics
            {
                TotalDocuments = totalDocuments,
                ActiveDocuments = totalDocuments, // TODO: Filter by status
                ExpiredDocuments = 0, // TODO: Calculate
                PendingVerification = 0, // TODO: Calculate
                RequiredDocuments = 0, // TODO: Calculate
                TotalStorageUsed = totalStorageUsed,
                DocumentsByType = documentsByType,
                DocumentsByCategory = documentsByCategory
            };
        }

        public async Task<DocumentHierarchyResponse> GetDocumentHierarchyAsync()
        {
            var documentTypes = await GetAllDocumentTypesAsync();
            return new DocumentHierarchyResponse
            {
                DocumentTypes = documentTypes
            };
        }

        // Utility methods
        public async Task<bool> IsDocumentTypeExistsAsync(Guid documentTypeId)
        {
            return await _documentRepository.IsDocumentTypeExistsAsync(documentTypeId);
        }

        public async Task<bool> IsDocumentCategoryExistsAsync(Guid categoryId)
        {
            return await _documentRepository.IsDocumentCategoryExistsAsync(categoryId);
        }

        public async Task<bool> IsDocumentSubCategoryExistsAsync(Guid subCategoryId)
        {
            return await _documentRepository.IsDocumentSubCategoryExistsAsync(subCategoryId);
        }

        public async Task<string> GetDocumentTypeNameAsync(Guid documentTypeId)
        {
            return await _documentRepository.GetDocumentTypeNameAsync(documentTypeId);
        }

        public async Task<string> GetDocumentCategoryNameAsync(Guid categoryId)
        {
            return await _documentRepository.GetDocumentCategoryNameAsync(categoryId);
        }

        public async Task<string?> GetDocumentSubCategoryNameAsync(Guid subCategoryId)
        {
            return await _documentRepository.GetDocumentSubCategoryNameAsync(subCategoryId);
        }

        // Private helper methods
        private async Task<string> UploadToS3Async(IFormFile file, DocumentUploadRequest request)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            
            using var transferUtility = new TransferUtility(_s3Client);
            using var stream = file.OpenReadStream();
            
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileName,
                BucketName = _bucketName,
                ContentType = file.ContentType
            };

            await transferUtility.UploadAsync(uploadRequest);
            return fileName;
        }

        private async Task<DocumentResponse> MapToDocumentResponseAsync(PropertyDocument document)
        {
            return new DocumentResponse
            {
                Id = document.Id,
                DocumentTypeId = document.DocumentTypeId,
                DocumentCategoryId = document.DocumentCategoryId,
                DocumentSubCategoryId = document.DocumentSubCategoryId,
                DocumentTypeName = document.DocumentType?.Name ?? await GetDocumentTypeNameAsync(document.DocumentTypeId),
                DocumentCategoryName = document.DocumentCategory?.Name ?? await GetDocumentCategoryNameAsync(document.DocumentCategoryId),
                DocumentSubCategoryName = document.DocumentSubCategory?.Name ?? await GetDocumentSubCategoryNameAsync(document.DocumentSubCategoryId ?? Guid.Empty),
                FileName = document.FileName,
                FileUrl = document.FileUrl,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                UploadedAt = document.UploadedAt,
                UploadedBy = document.UploadedBy,
                ExpiryDate = document.ExpiryDate,
                IsRequired = document.IsRequired,
                IsVerified = document.IsVerified,
                VerifiedAt = document.VerifiedAt,
                VerifiedBy = document.VerifiedBy,
                Status = document.Status,
                Version = document.Version,
                Tags = document.Tags,
                Description = document.Description,
                Comments = document.Comments,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                PropertyId = document.PropertyId,
                TenantId = document.TenantId,
                LeaseId = document.LeaseId,
                MaintenanceTicketId = document.MaintenanceTicketId,
                BookingId = document.BookingId,
                RenewalRequestId = document.RenewalRequestId,
                TerminationProcessId = document.TerminationProcessId,
                BillId = document.BillId,
                PaymentId = document.PaymentId
            };
        }

        private DocumentTypeResponse MapToDocumentTypeResponse(DocumentType documentType)
        {
            return new DocumentTypeResponse
            {
                Id = documentType.Id,
                Name = documentType.Name,
                Description = documentType.Description,
                IsActive = documentType.IsActive,
                DisplayOrder = documentType.DisplayOrder,
                CreatedAt = documentType.CreatedAt,
                UpdatedAt = documentType.UpdatedAt,
                CategoryCount = documentType.Categories?.Count ?? 0,
                DocumentCount = documentType.Documents?.Count ?? 0
            };
        }

        private async Task<DocumentCategoryResponse> MapToDocumentCategoryResponseAsync(DocumentCategory category)
        {
            return new DocumentCategoryResponse
            {
                Id = category.Id,
                DocumentTypeId = category.DocumentTypeId,
                DocumentTypeName = category.DocumentType?.Name ?? await GetDocumentTypeNameAsync(category.DocumentTypeId),
                Name = category.Name,
                Description = category.Description,
                Code = category.Code,
                IsActive = category.IsActive,
                IsRequired = category.IsRequired,
                DisplayOrder = category.DisplayOrder,
                ValidationRules = category.ValidationRules,
                ExpiryWarningDays = category.ExpiryWarningDays,
                AllowedFileTypes = category.AllowedFileTypes,
                MaxFileSizeInBytes = category.MaxFileSizeInBytes,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                SubCategoryCount = category.SubCategories?.Count ?? 0,
                DocumentCount = category.Documents?.Count ?? 0
            };
        }

        private async Task<DocumentSubCategoryResponse> MapToDocumentSubCategoryResponseAsync(DocumentSubCategory subCategory)
        {
            return new DocumentSubCategoryResponse
            {
                Id = subCategory.Id,
                DocumentCategoryId = subCategory.DocumentCategoryId,
                DocumentCategoryName = subCategory.DocumentCategory?.Name ?? await GetDocumentCategoryNameAsync(subCategory.DocumentCategoryId),
                Name = subCategory.Name,
                Description = subCategory.Description,
                Code = subCategory.Code,
                IsActive = subCategory.IsActive,
                IsRequired = subCategory.IsRequired,
                DisplayOrder = subCategory.DisplayOrder,
                ValidationRules = subCategory.ValidationRules,
                ExpiryWarningDays = subCategory.ExpiryWarningDays,
                AllowedFileTypes = subCategory.AllowedFileTypes,
                MaxFileSizeInBytes = subCategory.MaxFileSizeInBytes,
                CreatedAt = subCategory.CreatedAt,
                UpdatedAt = subCategory.UpdatedAt,
                DocumentCount = subCategory.Documents?.Count ?? 0
            };
        }

        private async Task<SharedKernel.Dto.DocumentRequirement> MapToDocumentRequirementDtoAsync(SharedKernel.Models.DocumentRequirement requirement)
        {
            return new SharedKernel.Dto.DocumentRequirement
            {
                Id = requirement.Id,
                ProcessType = requirement.ProcessType,
                SubProcess = requirement.SubProcess,
                DocumentTypeId = requirement.DocumentTypeId,
                DocumentCategoryId = requirement.DocumentCategoryId,
                DocumentSubCategoryId = requirement.DocumentSubCategoryId,
                DocumentTypeName = requirement.DocumentType?.Name ?? await GetDocumentTypeNameAsync(requirement.DocumentTypeId),
                DocumentCategoryName = requirement.DocumentCategory?.Name ?? await GetDocumentCategoryNameAsync(requirement.DocumentCategoryId),
                DocumentSubCategoryName = requirement.DocumentSubCategory?.Name ?? await GetDocumentSubCategoryNameAsync(requirement.DocumentSubCategoryId ?? Guid.Empty),
                IsRequired = requirement.IsRequired,
                Description = requirement.Description,
                ValidationRules = requirement.ValidationRules,
                ExpiryWarningDays = requirement.ExpiryWarningDays,
                DisplayOrder = requirement.DisplayOrder,
                IsActive = requirement.IsActive
            };
        }
    }
} 
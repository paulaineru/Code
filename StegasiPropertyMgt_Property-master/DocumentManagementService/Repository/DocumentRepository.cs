using Microsoft.EntityFrameworkCore;
using SharedKernel.Dto;
using SharedKernel.Models;
using SharedKernel.Services.Interfaces;
using DocumentManagementService.Data;

namespace DocumentManagementService.Repository
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly DocumentDbContext _context;

        public DocumentRepository(DocumentDbContext context)
        {
            _context = context;
        }

        // Document CRUD operations
        public async Task<PropertyDocument> CreateDocumentAsync(PropertyDocument document)
        {
            _context.PropertyDocuments.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<PropertyDocument?> GetDocumentByIdAsync(Guid documentId)
        {
            return await _context.PropertyDocuments
                .Include(d => d.DocumentType)
                .Include(d => d.DocumentCategory)
                .Include(d => d.DocumentSubCategory)
                .FirstOrDefaultAsync(d => d.Id == documentId);
        }

        public async Task<List<PropertyDocument>> GetDocumentsAsync(DocumentSearchRequest request)
        {
            var query = _context.PropertyDocuments
                .Include(d => d.DocumentType)
                .Include(d => d.DocumentCategory)
                .Include(d => d.DocumentSubCategory)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(d => d.FileName.Contains(request.SearchTerm) || 
                                       d.Description.Contains(request.SearchTerm) ||
                                       d.Tags.Contains(request.SearchTerm));
            }

            if (request.DocumentTypeId.HasValue)
                query = query.Where(d => d.DocumentTypeId == request.DocumentTypeId);

            if (request.DocumentCategoryId.HasValue)
                query = query.Where(d => d.DocumentCategoryId == request.DocumentCategoryId);

            if (request.DocumentSubCategoryId.HasValue)
                query = query.Where(d => d.DocumentSubCategoryId == request.DocumentSubCategoryId);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(d => d.Status == request.Status);

            if (request.PropertyId.HasValue)
                query = query.Where(d => d.PropertyId == request.PropertyId);

            if (request.TenantId.HasValue)
                query = query.Where(d => d.TenantId == request.TenantId);

            if (request.LeaseId.HasValue)
                query = query.Where(d => d.LeaseId == request.LeaseId);

            if (request.MaintenanceTicketId.HasValue)
                query = query.Where(d => d.MaintenanceTicketId == request.MaintenanceTicketId);

            if (request.BookingId.HasValue)
                query = query.Where(d => d.BookingId == request.BookingId);

            if (request.RenewalRequestId.HasValue)
                query = query.Where(d => d.RenewalRequestId == request.RenewalRequestId);

            if (request.TerminationProcessId.HasValue)
                query = query.Where(d => d.TerminationProcessId == request.TerminationProcessId);

            if (request.BillId.HasValue)
                query = query.Where(d => d.BillId == request.BillId);

            if (request.PaymentId.HasValue)
                query = query.Where(d => d.PaymentId == request.PaymentId);

            if (request.IsRequired.HasValue)
                query = query.Where(d => d.IsRequired == request.IsRequired);

            if (request.IsVerified.HasValue)
                query = query.Where(d => d.IsVerified == request.IsVerified);

            if (request.UploadedFrom.HasValue)
                query = query.Where(d => d.UploadedAt >= request.UploadedFrom);

            if (request.UploadedTo.HasValue)
                query = query.Where(d => d.UploadedAt <= request.UploadedTo);

            if (request.ExpiresFrom.HasValue)
                query = query.Where(d => d.ExpiryDate >= request.ExpiresFrom);

            if (request.ExpiresTo.HasValue)
                query = query.Where(d => d.ExpiryDate <= request.ExpiresTo);

            // Apply pagination
            var skip = (request.Page - 1) * request.PageSize;
            return await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();
        }

        public async Task<PropertyDocument> UpdateDocumentAsync(PropertyDocument document)
        {
            document.UpdatedAt = DateTime.UtcNow;
            _context.PropertyDocuments.Update(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<bool> DeleteDocumentAsync(Guid documentId)
        {
            var document = await _context.PropertyDocuments.FindAsync(documentId);
            if (document == null) return false;

            _context.PropertyDocuments.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        // Document type management
        public async Task<DocumentType> CreateDocumentTypeAsync(DocumentType documentType)
        {
            _context.DocumentTypes.Add(documentType);
            await _context.SaveChangesAsync();
            return documentType;
        }

        public async Task<DocumentType?> GetDocumentTypeByIdAsync(Guid documentTypeId)
        {
            return await _context.DocumentTypes
                .Include(dt => dt.Categories)
                .FirstOrDefaultAsync(dt => dt.Id == documentTypeId);
        }

        public async Task<List<DocumentType>> GetAllDocumentTypesAsync()
        {
            return await _context.DocumentTypes
                .Include(dt => dt.Categories)
                .Where(dt => dt.IsActive)
                .OrderBy(dt => dt.DisplayOrder)
                .ToListAsync();
        }

        public async Task<DocumentType> UpdateDocumentTypeAsync(DocumentType documentType)
        {
            documentType.UpdatedAt = DateTime.UtcNow;
            _context.DocumentTypes.Update(documentType);
            await _context.SaveChangesAsync();
            return documentType;
        }

        public async Task<bool> DeleteDocumentTypeAsync(Guid documentTypeId)
        {
            var documentType = await _context.DocumentTypes.FindAsync(documentTypeId);
            if (documentType == null) return false;

            _context.DocumentTypes.Remove(documentType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsDocumentTypeExistsAsync(Guid documentTypeId)
        {
            return await _context.DocumentTypes.AnyAsync(dt => dt.Id == documentTypeId);
        }

        public async Task<string> GetDocumentTypeNameAsync(Guid documentTypeId)
        {
            var documentType = await _context.DocumentTypes.FindAsync(documentTypeId);
            return documentType?.Name ?? string.Empty;
        }

        // Document category management
        public async Task<DocumentCategory> CreateDocumentCategoryAsync(DocumentCategory category)
        {
            _context.DocumentCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<DocumentCategory?> GetDocumentCategoryByIdAsync(Guid categoryId)
        {
            return await _context.DocumentCategories
                .Include(dc => dc.DocumentType)
                .Include(dc => dc.SubCategories)
                .FirstOrDefaultAsync(dc => dc.Id == categoryId);
        }

        public async Task<List<DocumentCategory>> GetCategoriesByTypeAsync(Guid documentTypeId)
        {
            return await _context.DocumentCategories
                .Include(dc => dc.SubCategories)
                .Where(dc => dc.DocumentTypeId == documentTypeId && dc.IsActive)
                .OrderBy(dc => dc.DisplayOrder)
                .ToListAsync();
        }

        public async Task<DocumentCategory> UpdateDocumentCategoryAsync(DocumentCategory category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            _context.DocumentCategories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteDocumentCategoryAsync(Guid categoryId)
        {
            var category = await _context.DocumentCategories.FindAsync(categoryId);
            if (category == null) return false;

            _context.DocumentCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsDocumentCategoryExistsAsync(Guid categoryId)
        {
            return await _context.DocumentCategories.AnyAsync(dc => dc.Id == categoryId);
        }

        public async Task<string> GetDocumentCategoryNameAsync(Guid categoryId)
        {
            var category = await _context.DocumentCategories.FindAsync(categoryId);
            return category?.Name ?? string.Empty;
        }

        // Document sub-category management
        public async Task<DocumentSubCategory> CreateDocumentSubCategoryAsync(DocumentSubCategory subCategory)
        {
            _context.DocumentSubCategories.Add(subCategory);
            await _context.SaveChangesAsync();
            return subCategory;
        }

        public async Task<DocumentSubCategory?> GetDocumentSubCategoryByIdAsync(Guid subCategoryId)
        {
            return await _context.DocumentSubCategories
                .Include(dsc => dsc.DocumentCategory)
                .FirstOrDefaultAsync(dsc => dsc.Id == subCategoryId);
        }

        public async Task<List<DocumentSubCategory>> GetSubCategoriesByCategoryAsync(Guid categoryId)
        {
            return await _context.DocumentSubCategories
                .Where(dsc => dsc.DocumentCategoryId == categoryId && dsc.IsActive)
                .OrderBy(dsc => dsc.DisplayOrder)
                .ToListAsync();
        }

        public async Task<DocumentSubCategory> UpdateDocumentSubCategoryAsync(DocumentSubCategory subCategory)
        {
            subCategory.UpdatedAt = DateTime.UtcNow;
            _context.DocumentSubCategories.Update(subCategory);
            await _context.SaveChangesAsync();
            return subCategory;
        }

        public async Task<bool> DeleteDocumentSubCategoryAsync(Guid subCategoryId)
        {
            var subCategory = await _context.DocumentSubCategories.FindAsync(subCategoryId);
            if (subCategory == null) return false;

            _context.DocumentSubCategories.Remove(subCategory);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsDocumentSubCategoryExistsAsync(Guid subCategoryId)
        {
            return await _context.DocumentSubCategories.AnyAsync(dsc => dsc.Id == subCategoryId);
        }

        public async Task<string?> GetDocumentSubCategoryNameAsync(Guid subCategoryId)
        {
            var subCategory = await _context.DocumentSubCategories.FindAsync(subCategoryId);
            return subCategory?.Name;
        }

        // Document requirements management
        public async Task<SharedKernel.Models.DocumentRequirement> CreateDocumentRequirementAsync(SharedKernel.Models.DocumentRequirement requirement)
        {
            _context.DocumentRequirements.Add(requirement);
            await _context.SaveChangesAsync();
            return requirement;
        }

        public async Task<SharedKernel.Models.DocumentRequirement?> GetDocumentRequirementByIdAsync(Guid requirementId)
        {
            return await _context.DocumentRequirements
                .Include(dr => dr.DocumentType)
                .Include(dr => dr.DocumentCategory)
                .Include(dr => dr.DocumentSubCategory)
                .FirstOrDefaultAsync(dr => dr.Id == requirementId);
        }

        public async Task<List<SharedKernel.Models.DocumentRequirement>> GetDocumentRequirementsAsync(string processType, string? subProcess = null)
        {
            var query = _context.DocumentRequirements
                .Include(dr => dr.DocumentType)
                .Include(dr => dr.DocumentCategory)
                .Include(dr => dr.DocumentSubCategory)
                .Where(dr => dr.ProcessType == processType && dr.IsActive);

            if (!string.IsNullOrEmpty(subProcess))
                query = query.Where(dr => dr.SubProcess == subProcess);

            return await query
                .OrderBy(dr => dr.DisplayOrder)
                .ToListAsync();
        }

        public async Task<SharedKernel.Models.DocumentRequirement> UpdateDocumentRequirementAsync(SharedKernel.Models.DocumentRequirement requirement)
        {
            requirement.UpdatedAt = DateTime.UtcNow;
            _context.DocumentRequirements.Update(requirement);
            await _context.SaveChangesAsync();
            return requirement;
        }

        public async Task<bool> DeleteDocumentRequirementAsync(Guid requirementId)
        {
            var requirement = await _context.DocumentRequirements.FindAsync(requirementId);
            if (requirement == null) return false;

            _context.DocumentRequirements.Remove(requirement);
            await _context.SaveChangesAsync();
            return true;
        }

        // Document statistics and reporting
        public async Task<int> GetDocumentCountAsync()
        {
            return await _context.PropertyDocuments.CountAsync();
        }

        public async Task<int> GetDocumentCountByTypeAsync(Guid documentTypeId)
        {
            return await _context.PropertyDocuments.CountAsync(d => d.DocumentTypeId == documentTypeId);
        }

        public async Task<int> GetDocumentCountByCategoryAsync(Guid categoryId)
        {
            return await _context.PropertyDocuments.CountAsync(d => d.DocumentCategoryId == categoryId);
        }

        public async Task<int> GetDocumentCountBySubCategoryAsync(Guid subCategoryId)
        {
            return await _context.PropertyDocuments.CountAsync(d => d.DocumentSubCategoryId == subCategoryId);
        }

        public async Task<long> GetTotalStorageUsedAsync()
        {
            return await _context.PropertyDocuments.SumAsync(d => d.FileSize);
        }

        public async Task<Dictionary<string, int>> GetDocumentsByTypeStatisticsAsync()
        {
            return await _context.PropertyDocuments
                .GroupBy(d => d.DocumentType.Name)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetDocumentsByCategoryStatisticsAsync()
        {
            return await _context.PropertyDocuments
                .GroupBy(d => d.DocumentCategory.Name)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        // Document validation and completeness
        public async Task<List<PropertyDocument>> GetExpiringDocumentsAsync(int daysThreshold)
        {
            var expiryDate = DateTime.UtcNow.AddDays(daysThreshold);
            return await _context.PropertyDocuments
                .Include(d => d.DocumentType)
                .Include(d => d.DocumentCategory)
                .Where(d => d.ExpiryDate.HasValue && d.ExpiryDate <= expiryDate && d.Status == "Active")
                .ToListAsync();
        }

        public async Task<List<PropertyDocument>> GetDocumentsRequiringVerificationAsync()
        {
            return await _context.PropertyDocuments
                .Include(d => d.DocumentType)
                .Include(d => d.DocumentCategory)
                .Where(d => !d.IsVerified && d.Status == "Active")
                .ToListAsync();
        }

        public async Task<List<PropertyDocument>> GetDocumentsByEntityAsync(Guid entityId, string entityType)
        {
            var query = _context.PropertyDocuments
                .Include(d => d.DocumentType)
                .Include(d => d.DocumentCategory)
                .Include(d => d.DocumentSubCategory)
                .AsQueryable();

            switch (entityType.ToLower())
            {
                case "property":
                    query = query.Where(d => d.PropertyId == entityId);
                    break;
                case "tenant":
                    query = query.Where(d => d.TenantId == entityId);
                    break;
                case "lease":
                    query = query.Where(d => d.LeaseId == entityId);
                    break;
                case "maintenance":
                    query = query.Where(d => d.MaintenanceTicketId == entityId);
                    break;
                case "booking":
                    query = query.Where(d => d.BookingId == entityId);
                    break;
                case "renewal":
                    query = query.Where(d => d.RenewalRequestId == entityId);
                    break;
                case "termination":
                    query = query.Where(d => d.TerminationProcessId == entityId);
                    break;
                case "bill":
                    query = query.Where(d => d.BillId == entityId);
                    break;
                case "payment":
                    query = query.Where(d => d.PaymentId == entityId);
                    break;
            }

            return await query.ToListAsync();
        }

        public async Task<List<PropertyDocument>> GetRequiredDocumentsAsync(Guid entityId, string processType, string? subProcess = null)
        {
            // This would need to be implemented based on the specific entity type and process
            // For now, returning empty list as this requires business logic
            return new List<PropertyDocument>();
        }

        // Utility methods
        public async Task<bool> IsDocumentExistsAsync(Guid documentId)
        {
            return await _context.PropertyDocuments.AnyAsync(d => d.Id == documentId);
        }

        public async Task<List<PropertyDocument>> GetDocumentsByUserAsync(Guid userId)
        {
            return await _context.PropertyDocuments
                .Include(d => d.DocumentType)
                .Include(d => d.DocumentCategory)
                .Where(d => d.UploadedBy == userId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PropertyDocument>> GetDocumentsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.PropertyDocuments
                .Include(d => d.DocumentType)
                .Include(d => d.DocumentCategory)
                .Where(d => d.CreatedAt >= fromDate && d.CreatedAt <= toDate)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }
    }
} 
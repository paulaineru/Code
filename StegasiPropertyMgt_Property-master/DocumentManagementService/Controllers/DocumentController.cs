using Microsoft.AspNetCore.Mvc;
using SharedKernel.Dto;
using SharedKernel.Services.Interfaces;
using SharedKernel.Models;

namespace DocumentManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        // Document CRUD operations
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<DocumentResponse>>> UploadDocument([FromForm] DocumentUploadRequest request, IFormFile file)
        {
            try
            {
                var result = await _documentService.UploadDocumentAsync(request, file);
                return Ok(new ApiResponse<DocumentResponse>("200", "Document uploaded successfully", result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<DocumentResponse>("400", ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentResponse>("500", "An error occurred while uploading the document", null));
            }
        }

        [HttpGet("{documentId}")]
        public async Task<ActionResult<ApiResponse<DocumentResponse>>> GetDocument(Guid documentId)
        {
            try
            {
                var result = await _documentService.GetDocumentByIdAsync(documentId);
                return Ok(new ApiResponse<DocumentResponse>("200", "Success message", result));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<DocumentResponse>("404", ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentResponse>("500", "An error occurred while retrieving the document", null));
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<ApiResponse<List<DocumentResponse>>>> SearchDocuments([FromBody] DocumentSearchRequest request)
        {
            try
            {
                var result = await _documentService.GetDocumentsAsync(request);
                return Ok(new ApiResponse<List<DocumentResponse>>("200", "Success message", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<DocumentResponse>>("500", "An error occurred while searching documents", null));
            }
        }

        [HttpPut("{documentId}")]
        public async Task<ActionResult<ApiResponse<DocumentResponse>>> UpdateDocument(Guid documentId, [FromBody] DocumentUpdateRequest request)
        {
            try
            {
                var result = await _documentService.UpdateDocumentAsync(documentId, request);
                return Ok(new ApiResponse<DocumentResponse>("200", "Document updated successfully", result));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<DocumentResponse>("404", ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentResponse>("500", "An error occurred while updating the document", null));
            }
        }

        [HttpDelete("{documentId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteDocument(Guid documentId)
        {
            try
            {
                var result = await _documentService.DeleteDocumentAsync(documentId);
                return Ok(new ApiResponse<bool>("200", result ? "Document deleted successfully" : "Document not found", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>("500", "An error occurred while deleting the document", false));
            }
        }

        [HttpPost("verify")]
        public async Task<ActionResult<ApiResponse<DocumentResponse>>> VerifyDocument([FromBody] DocumentVerificationRequest request)
        {
            try
            {
                var result = await _documentService.VerifyDocumentAsync(request);
                return Ok(new ApiResponse<DocumentResponse>("200", "Document verification updated successfully", result));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<DocumentResponse>("404", ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentResponse>("500", "An error occurred while verifying the document", null));
            }
        }

        // Document type management
        [HttpPost("types")]
        public async Task<ActionResult<ApiResponse<DocumentTypeResponse>>> CreateDocumentType([FromBody] DocumentTypeRequest request)
        {
            try
            {
                var result = await _documentService.CreateDocumentTypeAsync(request);
                return Ok(new ApiResponse<DocumentTypeResponse>("200", "Document type created successfully", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentTypeResponse>("500", "An error occurred while creating the document type", null));
            }
        }

        [HttpGet("types")]
        public async Task<ActionResult<ApiResponse<List<DocumentTypeResponse>>>> GetAllDocumentTypes()
        {
            try
            {
                var result = await _documentService.GetAllDocumentTypesAsync();
                return Ok(new ApiResponse<List<DocumentTypeResponse>>("200", "Success message", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<DocumentTypeResponse>>("500", "An error occurred while retrieving document types", null));
            }
        }

        [HttpGet("types/{documentTypeId}")]
        public async Task<ActionResult<ApiResponse<DocumentTypeResponse>>> GetDocumentType(Guid documentTypeId)
        {
            try
            {
                var result = await _documentService.GetDocumentTypeByIdAsync(documentTypeId);
                return Ok(new ApiResponse<DocumentTypeResponse>("200", "Success message", result));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<DocumentTypeResponse>("404", ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentTypeResponse>("500", "An error occurred while retrieving the document type", null));
            }
        }

        [HttpPut("types/{documentTypeId}")]
        public async Task<ActionResult<ApiResponse<DocumentTypeResponse>>> UpdateDocumentType(Guid documentTypeId, [FromBody] DocumentTypeRequest request)
        {
            try
            {
                var result = await _documentService.UpdateDocumentTypeAsync(documentTypeId, request);
                return Ok(new ApiResponse<DocumentTypeResponse>("200", "Document type updated successfully", result));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<DocumentTypeResponse>("404", ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentTypeResponse>("500", "An error occurred while updating the document type", null));
            }
        }

        [HttpDelete("types/{documentTypeId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteDocumentType(Guid documentTypeId)
        {
            try
            {
                var result = await _documentService.DeleteDocumentTypeAsync(documentTypeId);
                return Ok(new ApiResponse<bool>("200", result ? "Document type deleted successfully" : "Document type not found", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>("500", "An error occurred while deleting the document type", false));
            }
        }

        // Document category management
        [HttpPost("categories")]
        public async Task<ActionResult<ApiResponse<DocumentCategoryResponse>>> CreateDocumentCategory([FromBody] DocumentCategoryRequest request)
        {
            try
            {
                var result = await _documentService.CreateDocumentCategoryAsync(request);
                return Ok(new ApiResponse<DocumentCategoryResponse>("200", "Document category created successfully", result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<DocumentCategoryResponse>("400", ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentCategoryResponse>("500", "An error occurred while creating the document category", null));
            }
        }

        [HttpGet("categories/type/{documentTypeId}")]
        public async Task<ActionResult<ApiResponse<List<DocumentCategoryResponse>>>> GetCategoriesByType(Guid documentTypeId)
        {
            try
            {
                var result = await _documentService.GetCategoriesByTypeAsync(documentTypeId);
                return Ok(new ApiResponse<List<DocumentCategoryResponse>>("200", "Success message", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<DocumentCategoryResponse>>("500", "An error occurred while retrieving document categories", null));
            }
        }

        [HttpGet("categories/{categoryId}")]
        public async Task<ActionResult<ApiResponse<DocumentCategoryResponse>>> GetDocumentCategory(Guid categoryId)
        {
            try
            {
                var result = await _documentService.GetDocumentCategoryByIdAsync(categoryId);
                return Ok(new ApiResponse<DocumentCategoryResponse>("200", "Success message", result));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<DocumentCategoryResponse>("404", ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentCategoryResponse>("500", "An error occurred while retrieving the document category", null));
            }
        }

        [HttpPut("categories/{categoryId}")]
        public async Task<ActionResult<ApiResponse<DocumentCategoryResponse>>> UpdateDocumentCategory(Guid categoryId, [FromBody] DocumentCategoryRequest request)
        {
            try
            {
                var result = await _documentService.UpdateDocumentCategoryAsync(categoryId, request);
                return Ok(new ApiResponse<DocumentCategoryResponse>("200", "Document category updated successfully", result));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<DocumentCategoryResponse>("404", ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentCategoryResponse>("500", "An error occurred while updating the document category", null));
            }
        }

        [HttpDelete("categories/{categoryId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteDocumentCategory(Guid categoryId)
        {
            try
            {
                var result = await _documentService.DeleteDocumentCategoryAsync(categoryId);
                return Ok(new ApiResponse<bool>("200", result ? "Document category deleted successfully" : "Document category not found", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>("500", "An error occurred while deleting the document category", false));
            }
        }

        // Document sub-category management
        [HttpPost("subcategories")]
        public async Task<ActionResult<ApiResponse<DocumentSubCategoryResponse>>> CreateDocumentSubCategory([FromBody] DocumentSubCategoryRequest request)
        {
            try
            {
                var result = await _documentService.CreateDocumentSubCategoryAsync(request);
                return Ok(new ApiResponse<DocumentSubCategoryResponse>("200", "Document sub-category created successfully", result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<DocumentSubCategoryResponse>("400", ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentSubCategoryResponse>("500", "An error occurred while creating the document sub-category", null));
            }
        }

        [HttpGet("subcategories/category/{categoryId}")]
        public async Task<ActionResult<ApiResponse<List<DocumentSubCategoryResponse>>>> GetSubCategoriesByCategory(Guid categoryId)
        {
            try
            {
                var result = await _documentService.GetSubCategoriesByCategoryAsync(categoryId);
                return Ok(new ApiResponse<List<DocumentSubCategoryResponse>>("200", "Success message", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<DocumentSubCategoryResponse>>("500", "An error occurred while retrieving document sub-categories", null));
            }
        }

        [HttpGet("subcategories/{subCategoryId}")]
        public async Task<ActionResult<ApiResponse<DocumentSubCategoryResponse>>> GetDocumentSubCategory(Guid subCategoryId)
        {
            try
            {
                var result = await _documentService.GetDocumentSubCategoryByIdAsync(subCategoryId);
                return Ok(new ApiResponse<DocumentSubCategoryResponse>("200", "Success message", result));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<DocumentSubCategoryResponse>("404", ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentSubCategoryResponse>("500", "An error occurred while retrieving the document sub-category", null));
            }
        }

        [HttpPut("subcategories/{subCategoryId}")]
        public async Task<ActionResult<ApiResponse<DocumentSubCategoryResponse>>> UpdateDocumentSubCategory(Guid subCategoryId, [FromBody] DocumentSubCategoryRequest request)
        {
            try
            {
                var result = await _documentService.UpdateDocumentSubCategoryAsync(subCategoryId, request);
                return Ok(new ApiResponse<DocumentSubCategoryResponse>("200", "Document sub-category updated successfully", result));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<DocumentSubCategoryResponse>("404", ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentSubCategoryResponse>("500", "An error occurred while updating the document sub-category", null));
            }
        }

        [HttpDelete("subcategories/{subCategoryId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteDocumentSubCategory(Guid subCategoryId)
        {
            try
            {
                var result = await _documentService.DeleteDocumentSubCategoryAsync(subCategoryId);
                return Ok(new ApiResponse<bool>("200", result ? "Document sub-category deleted successfully" : "Document sub-category not found", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>("500", "An error occurred while deleting the document sub-category", false));
            }
        }

        // Document requirements management
        [HttpPost("requirements")]
        public async Task<ActionResult<ApiResponse<SharedKernel.Dto.DocumentRequirement>>> CreateDocumentRequirement([FromBody] SharedKernel.Dto.DocumentRequirement request)
        {
            try
            {
                var result = await _documentService.CreateDocumentRequirementAsync(request);
                return Ok(new ApiResponse<SharedKernel.Dto.DocumentRequirement>("200", "Document requirement created successfully", result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<SharedKernel.Dto.DocumentRequirement>("400", ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<SharedKernel.Dto.DocumentRequirement>("500", "An error occurred while creating the document requirement", null));
            }
        }

        [HttpGet("requirements/{processType}")]
        public async Task<ActionResult<ApiResponse<List<SharedKernel.Dto.DocumentRequirement>>>> GetDocumentRequirements(string processType, [FromQuery] string? subProcess)
        {
            try
            {
                var result = await _documentService.GetDocumentRequirementsAsync(processType, subProcess);
                return Ok(new ApiResponse<List<SharedKernel.Dto.DocumentRequirement>>("200", "Success message", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<SharedKernel.Dto.DocumentRequirement>>("500", "An error occurred while retrieving document requirements", null));
            }
        }

        // Document completeness and validation
        [HttpGet("completeness/{processType}/{entityId}")]
        public async Task<ActionResult<ApiResponse<DocumentCompletenessResponse>>> CheckDocumentCompleteness(string processType, Guid entityId, [FromQuery] string? subProcess)
        {
            try
            {
                var result = await _documentService.CheckDocumentCompletenessAsync(processType, subProcess, entityId);
                return Ok(new ApiResponse<DocumentCompletenessResponse>("200", "Success message", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentCompletenessResponse>("500", "An error occurred while checking document completeness", null));
            }
        }

        [HttpGet("expiring")]
        public async Task<ActionResult<ApiResponse<List<DocumentResponse>>>> GetExpiringDocuments([FromQuery] int daysThreshold = 30)
        {
            try
            {
                var result = await _documentService.GetExpiringDocumentsAsync(daysThreshold);
                return Ok(new ApiResponse<List<DocumentResponse>>("200", "Success message", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<DocumentResponse>>("500", "An error occurred while retrieving expiring documents", null));
            }
        }

        [HttpGet("pending-verification")]
        public async Task<ActionResult<ApiResponse<List<DocumentResponse>>>> GetDocumentsRequiringVerification()
        {
            try
            {
                var result = await _documentService.GetDocumentsRequiringVerificationAsync();
                return Ok(new ApiResponse<List<DocumentResponse>>("200", "Success message", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<DocumentResponse>>("500", "An error occurred while retrieving documents requiring verification", null));
            }
        }

        // Document statistics and reporting
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<DocumentStatistics>>> GetDocumentStatistics()
        {
            try
            {
                var result = await _documentService.GetDocumentStatisticsAsync();
                return Ok(new ApiResponse<DocumentStatistics>("200", "Success message", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentStatistics>("500", "An error occurred while retrieving document statistics", null));
            }
        }

        [HttpGet("hierarchy")]
        public async Task<ActionResult<ApiResponse<DocumentHierarchyResponse>>> GetDocumentHierarchy()
        {
            try
            {
                var result = await _documentService.GetDocumentHierarchyAsync();
                return Ok(new ApiResponse<DocumentHierarchyResponse>("200", "Success message", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentHierarchyResponse>("500", "An error occurred while retrieving document hierarchy", null));
            }
        }
    }
} 
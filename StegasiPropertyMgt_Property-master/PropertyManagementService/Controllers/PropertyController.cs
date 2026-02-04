using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PropertyManagementService.Services;
using SharedKernel.Dto;
using SharedKernel.Models;
using SharedKernel.Utilities;
using SharedKernel.Services;
using Microsoft.AspNetCore.Http;
using PropertyManagementService.Repository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PropertyManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;
        private readonly IAmenityService _amenityService;
        private readonly IApprovalWorkflowService _approvalWorkflowService;
        private readonly ILogger<PropertyController> _logger;
        private readonly S3ImageService _s3ImageService;
        private readonly PropertyDbContext _dbContext;

        public PropertyController(
            IPropertyService propertyService,
            IAmenityService amenityService,
            IApprovalWorkflowService approvalWorkflowService,
            ILogger<PropertyController> logger,
            S3ImageService s3ImageService,
            PropertyDbContext dbContext
        )
        {
            _propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
            _amenityService = amenityService ?? throw new ArgumentNullException(nameof(amenityService));
            _approvalWorkflowService = approvalWorkflowService ?? throw new ArgumentNullException(nameof(approvalWorkflowService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _s3ImageService = s3ImageService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Registers a new property for approval (Estates Officer).
        /// </summary>
        /// <param name="dto">Property details including type-specific specifications</param>
        /// <returns>The registered property with pending status</returns>
        /// <response code="201">Property registered successfully</response>
        /// <response code="400">Invalid property data</response>
        /// <response code="500">Server error</response>
        [HttpPost]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        [ProducesResponseType(typeof(Property), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterProperty([FromBody] CreatePropertyDto dto)
        {
            try
            {
                var userId = HttpContext.GetUserId().ToString();
                var property = await _propertyService.CreatePropertyAsync(dto, userId);

                // Create approval workflow
                var workflow = await _approvalWorkflowService.CreateWorkflowAsync(
                    "Property",
                    property.Id,
                    property.GetType().Name,
                    Guid.Parse(userId)
                );

                _logger.LogInformation("Property registered for approval: {Id} by user: {UserId}", property.Id, userId);
                return CreatedAtAction(nameof(GetProperty), new { id = property.Id }, 
                    ApiResponse<Property>.Success(property, "Property registered successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid property registration data: {Error}", ex.Message);
                return BadRequest(ApiResponse.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering property: {Name}", dto?.Name);
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while registering the property"));
            }
        }

        /// <summary>
        /// Approves or rejects a pending property (Property Manager).
        /// </summary>
        /// <param name="id">Property ID to approve/reject</param>
        /// <param name="decision">Approval decision DTO</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Approval status updated</response>
        /// <response code="400">Invalid decision data</response>
        /// <response code="404">Property not found</response>
        /// <response code="500">Server error</response>
        [HttpPut("{id:guid}/approval")]
        [Authorize(Roles = "Property Manager,Estates Officer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApproveProperty(Guid id, [FromBody] PropertyApprovalDto decision)
        {
            try
            {
                var userId = HttpContext.GetUserId().ToString();
                var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value 
                    ?? HttpContext.User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
                var property = await _propertyService.GetPropertyByIdAsync(id);

                // Get the current workflow
                var workflow = await _approvalWorkflowService.GetWorkflowByEntityAsync("Property", id);
                if (workflow == null)
                {
                    return BadRequest(ApiResponse.BadRequest("No approval workflow found for this property"));
                }

                // Get current stage
                var currentStage = await _approvalWorkflowService.GetCurrentStageAsync(workflow.Id);
                if (currentStage == null)
                {
                    return BadRequest(ApiResponse.BadRequest("No pending approval stage found"));
                }

                _logger.LogInformation("Current stage - WorkflowId: {WorkflowId}, StageNumber: {StageNumber}, RequiredRole: {RequiredRole}, UserRole: {UserRole}", 
                    workflow.Id, currentStage.StageNumber, currentStage.Role, userRole);

                // Check if user can approve this stage
                if (!await _approvalWorkflowService.CanApproveStageAsync(workflow.Id, currentStage.StageNumber, Guid.Parse(userId), userRole))
                {
                    _logger.LogWarning("User {UserId} with role {UserRole} cannot approve stage {StageNumber} requiring role {RequiredRole}", 
                        userId, userRole, currentStage.StageNumber, currentStage.Role);
                    return StatusCode(403, ApiResponse.Forbidden($"Access denied. Stage {currentStage.StageNumber} requires '{currentStage.Role}' role, but you have '{userRole}' role."));
                }

                // Process the approval decision
                switch (decision.Status.ToLower())
                {
                    case "approved":
                        await _approvalWorkflowService.ApproveStageAsync(workflow.Id, currentStage.StageNumber, Guid.Parse(userId), decision.Comments, userRole);
                        break;
                    case "rejected":
                        await _approvalWorkflowService.RejectStageAsync(workflow.Id, currentStage.StageNumber, Guid.Parse(userId), decision.Comments, userRole);
                        break;
                    case "moreinfo":
                        await _approvalWorkflowService.RequestMoreInfoAsync(workflow.Id, currentStage.StageNumber, Guid.Parse(userId), decision.Comments, userRole);
                        break;
                    default:
                        return BadRequest(ApiResponse.BadRequest("Invalid approval status"));
                }

                // Update property status based on workflow status
                var updatedWorkflow = await _approvalWorkflowService.GetWorkflowAsync(workflow.Id);
                
                // Map workflow status to property approval status
                string propertyApprovalStatus;
                switch (updatedWorkflow.Status)
                {
                    case ApprovalStatus.Approved:
                        propertyApprovalStatus = "Approved";
                        break;
                    case ApprovalStatus.Rejected:
                        propertyApprovalStatus = "Rejected";
                        break;
                    case ApprovalStatus.MoreInfoRequired:
                        propertyApprovalStatus = "MoreInfoRequired";
                        break;
                    case ApprovalStatus.Pending:
                    case ApprovalStatus.InProgress:
                        propertyApprovalStatus = "Pending";
                        break;
                    default:
                        propertyApprovalStatus = "Pending";
                        break;
                }
                
                var updateDto = MapToUpdateDto(property, new PropertyApprovalDto 
                { 
                    Status = propertyApprovalStatus,
                    Comments = decision.Comments
                });
                await _propertyService.UpdatePropertyAsync(id, updateDto, userId);

                _logger.LogInformation("Property {Id} approval status updated to {Status} by {UserId}",
                    id, decision.Status, userId);
                
                // Return success response with next steps information
                var responseData = new PropertyApprovalResponseDto
                { 
                    PropertyId = id,
                    CurrentStatus = propertyApprovalStatus,
                    ApprovedBy = Guid.Parse(userId),
                    ApprovedAt = DateTime.UtcNow,
                    NextStage = updatedWorkflow.Status == ApprovalStatus.Approved ? "Complete" : "Pending next approval",
                    Message = $"Property approved for stage {currentStage.StageNumber}. {(updatedWorkflow.Status == ApprovalStatus.Approved ? "Approval process completed." : "Awaiting next stage approval.")}"
                };
                
                return Ok(ApiResponse<PropertyApprovalResponseDto>.Success(responseData, "Property approval processed successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Property not found for approval: {Id}", id);
                return NotFound(ApiResponse.NotFound(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid approval decision for property {Id}: {Error}", id, ex.Message);
                return BadRequest(ApiResponse.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating approval for property: {Id}", id);
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while updating property approval"));
            }
        }

        /// <summary>
        /// Retrieves a property by ID.
        /// </summary>
        /// <param name="id">Property ID</param>
        /// <returns>The requested property</returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Property), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProperty(Guid id)
        {
            try
            {
                var property = await _propertyService.GetPropertyByIdAsync(id);
                return Ok(ApiResponse<Property>.Success(property, "Property retrieved successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Property not found: {Id}", id);
                return NotFound(ApiResponse.NotFound(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching property: {Id}", id);
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while fetching the property"));
            }
        }

        /// <summary>
        /// Retrieves all active properties.
        /// </summary>
        /// <returns>List of active properties</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<Property>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllProperties()
        {
            try
            {
                var properties = await _propertyService.GetAllPropertiesAsync();
                var activeProperties = properties.Where(p => p.ApprovalStatus == "Approved").ToList();
                _logger.LogInformation("Retrieved {Count} active properties", activeProperties.Count);
                return Ok(ApiResponse<IEnumerable<Property>>.Success(activeProperties, $"{activeProperties.Count} active properties retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all properties");
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while fetching properties"));
            }
        }

        /// <summary>
        /// Retrieves properties by type (only approved properties).
        /// </summary>
        /// <param name="propertyType">Type of properties to retrieve</param>
        /// <returns>List of approved properties of specified type</returns>
        [HttpGet("type/{propertyType}")]
        [Authorize(Roles = "Tenant,Sales Officer,Sales Manager")]
        [ProducesResponseType(typeof(IEnumerable<Property>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPropertiesByType(string propertyType)
        {
            try
            {
                var properties = await _propertyService.GetPropertiesByTypeAsync(propertyType);
                var approvedProperties = properties.Where(p => p.ApprovalStatus == "Approved").ToList();
                _logger.LogInformation("Retrieved {Count} approved properties of type {Type}", approvedProperties.Count, propertyType);
                return Ok(ApiResponse<IEnumerable<Property>>.Success(approvedProperties, $"{approvedProperties.Count} approved {propertyType} properties retrieved"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid property type requested: {Type}, Error: {Error}", propertyType, ex.Message);
                return BadRequest(ApiResponse.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching properties by type: {Type}", propertyType);
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while fetching properties by type"));
            }
        }

        /// <summary>
        /// Retrieves properties filtered by status and/or type.
        /// </summary>
        /// <param name="status">Property status (e.g., active, pending, approved)</param>
        /// <param name="type">Property type (e.g., residential, commercial)</param>
        /// <returns>List of properties matching the filter criteria</returns>
        /// <response code="200">Returns filtered properties</response>
        /// <response code="400">Invalid filter parameters</response>
        /// <response code="500">Server error</response>
        [HttpGet("properties")]
        [Authorize(Roles = "Estates Officer,Property Manager,Tenant,Sales Officer,Sales Manager")]
        [ProducesResponseType(typeof(IEnumerable<Property>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPropertiesByFilter(
            [FromQuery] string status = null,
            [FromQuery] string type = null)
        {
            try
            {
                var properties = await _propertyService.GetPropertiesByFilterAsync(status, type);
                _logger.LogInformation("Retrieved {Count} properties with status: {Status} and type: {Type}",
                    properties.Count, status ?? "any", type ?? "any");
                return Ok(properties);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid filter parameters - Status: {Status}, Type: {Type}, Error: {Error}",
                    status, type, ex.Message);
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching properties with status: {Status} and type: {Type}", status, type);
                return StatusCode(500, new ErrorResponse { Error = "An error occurred while fetching filtered properties" });
            }
        }

        /// <summary>
        /// Updates an existing property (Estates Officer can modify pending properties).
        /// </summary>
        /// <param name="id">Property ID</param>
        /// <param name="dto">Updated property details</param>
        /// <returns>No content on success</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Estates Officer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProperty(Guid id, [FromBody] CreatePropertyDto dto)
        {
            try
            {
                var userId = HttpContext.GetUserId().ToString();
                var existingProperty = await _propertyService.GetPropertyByIdAsync(id);

                if (existingProperty.ApprovalStatus != "Pending")
                {
                    return BadRequest(new ErrorResponse { Error = "Can only modify pending properties" });
                }

                dto.ApprovalStatus = "Pending"; // Maintain pending status
                await _propertyService.UpdatePropertyAsync(id, dto, userId);
                _logger.LogInformation("Pending property updated: {Id} by {UserId}", id, userId);
                
                var responseData = new 
                { 
                    propertyId = id,
                    updatedAt = DateTime.UtcNow,
                    updatedBy = userId,
                    message = "Property updated successfully"
                };
                
                return Ok(ApiResponse<object>.Success(responseData, "Property updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Property not found for update: {Id}", id);
                return NotFound(ApiResponse.NotFound(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid update data for property {Id}: {Error}", id, ex.Message);
                return BadRequest(ApiResponse.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating property: {Id}", id);
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while updating the property"));
            }
        }

        /// <summary>
        /// Deletes a pending property (Estates Officer).
        /// </summary>
        /// <param name="id">Property ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Estates Officer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProperty(Guid id)
        {
            try
            {
                var userId = HttpContext.GetUserId().ToString();
                var property = await _propertyService.GetPropertyByIdAsync(id);

                if (property.ApprovalStatus != "Pending")
                {
                    return BadRequest(ApiResponse.BadRequest("Can only delete pending properties"));
                }

                await _propertyService.DeletePropertyAsync(id, userId);
                _logger.LogInformation("Pending property deleted: {Id} by {UserId}", id, userId);
                
                var responseData = new 
                { 
                    propertyId = id,
                    deletedAt = DateTime.UtcNow,
                    deletedBy = userId,
                    message = "Property deleted successfully"
                };
                
                return Ok(ApiResponse<object>.Success(responseData, "Property deleted successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Property not found for deletion: {Id}", id);
                return NotFound(ApiResponse.NotFound(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting property: {Id}", id);
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while deleting the property"));
            }
        }
        // Amenity Endpoints
        [HttpPost("amenities")]
        [Authorize(Roles = "Admin,Property Manager")]
        public async Task<ActionResult<Amenity>> CreateAmenity([FromBody] CreateAmenityDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid amenity creation request: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found.");
                var amenity = await _amenityService.CreateAmenityAsync(dto, userId);
            
                return CreatedAtAction(nameof(GetAmenityById), new { id = amenity.Id }, amenity);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized attempt to create amenity: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create amenity: {Name}", dto.Name);
                return StatusCode(500, "An error occurred while creating the amenity.");
            }
        }

        [HttpGet("amenities/{id}")]
        public async Task<ActionResult<Amenity>> GetAmenityById(Guid id)
        {
            try
            {
                var amenity = await _amenityService.GetAmenityByIdAsync(id);
                return Ok(amenity);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Amenity not found: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve amenity: {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the amenity.");
            }
        }

        [HttpGet("amenities")]
        public async Task<ActionResult<List<Amenity>>> GetAllAmenities()
        {
            try
            {
                var amenities = await _amenityService.GetAllAmenitiesAsync();
                return Ok(amenities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all amenities");
                return StatusCode(500, "An error occurred while retrieving amenities.");
            }
        }

        [HttpPut("amenities/{id}")]
        [Authorize(Roles = "Admin,Property Manager")]
        public async Task<IActionResult> UpdateAmenity(Guid id, [FromBody] CreateAmenityDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid amenity update request: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found.");
                await _amenityService.UpdateAmenityAsync(id, dto, userId);
                _logger.LogInformation("Amenity updated by {UserId}: {AmenityId}", userId, id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Amenity not found for update: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized attempt to update amenity: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update amenity: {Id}", id);
                return StatusCode(500, "An error occurred while updating the amenity.");
            }
        }

        [HttpDelete("amenities/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAmenity(Guid id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found.");
                await _amenityService.DeleteAmenityAsync(id, userId);
                _logger.LogInformation("Amenity deleted by {UserId}: {AmenityId}", userId, id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Amenity not found for deletion: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized attempt to delete amenity: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete amenity: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the amenity.");
            }
        }

        [HttpPost("properties/{propertyId}/amenities/{amenityId}")]
        [Authorize(Roles = "Admin,Property Manager")]
        public async Task<IActionResult> AssociateAmenityWithProperty(Guid propertyId, Guid amenityId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found.");
                await _amenityService.AssociateAmenityWithPropertyAsync(amenityId, propertyId, userId);
                _logger.LogInformation("Amenity {AmenityId} associated with property {PropertyId} by {UserId}", amenityId, propertyId, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Association failed: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized attempt to associate amenity: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to associate amenity {AmenityId} with property {PropertyId}", amenityId, propertyId);
                return StatusCode(500, "An error occurred while associating the amenity.");
            }
        }

        [HttpDelete("properties/{propertyId}/amenities/{amenityId}")]
        [Authorize(Roles = "Admin,Property Manager")]
        public async Task<IActionResult> DissociateAmenityFromProperty(Guid propertyId, Guid amenityId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found.");
                await _amenityService.DissociateAmenityFromPropertyAsync(amenityId, propertyId, userId);
                _logger.LogInformation("Amenity {AmenityId} dissociated from property {PropertyId} by {UserId}", amenityId, propertyId, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Dissociation failed: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized attempt to dissociate amenity: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dissociate amenity {AmenityId} from property {PropertyId}", amenityId, propertyId);
                return StatusCode(500, "An error occurred while dissociating the amenity.");
            }
        }

        /// <summary>
        /// Retrieves property statistics including counts of booked, leased, rented, and bought properties.
        /// </summary>
        /// <returns>Property statistics with counts for each status</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Sales Officer,Sales Manager,Tenant,Property Manager,Estates Officer")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPropertyStatistics()
        {
            try
            {
                var properties = await _propertyService.GetAllPropertiesAsync();
                var approvedProperties = properties.Where(p => p.ApprovalStatus == "Approved").ToList();

                // Initialize counters
                var statistics = new
                {
                    TotalProperties = approvedProperties.Count,
                    Booked = 0,
                    Leased = 0,
                    Rented = 0,
                    Bought = 0,
                    Available = 0,
                    LastUpdated = DateTime.UtcNow
                };

                // Count properties based on their actual status
                foreach (var property in approvedProperties)
                {
                    // Check if property has active lease agreements (this covers both rental and lease)
                    if (property.LeaseAgreements?.Any(la => la.Status == "Active") == true)
                    {
                        // For now, we'll count all active lease agreements as "Leased"
                        // In a real system, you might want to differentiate between rental and lease
                        statistics = new
                        {
                            statistics.TotalProperties,
                            statistics.Booked,
                            Leased = statistics.Leased + 1,
                            statistics.Rented,
                            statistics.Bought,
                            statistics.Available,
                            statistics.LastUpdated
                        };
                    }
                    // Check if property has been bought/sold
                    else if (property.OwnershipStatus == "Sold")
                    {
                        statistics = new
                        {
                            statistics.TotalProperties,
                            statistics.Booked,
                            statistics.Leased,
                            statistics.Rented,
                            Bought = statistics.Bought + 1,
                            statistics.Available,
                            statistics.LastUpdated
                        };
                    }
                    // If none of the above, property is available
                    else
                    {
                        statistics = new
                        {
                            statistics.TotalProperties,
                            statistics.Booked,
                            statistics.Leased,
                            statistics.Rented,
                            statistics.Bought,
                            Available = statistics.Available + 1,
                            statistics.LastUpdated
                        };
                    }
                }

                _logger.LogInformation("Retrieved property statistics: {Statistics}", statistics);
                return Ok(ApiResponse<object>.Success(statistics, "Property statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching property statistics");
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while fetching property statistics"));
            }
        }

        /// <summary>
        /// Test endpoint to debug authentication and token issues.
        /// </summary>
        /// <returns>Authentication status and token information</returns>
        [HttpGet("auth-test")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult TestAuthentication()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            var hasAuthHeader = !string.IsNullOrEmpty(authHeader);
            var isBearerToken = hasAuthHeader && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
            
            var result = new
            {
                HasAuthorizationHeader = hasAuthHeader,
                IsBearerToken = isBearerToken,
                AuthorizationHeader = hasAuthHeader ? (authHeader.Length > 50 ? authHeader.Substring(0, 50) + "..." : authHeader) : null,
                TokenLength = isBearerToken ? authHeader.Substring("Bearer ".Length).Trim().Length : 0,
                UserAuthenticated = User.Identity?.IsAuthenticated ?? false,
                UserName = User.Identity?.Name,
                Claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList()
            };

            return Ok(ApiResponse<object>.Success(result, "Authentication test completed"));
        }

        private CreatePropertyDto MapToUpdateDto(Property property, PropertyApprovalDto decision)
        {
            // Assuming CreatePropertyDto can be used for updates
            return new CreatePropertyDto
            {
                Name = property.Name,
                Address = property.Address,
                OwnerId = property.OwnerId,
                PropertyType = property.PropertyType,
                FairValue = property.FairValue,
                InsurableValue = property.InsurableValue,
                OwnershipStatus = property.OwnershipStatus,
                SalePrice = property.SalePrice,
                IsRentable = property.IsRentable,
                IsSaleable = property.IsSaleable,
                RentPrice = property.RentPrice,
                ApprovalStatus = decision.Status,
                // Type-specific fields would need to be mapped based on actual property type
                // This is a simplified version; in practice, you'd need to map all type-specific fields
            };
        }
    }

    public class PropertyApprovalDto
    {
        public required string Status { get; set; } // "Approved", "Rejected", "MoreInfo"
        public required string Comments { get; set; } // Optional comments from Property Manager
    }

    public class ErrorResponse
    {
        public required string Error { get; set; }
    }
}
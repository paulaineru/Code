// TenantManagementService/Controllers/TenantController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TenantManagementService.Services;
using SharedKernel.Models;
using SharedKernel.Dto.Tenants;
using SharedKernel.Services;
using SharedKernel.Utilities;
using SharedKernel.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedKernel.Models.Tenants;
using System.Linq;

namespace TenantManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantController : ControllerBase
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IRenewalRequestRepository _renewalRepository;
        private readonly ITerminationProcessRepository _terminationRepository;
        private readonly ITenantNotificationManager _notificationManager;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<TenantController> _logger;
        private readonly IUserService _userService;
        private readonly IPropertyService _propertyService;


        public TenantController(
            ITenantRepository tenantRepository,
            IBookingRepository bookingRepository,
            IRenewalRequestRepository renewalRepository,
            ITerminationProcessRepository terminationRepository,
            ITenantNotificationManager notificationManager,
            IAuditLogService auditLogService,
            ILogger<TenantController> logger,
            IUserService userService,
            IPropertyService propertyService)
        {
            _tenantRepository = tenantRepository;
            _bookingRepository = bookingRepository;
            _renewalRepository = renewalRepository;
            _terminationRepository = terminationRepository;
            _notificationManager = notificationManager;
            _auditLogService = auditLogService;
            _userService = userService;
            _propertyService = propertyService;
            _logger = logger;
        }

        [HttpGet("types")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<TenantTypeInfo>), StatusCodes.Status200OK)]
        public IActionResult GetTenantTypes()
        {
            try
            {
                var tenantTypes = Enum.GetValues(typeof(TenantType))
                    .Cast<TenantType>()
                    .Select(tt => new TenantTypeInfo
                    {
                        Id = (int)tt,
                        Name = tt.ToString(),
                        Description = GetTenantTypeDescription(tt)
                    })
                    .ToList();

                return Ok(ApiResponse<List<TenantTypeInfo>>.Success(tenantTypes, "Tenant types retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant types");
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while retrieving tenant types"));
            }
        }

        private string GetTenantTypeDescription(TenantType tenantType)
        {
            return tenantType switch
            {
                TenantType.Individual => "Personal/individual tenants - Single person renting a property",
                TenantType.CorporateOrganisation => "Business/corporate entities - Companies, businesses, or organizations",
                TenantType.GovernmentAgency => "Government departments or agencies - Public sector organizations",
                _ => "Unknown tenant type"
            };
        }

        [HttpPost]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        [ProducesResponseType(typeof(Tenant), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto dto)
        {
            try
            {
                var tenant = new Tenant
                {
                    Name = dto.Name,
                    PrimaryEmail = dto.PrimaryEmail,
                    TaxIdentificationNumber = dto.TaxIdentificationNumber,
                    PrimaryTelephone = dto.PrimaryTelephone,
                    BillingEntity = dto.BillingEntity,
                    TenantType = dto.TenantType,
                    Status = Status.Inactive,
                    NotificationPreferences = NotificationPreferences.Email,
                    BusinessRegistrationNumber = dto.BusinessRegistrationNumber,
                    Contacts = dto.Contacts?.Select(c => new ContactDetail
                    {
                        Type = c.Type,
                        Value = c.Value
                    }).ToList()
                };

                await _tenantRepository.AddAsync(tenant);

                // Record audit log
                await _auditLogService.RecordActionAsync(
                    action: "TenantCreated",
                    entityId: tenant.Id,
                    userId: HttpContext.GetUserId(),
                    details: $"Tenant created: {tenant.Name}",
                    moduleId: Guid.Empty
                );

                return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, 
                    ApiResponse<Tenant>.Success(tenant, "Tenant created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant");
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while creating the tenant"));
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        public async Task<IActionResult> GetTenant([FromRoute] Guid id)
        {
            try
            {
                var tenant = await _tenantRepository.GetByIdAsync(id);

                if (tenant == null)
                {
                    return NotFound(ApiResponse.NotFound("Tenant not found"));
                }

                return Ok(ApiResponse<Tenant>.Success(tenant, "Tenant retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tenant");
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while fetching the tenant"));
            }
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantDto dto)
        {
            try
            {
                var tenant = await _tenantRepository.GetByIdAsync(id);

                if (tenant == null)
                {
                    return NotFound(ApiResponse.NotFound("Tenant not found"));
                }

                tenant.Name = dto.Name ?? tenant.Name;
                tenant.PrimaryEmail = dto.Email ?? tenant.PrimaryEmail;
                tenant.TaxIdentificationNumber = dto.TaxIdentificationNumber ?? tenant.TaxIdentificationNumber;
                tenant.BillingEntity = dto.BillingEntity ?? tenant.BillingEntity;

                await _tenantRepository.UpdateAsync(tenant);

                // Record audit log
                await _auditLogService.RecordActionAsync(
                    action: "TenantUpdated",
                    entityId: tenant.Id,
                    userId: HttpContext.GetUserId(),
                    details: $"Tenant updated: {tenant.Name}",
                    moduleId: Guid.Empty
                );

                var responseData = new 
                { 
                    tenantId = tenant.Id,
                    updatedAt = DateTime.UtcNow,
                    updatedBy = HttpContext.GetUserId(),
                    message = "Tenant information updated successfully"
                };
                
                return Ok(ApiResponse<object>.Success(responseData, "Tenant updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Tenant not found for update: {Id}", id);
                return NotFound(ApiResponse.NotFound(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid update data for tenant {Id}: {Error}", id, ex.Message);
                return BadRequest(ApiResponse.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tenant: {Id}", id);
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while updating the tenant"));
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTenant(Guid id)
        {
            try
            {
                var tenant = await _tenantRepository.GetByIdAsync(id);

                if (tenant == null)
                {
                    return NotFound(ApiResponse.NotFound("Tenant not found"));
                }

                await _tenantRepository.DeleteAsync(tenant);

                // Record audit log
                await _auditLogService.RecordActionAsync(
                    action: "TenantDeleted",
                    entityId: tenant.Id,
                    userId: HttpContext.GetUserId(),
                    details: $"Tenant deleted: {tenant.Name}",
                    moduleId: Guid.Empty
                );

                var responseData = new 
                { 
                    tenantId = tenant.Id,
                    deletedAt = DateTime.UtcNow,
                    deletedBy = HttpContext.GetUserId(),
                    message = "Tenant deleted successfully"
                };
                
                return Ok(ApiResponse<object>.Success(responseData, "Tenant deleted successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Tenant not found for deletion: {Id}", id);
                return NotFound(ApiResponse.NotFound(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tenant: {Id}", id);
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while deleting the tenant"));
            }
        }
        [HttpPost("{leaseId}/initiate-termination")]
        [Authorize(Roles = "Tenant,Property Manager")]
        public async Task<IActionResult> InitiateTermination([FromRoute] Guid leaseId, [FromBody] TerminationRequestDto dto) // Use the correct DTO
        {
            try
            {
                var process = new TerminationProcess
                {
                    LeaseAgreementId = leaseId,
                    TenantId = HttpContext.GetUserId(),
                    Status = TerminationStatus.Initiated,
                    OutstandingAmount = dto.OutstandingAmount,
                    SecurityDepositDeduction = dto.SecurityDepositDeduction
                };

                await _terminationRepository.AddAsync(process);

                // Notify admin and property manager
                var adminEmail = ConfigurationHelper.GetAdminEmailFromConfiguration();
                var propertyManagerEmailTask = ConfigurationHelper.GetPropertyManagerEmailForLease(_userService, dto.LeaseAgreementId);
                var propertyManagerEmail = await propertyManagerEmailTask;


                await _notificationManager.NotifyTenantAsync(new SharedKernel.Dto.NotificationRequest
                {
                    TenantId = HttpContext.GetUserId(),
                    Subject = "Lease Termination Initiated",
                    Message = $"The lease for property ID {leaseId} has been initiated for termination.",
                    Type = "LeaseTermination"
                });

                // Record audit log
                await _auditLogService.RecordActionAsync(
                    action: "TerminationInitiated",
                    entityId: process.Id,
                    userId: HttpContext.GetUserId(),
                    details: $"Lease termination initiated for property ID {leaseId}",
                    moduleId: Guid.Empty
                );

                return CreatedAtAction(nameof(TerminationController.GetTerminationProcess), new { id = process.Id }, process);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating termination");
                return StatusCode(500, new { error = "An error occurred while initiating the termination process." });
            }
        }

        [HttpPost("{id}/terminate-lease")]
        [Authorize(Roles = "Tenant,Property Manager")]
        public async Task<IActionResult> TerminateLease([FromRoute] Guid id, [FromBody] TerminationRequestDto dto)
        {
            try
            {
                var terminationProcess = new TerminationProcess
                {
                    LeaseAgreementId = dto.LeaseAgreementId, // Ensure this field exists in DTO
                    TenantId = HttpContext.GetUserId(),
                    Status = TerminationStatus.Initiated,
                    OutstandingAmount = dto.OutstandingAmount,
                    SecurityDepositDeduction = dto.SecurityDepositDeduction
                };

                await _terminationRepository.AddAsync(terminationProcess);

                // Fetch property manager email for the lease
                var propertyManagerEmail = await ConfigurationHelper.GetPropertyManagerEmailForLease(_userService, dto.LeaseAgreementId); // Pass leaseId here

                // Notify admin and property manager
                var adminEmail = ConfigurationHelper.GetAdminEmailFromConfiguration();
                await _notificationManager.NotifyTenantAsync(new SharedKernel.Dto.NotificationRequest
                {
                    TenantId = HttpContext.GetUserId(),
                    Subject = "Lease Termination Initiated",
                    Message = $"The tenant has initiated termination for lease ID {dto.LeaseAgreementId}.",
                    Type = "LeaseTermination"
                });

                // Record audit log
                await _auditLogService.RecordActionAsync(
                    action: "LeaseTerminationInitiated",
                    entityId: terminationProcess.Id,
                    userId: HttpContext.GetUserId(),
                    details: $"Lease termination initiated for lease ID {dto.LeaseAgreementId}",
                    moduleId: Guid.Empty
                );

                return CreatedAtAction(nameof(TerminationController.GetTerminationProcess), new { id = terminationProcess.Id }, terminationProcess);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating lease termination");
                return StatusCode(500, new { error = "An error occurred while initiating the lease termination process." });
            }
        }

    }
}
// TenantManagementService/Controllers/TerminationController.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedKernel.Models.Tenants;
using SharedKernel.Dto.Tenants;
using SharedKernel.Services;
using SharedKernel.Utilities;
using SharedKernel.Dto;

namespace TenantManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TerminationController : ControllerBase
    {
        private readonly ITerminationProcessRepository _terminationRepository;
        private readonly IPropertyService _propertyService; // Add this line
        private readonly IUserService _userService; // Add this line
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<TerminationController> _logger;

        public TerminationController(
            ITerminationProcessRepository terminationRepository,
            IPropertyService propertyService, // Inject IPropertyService
            IUserService userService, // Inject IUserService
            INotificationService notificationService,
            IAuditLogService auditLogService,
            ILogger<TerminationController> logger)
        {
            _terminationRepository = terminationRepository;
            _propertyService = propertyService;
            _userService = userService;
            _notificationService = notificationService;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        [HttpPost("submit-termination")]
        [Authorize(Roles = "Tenant,Estates Officer,Property Manager")]
        [ProducesResponseType(typeof(TerminationProcess), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitTerminationRequest([FromBody] TerminationRequestDto dto)
        {
            try
            {
                // Fetch property details
                var property = await _propertyService.GetPropertyByIdAsync(dto.PropertyId); // Use PropertyId from DTO

                if (string.IsNullOrEmpty(property.PropertyManagerId))
                {
                    return NotFound(ApiResponse.NotFound("No property manager is assigned to this property."));
                }
                var process = new TerminationProcess
                {
                    LeaseAgreementId = dto.LeaseAgreementId, // Use LeaseAgreementId from DTO
                    TenantId = HttpContext.GetUserId(),
                    Status = TerminationStatus.Initiated,
                    OutstandingAmount = dto.OutstandingAmount,
                    SecurityDepositDeduction = dto.SecurityDepositDeduction
                };

                await _terminationRepository.AddAsync(process);

                // Notify admin and property manager
                var adminEmail = ConfigurationHelper.GetAdminEmailFromConfiguration();
                var managerEmail = await ConfigurationHelper.GetPropertyManagerEmailForProperty(_userService, Guid.Parse(property.PropertyManagerId));

                await _notificationService.SendCriticalActionNotificationAsync(
                    adminEmail,
                    managerEmail,
                    "Lease Termination Initiated",
                    $"The tenant has initiated termination for lease ID {dto.LeaseAgreementId}."
                );

                // Record audit log
                await _auditLogService.RecordActionAsync(
                    "TerminationInitiated",
                    dto.PropertyId,
                    HttpContext.GetUserId(),
                    $"Lease termination initiated for property ID {dto.PropertyId}",
                    dto.PropertyId
                );
                return CreatedAtAction(nameof(GetTerminationProcess), new { id = process.Id }, ApiResponse<TerminationProcess>.Success(process, "Termination process initiated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating termination");
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while initiating the termination process."));
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Tenant,Estates Officer,Property Manager")]
        public async Task<IActionResult> GetTerminationProcess(Guid id)
        {
            try
            {
                var process = await _terminationRepository.GetByIdAsync(id);

                if (process == null)
                {
                    return NotFound(ApiResponse.NotFound("Termination process not found."));
                }

                return Ok(ApiResponse<TerminationProcess>.Success(process, "Termination process retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching termination process");
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while fetching the termination process."));
            }
        }

        
    }
}
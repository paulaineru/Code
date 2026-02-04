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
using TenantManagementService.Services;

namespace TenantManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RenewalController : ControllerBase
    {
        private readonly IRenewalRequestRepository _renewalRepository;
        private readonly IPropertyService _propertyService;
        private readonly IUserService _userService; 
        private readonly ITenantNotificationManager _notificationManager;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<RenewalController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RenewalController(
            IRenewalRequestRepository renewalRepository,
            IPropertyService propertyService,
            IUserService userService,
            ITenantNotificationManager notificationManager,
            IAuditLogService auditLogService,
            ILogger<RenewalController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _renewalRepository = renewalRepository;
            _propertyService = propertyService;
            _userService = userService;
            _notificationManager = notificationManager;
            _auditLogService = auditLogService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("submit-renewal")]
        [Authorize(Roles = "Tenant,Estates Officer,Property Manager")]
        [ProducesResponseType(typeof(RenewalRequest), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitRenewalRequest([FromBody] SubmitRenewalRequestDto dto)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

                // Fetch the property details
                var property = await _propertyService.GetPropertyDetailByIdAsync(dto.PropertyId,token);
                

                /*if (property == null || !property.PropertyManagerId.HasValue)
                {
                    throw new KeyNotFoundException("No property manager is assigned to this property.");
                }*/

                var request = new RenewalRequest
                {
                    LeaseAgreementId = dto.PropertyId,
                    TenantId = dto.TenantId,
                    NewTerms = dto.NewTerms,
                    NewMonthlyRent = dto.NewMonthlyRent,
                    Status = RenewalStatus.Pending
                };

                await _renewalRepository.AddAsync(request);

                
                
                // Notify admin and property manager
                var adminEmail = ConfigurationHelper.GetAdminEmailFromConfiguration();
                //var managerEmail = await ConfigurationHelper.GetPropertyManagerEmailForProperty(_userService, property.PropertyManagerId);
                /*
                await _notificationManager.NotifyTenantAsync(new SharedKernel.Dto.NotificationRequest
                {
                    TenantId = HttpContext.GetUserId(),
                    Subject = "Lease Renewal Request",
                    Message = $"A tenant has requested to renew the lease for property ID {dto.PropertyId}.",
                    Type = "LeaseRenewal"
                });*/

                // Record audit log
                await _auditLogService.RecordActionAsync(
                    action: "RenewalRequested",
                    entityId: dto.PropertyId,
                    userId: HttpContext.GetUserId(),
                    details: $"Lease renewal requested for property ID {dto.PropertyId}",
                    moduleId: Guid.Empty
                );


                return CreatedAtAction(nameof(GetRenewalRequest), new { id = request.Id }, ApiResponse<RenewalRequest>.Success(request, "Renewal request submitted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting renewal request");
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while submitting the renewal request."));
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Tenant,Estates Officer,Property Manager")]
        public async Task<IActionResult> GetRenewalRequest(Guid id)
        {
            var request = await _renewalRepository.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound(ApiResponse.NotFound("Renewal request not found."));
            }

            return Ok(ApiResponse<RenewalRequest>.Success(request, "Renewal request retrieved successfully"));
        }
    }
}
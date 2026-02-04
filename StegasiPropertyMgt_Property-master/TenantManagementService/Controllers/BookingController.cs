using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedKernel.Models;
using SharedKernel.Services;
using TenantManagementService.Repository;
using TenantManagementService.Services;
using SharedKernel.Utilities;
using SharedKernel.Dto;
using SharedKernel.Dto.Tenants;

namespace TenantManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingService _bookingService;
        private readonly IPropertyService _propertyService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<BookingController> _logger;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingController(
            IBookingRepository bookingRepository,
            IPropertyService propertyService,
            IAuditLogService auditLogService,
            IUserService userService,
            INotificationService notificationService,
            IBookingService bookingService,
            ILogger<BookingController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _bookingRepository = bookingRepository;
            _propertyService = propertyService;
            _auditLogService = auditLogService;
            _userService = userService;
            _notificationService = notificationService;
            _bookingService = bookingService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBookingAsync([FromBody] BookPropertyRequest request)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                var booking = await _bookingService.BookPropertyAsync(request,request.TenantId, token);
                _logger.LogInformation("Created booking {BookingId} for property {PropertyId}", booking.Id, request.PropertyId);
                return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, ApiResponse<Booking>.Success(booking, "Booking created successfully"));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to create booking for property {PropertyId}", request.PropertyId);
                return StatusCode(500, ApiResponse.InternalServerError("An unexpected error occurred."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse.NotFound(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.BadRequest(ex.Message));
            }
        }

        // Get a booking by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(Guid id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                    return NotFound(ApiResponse.NotFound("Booking not found"));

                return Ok(ApiResponse<Booking>.Success(booking, "Booking retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch booking {BookingId}", id);
                return StatusCode(500, ApiResponse.InternalServerError("An unexpected error occurred."));
            }
        }

        // Get all bookings for a tenant
        [HttpGet("tenant/{tenantId}")]
        public async Task<IActionResult> GetBookingsByTenantAsync(Guid tenantId)
        {
            try
            {
                var bookings = await _bookingService.GetBookingsByTenantAsync(tenantId);
                if (bookings == null || !bookings.Any())
                    return Ok(ApiResponse<IEnumerable<Booking>>.Success(new List<Booking>(), "No bookings found for tenant."));

                return Ok(ApiResponse<IEnumerable<Booking>>.Success(bookings, "Bookings retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch bookings for tenant {TenantId}", tenantId);
                return StatusCode(500, ApiResponse.InternalServerError("An unexpected error occurred."));
            }
        }

        // Update booking status
        [HttpPut("{id}/update-status")]
        public async Task<IActionResult> UpdateBookingStatusAsync(Guid id, [FromBody] UpdateBookingStatusRequest request)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                    return NotFound(ApiResponse.NotFound("Booking not found"));

                booking.Status = request.NewStatus;
                await _bookingService.UpdateBookingAsync(booking);

                _logger.LogInformation("Updated booking {BookingId} status to {Status}", id, request.NewStatus);
                return Ok(ApiResponse<object>.Success(new { bookingId = id, newStatus = request.NewStatus }, "Booking status updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update booking status {BookingId}", id);
                return StatusCode(500, ApiResponse.InternalServerError("An unexpected error occurred."));
            }
        }

    }
}
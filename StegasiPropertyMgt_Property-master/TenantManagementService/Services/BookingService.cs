using SharedKernel.Models;
using SharedKernel.Dto;
using SharedKernel.Dto.Tenants;
using SharedKernel.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenantManagementService.Repository;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Linq;

namespace TenantManagementService.Services
{

    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IPropertyService _propertyService;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private readonly IBillingClient _billingClient;
        private readonly ILogger<BookingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly TenantDbContext _context;

        public BookingService(
            IBookingRepository bookingRepository,
            IPropertyService propertyService,
            INotificationService notificationService,
            IAuditLogService auditLogService,
            IBillingClient billingClient,
            ILogger<BookingService> logger, // Corrected from BillingService to BookingService
            IHttpContextAccessor httpContextAccessor,
            IUserService userService,
            TenantDbContext context)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _billingClient = billingClient ?? throw new ArgumentNullException(nameof(billingClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Booking> GetBookingByIdAsync(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                _logger.LogWarning("Booking not found with ID: {BookingId}", id);
                throw new KeyNotFoundException($"Booking with ID {id} not found.");
            }
            return booking;
        }

        public async Task<Booking> GetBookingByIdAsync(string bookingIdString)
        {
            if (!Guid.TryParse(bookingIdString, out Guid bookingId))
            {
                _logger.LogWarning("Invalid booking ID format: {BookingIdString}", bookingIdString);
                throw new ArgumentException("Invalid booking ID format.", nameof(bookingIdString));
            }
            return await GetBookingByIdAsync(bookingId);
        }

        public async Task<Booking> BookPropertyAsync(BookPropertyRequest dto, Guid? tenantId = null,string token = null)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Booking request cannot be null.");

            ValidateBookPropertyDto(dto);

            Guid effectiveTenantId = tenantId ?? GetUserIdFromClaims();

            var property = await _propertyService.GetPropertyDetailByIdAsync(dto.PropertyId,token);
            
            // Add debug logging
            _logger.LogInformation("Property details for {PropertyId}: IsRentable={IsRentable}, Status={Status}, Type={Type}", 
                dto.PropertyId, property?.IsRentable, property?.Status, property?.Type);
            
            if (property == null || !property.IsRentable)
            {
                _logger.LogWarning("Property {PropertyId} is not available for booking. Property is null: {IsNull}, IsRentable: {IsRentable}", 
                    dto.PropertyId, property == null, property?.IsRentable);
                throw new InvalidOperationException($"Property {dto.PropertyId} is not available for booking.");
            }

            // Check for existing bookings that overlap with the requested date range
            var existingBookings = await _bookingRepository.GetExistingBookingsForPropertyAsync(dto.PropertyId, dto.StartDate, dto.EndDate);
            if (existingBookings.Any())
            {
                _logger.LogWarning("Property {PropertyId} is already booked for the requested period {StartDate} to {EndDate}. Found {Count} existing bookings.", 
                    dto.PropertyId, dto.StartDate, dto.EndDate, existingBookings.Count);
                throw new InvalidOperationException($"Property {dto.PropertyId} is already booked for the requested period from {dto.StartDate:yyyy-MM-dd} to {dto.EndDate:yyyy-MM-dd}. Please choose different dates.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    PropertyId = dto.PropertyId,
                    TenantId = effectiveTenantId,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,

                };

                await _bookingRepository.AddBookingAsync(booking);
                await _context.SaveChangesAsync();

                //await GenerateInvoiceForBookingAsync(booking);
                //await _notificationService.SendCriticalActionNotificationAsync(effectiveTenantId, $"Booking confirmed for property {property.Name} from {dto.StartDate} to {dto.EndDate}.");
                await _auditLogService.RecordActionAsync(
                    "BookingCreated",
                    dto.PropertyId,
                    effectiveTenantId,
                    $"Booking {booking.Id} created for property {dto.PropertyId} by tenant {effectiveTenantId}",
                    Guid.Empty // Using Guid.Empty as moduleId since we don't have a specific module ID
                );

                await transaction.CommitAsync();
                _logger.LogInformation("Booking {BookingId} successfully created for tenant {TenantId}.", booking.Id, effectiveTenantId);
                return booking;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to book property {PropertyId} for tenant {TenantId}", dto.PropertyId, effectiveTenantId);
                throw;
            }
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            try
            {
                return await _bookingRepository.GetAllBookingsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all bookings.");
                throw;
            }
        }

        public async Task UpdateBookingAsync(Booking booking)
        {
            if (booking == null)
                throw new ArgumentNullException(nameof(booking), "Booking cannot be null.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingBooking = await GetBookingByIdAsync(booking.Id);
                if (existingBooking == null)
                    throw new KeyNotFoundException($"Booking {booking.Id} not found for update.");

                await _bookingRepository.UpdateAsync(booking);
                await _context.SaveChangesAsync();

                await _auditLogService.RecordActionAsync(
                    "BookingUpdated",
                    booking.PropertyId,
                    booking.TenantId,
                    $"Booking {booking.Id} updated for property {booking.PropertyId} by tenant {booking.TenantId}",
                    Guid.Empty // Using Guid.Empty as moduleId since we don't have a specific module ID
                );
                await transaction.CommitAsync();
                _logger.LogInformation("Booking {BookingId} successfully updated.", booking.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update booking {BookingId}", booking.Id);
                throw;
            }
        }

        public async Task<List<Booking>> GetBookingsByTenantAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

            try
            {
                return await _bookingRepository.GetByTenantIdAsync(tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve bookings for tenant {TenantId}", tenantId);
                throw;
            }
        }

        private async Task GenerateInvoiceForBookingAsync(Booking booking)
        {
            try
            {
                var request = new GenerateInvoiceRequest
                {
                    BookingId = booking.Id,
                    PropertyId = booking.PropertyId,
                    TenantId = booking.TenantId,
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate
                };

                await _billingClient.GenerateInvoiceAsync(request);
                _logger.LogInformation("Invoice generated for booking {BookingId}", booking.Id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to generate invoice for booking {BookingId}", booking.Id);
                throw new InvalidOperationException("Invoice generation failed.", ex);
            }
        }

        private Guid GetUserIdFromClaims()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                _logger.LogWarning("User ID not found or invalid in claims.");
                throw new InvalidOperationException("User ID not found or invalid in claims.");
            }
            return userId;
        }

        private void ValidateBookPropertyDto(BookPropertyDto dto)
        {
            if (dto.PropertyId == Guid.Empty)
                throw new ArgumentException("Property ID cannot be empty.", nameof(dto.PropertyId));
            if (dto.StartDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Start date cannot be in the past.", nameof(dto.StartDate));
            if (dto.EndDate <= dto.StartDate)
                throw new ArgumentException("End date must be after start date.", nameof(dto.EndDate));
        }
        private void ValidateBookPropertyDto(BookPropertyRequest dto)
        {
            if (dto.PropertyId == Guid.Empty)
                throw new ArgumentException("Property ID cannot be empty.", nameof(dto.PropertyId));
            if (dto.StartDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Start date cannot be in the past.", nameof(dto.StartDate));
            if (dto.EndDate <= dto.StartDate)
                throw new ArgumentException("End date must be after start date.", nameof(dto.EndDate));
        }
        private Guid? ParseNullableGuidId(string idString)
        {
            if (string.IsNullOrEmpty(idString))
                return null;

            return Guid.TryParse(idString, out Guid result) ? result : throw new FormatException("Invalid GUID format.");
        }
    }
}
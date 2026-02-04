using SharedKernel.Models;
using SharedKernel.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


namespace TenantManagementService.Services
{
    /*

    public class TenantBillingService : ITenantBillingService
    {
        private readonly IBillingRepository _billingRepository;
        private readonly IBookingService _bookingService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TenantBillingService> _logger;

        public TenantBillingService(
            IBillingRepository billingRepository,
            IBookingService bookingService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TenantBillingService> logger)
        {
            _billingRepository = billingRepository;
            _bookingService = bookingService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Invoice> GenerateInvoiceForBookingAsync(Guid bookingId)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                var invoice = new Invoice
                {
                    BookingId = bookingId,
                    TenantId = booking.TenantId,
                    Amount = booking.CalculateTotalRent(), // Example method
                    DueDate = DateTime.Now.AddMonths(1)
                };

                await _billingRepository.AddAsync(invoice); // Ensure this method exists

                // Update booking status to Billed
                booking.Status = BookingStatus.Billed;
                await _bookingService.UpdateBookingAsync(booking);

                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate invoice for booking {BookingId}", bookingId);
                throw new InvalidOperationException("Invoice generation failed", ex);
            }
        }

        public async Task<List<Invoice>> GetInvoicesByTenantAsync(Guid tenantId)
        {
            return await _billingRepository.GetInvoicesByTenantAsync(tenantId); // Ensure this method exists
        }

        public async Task UpdateBookingStatusToBilledAsync(Guid bookingId)
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            booking.Status = BookingStatus.Billed;
            await _bookingService.UpdateBookingAsync(booking);
        }
    }*/
}
using SharedKernel.Models;
using SharedKernel.Dto;
using SharedKernel.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillingService.Data;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services
{
    public class BillingService : IBillingService
    {
        private readonly IBillingRepository _billingRepository;
        private readonly IPropertyClient _propertyClient;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<BillingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BillingDbContext _context;
        private readonly Guid _moduleId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Billing module ID

        public BillingService(
            IBillingRepository billingRepository,
            IPropertyClient propertyClient,
            IUserService userService,
            INotificationService notificationService,
            IAuditLogService auditLogService,
            ILogger<BillingService> logger,
            IHttpContextAccessor httpContextAccessor,
            BillingDbContext context
           )
        {
            _billingRepository = billingRepository ?? throw new ArgumentNullException(nameof(billingRepository));
            _propertyClient = propertyClient ?? throw new ArgumentNullException(nameof(propertyClient));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Invoice> GenerateInvoiceAsync(Guid propertyId, Guid tenantId, GenerateInvoiceDto dto)
        {
            if (propertyId == Guid.Empty) throw new ArgumentException("Property ID cannot be empty.", nameof(propertyId));
            if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.StartDate >= dto.EndDate) throw new ArgumentException("Start date must be before end date.", nameof(dto.StartDate));

            // Validate property exists
            var propertyExists = await _propertyClient.ValidatePropertyAsync(propertyId);
            if (!propertyExists) throw new InvalidOperationException($"Property with ID {propertyId} not found.");

            // Get property details
            var property = await _propertyClient.GetPropertyAsync(propertyId);
            if (property == null) throw new InvalidOperationException($"Failed to get property details for ID {propertyId}");

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Amount = (decimal)property.RentPrice,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                DueDate = dto.DueDate,
                Status = InvoiceStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Save invoice
            var savedInvoice = await _billingRepository.AddAsync(invoice);
            await NotifyAndLogInvoiceGeneratedAsync(savedInvoice);

            return savedInvoice;
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByTenantAsync(Guid tenantId)
        {
            return await _billingRepository.GetInvoicesByTenantIdAsync(tenantId);
        }

        public async Task<Payment> MakePaymentAsync(Guid invoiceId, MakePaymentDto dto)
        {
            if (invoiceId == Guid.Empty) throw new ArgumentException("Invoice ID cannot be empty.", nameof(invoiceId));
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.AmountPaid <= 0) throw new ArgumentException("Payment amount must be greater than zero.", nameof(dto.AmountPaid));

            var invoice = await _billingRepository.GetInvoiceByIdAsync(invoiceId, includePayments: true);
            if (invoice == null) throw new InvalidOperationException($"Invoice with ID {invoiceId} not found.");

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoiceId,
                AmountPaid = dto.AmountPaid,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = dto.PaymentMethod,
                Status = PaymentStatus.Processed
            };

            // Update invoice status
            invoice.Status = InvoiceStatus.Paid;
            await _billingRepository.UpdateInvoiceAsync(invoice);

            // Add payment
            await _billingRepository.AddPaymentAsync(payment);

            return payment;
        }

        public async Task<Invoice> GetInvoiceByIdAsync(Guid id)
        {
            return await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Payment>> GetPaymentsByInvoiceIdAsync(Guid invoiceId)
        {
            return await _context.Payments
                .Where(p => p.InvoiceId == invoiceId)
                .ToListAsync();
        }

        public async Task CancelInvoiceAsync(Guid invoiceId)
        {
            var invoice = await _billingRepository.GetInvoiceByIdAsync(invoiceId)
                ?? throw new KeyNotFoundException("Invoice not found.");

            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                throw new InvalidOperationException("Invoice is already cancelled.");
            }

            try
            {
                invoice.Status = InvoiceStatus.Cancelled;
                await _billingRepository.UpdateInvoiceAsync(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel invoice {InvoiceId}", invoiceId);
                throw new InvalidOperationException("Failed to cancel invoice.", ex);
            }

            await NotifyAndLogInvoiceCancelledAsync(invoice);
        }

        private async Task NotifyAndLogInvoiceGeneratedAsync(Invoice invoice)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(invoice.TenantId);
                var email = user?.Email ?? string.Empty;
                await _notificationService.SendNotificationAsync(
                    invoice.TenantId,
                    "Invoice Generated",
                    $"Your invoice for booking ID {invoice.BookingId} is ready. Amount due: ${invoice.Amount}.",
                    "INVOICE_GENERATED"
                );

                await _auditLogService.RecordActionAsync(
                    "InvoiceGenerated",
                    invoice.Id,
                    null,
                    $"Generated invoice for tenant {invoice.TenantId}",
                    _moduleId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification or log invoice generation for invoice {InvoiceId}", invoice.Id);
            }
        }

        private async Task NotifyAndLogInvoiceCancelledAsync(Invoice invoice)
        {
            try
            {
                await _notificationService.SendNotificationAsync(
                    invoice.TenantId,
                    "Invoice Cancelled",
                    $"Your invoice {invoice.Id} has been cancelled.",
                    "INVOICE_CANCELLED"
                );

                await _auditLogService.RecordActionAsync(
                    "InvoiceCancelled",
                    invoice.Id,
                    null,
                    $"Cancelled invoice {invoice.Id}",
                    _moduleId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification or log invoice cancellation for invoice {InvoiceId}", invoice.Id);
            }
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? "system";
        }

        public async Task<decimal> CalculateRentAmount(Guid propertyId, DateTime startDate, DateTime endDate)
        {
            var property = await _propertyClient.GetPropertyAsync(propertyId);
            if (property == null)
            {
                throw new InvalidOperationException($"Property with ID {propertyId} not found.");
            }

            var days = (endDate - startDate).Days;
            return (decimal)property.RentPrice * days;
        }

        public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
        {
            invoice.Id = Guid.NewGuid();
            invoice.CreatedAt = DateTime.UtcNow;
            invoice.LastUpdated = DateTime.UtcNow;
            invoice.Status = InvoiceStatus.Pending;

            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();

            await _auditLogService.RecordActionAsync(
                "Create",
                invoice.Id,
                null,
                $"Created invoice {invoice.Id} for tenant {invoice.TenantId}",
                _moduleId
            );

            return invoice;
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByTenantIdAsync(Guid tenantId)
        {
            return await _context.Invoices
                .Include(i => i.Payments)
                .Where(i => i.TenantId == tenantId)
                .ToListAsync();
        }

        public async Task<Payment> RecordPaymentAsync(Payment payment)
        {
            payment.Id = Guid.NewGuid();
            payment.PaidOn = DateTime.UtcNow;
            payment.Status = PaymentStatus.Processing;

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            await _auditLogService.RecordActionAsync(
                "Create",
                payment.Id,
                null,
                $"Recorded payment {payment.Id} for invoice {payment.InvoiceId}",
                _moduleId
            );

            return payment;
        }

        public async Task<Invoice> UpdateInvoiceStatusAsync(Guid id, InvoiceStatus status)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return null;

            invoice.Status = status;
            invoice.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLogService.RecordActionAsync(
                "UpdateStatus",
                id,
                null,
                $"Updated invoice {id} status to {status}",
                _moduleId
            );

            return invoice;
        }

        public async Task<Payment> UpdatePaymentStatusAsync(Guid id, PaymentStatus status)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return null;

            payment.Status = status;
            payment.ProcessedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLogService.RecordActionAsync(
                "UpdateStatus",
                id,
                null,
                $"Updated payment {id} status to {status}",
                _moduleId
            );

            return payment;
        }
    }
}
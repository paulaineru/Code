// TenantManagementService/Services/BillingService.cs
using SharedKernel.Models;
using SharedKernel.Services;
using SharedKernel.Dto;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TenantManagementService.Repository;

public class BillingService //: IBillingService
{/*
    private readonly IBillingRepository _billingRepository;
    private readonly IBookingService _bookingService;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;
    private readonly TenantDbContext _context; // Inject DbContext for transactions

    public BillingService(
        IBillingRepository billingRepository,
        IBookingService bookingService,
        INotificationService notificationService,
        IAuditLogService auditLogService,
        TenantDbContext context) // Add DbContext here
    {
        _billingRepository = billingRepository;
        _bookingService = bookingService;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
        _context = context; // Assign DbContext
    }

    public async Task<Invoice> GenerateInvoiceAsync(Guid bookingId, GenerateInvoiceDto dto)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);

            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
            }

            // Begin a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create the invoice
                var invoice = new Invoice
                {
                    BookingId = bookingId,
                    Amount = dto.Amount,
                    DueDate = dto.DueDate,
                    Status = InvoiceStatus.Pending
                };

                await _billingRepository.AddAsync(invoice);

                // Update booking status to "Billed"
                booking.Status = BookingStatus.Billed;
                await _bookingService.UpdateBookingAsync(booking);

                // Commit the transaction
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Rollback the transaction on failure
                await transaction.RollbackAsync();
                throw;
            }

            // Notify tenant
            var tenantEmail = await _userService.GetUserEmailByIdAsync(booking.TenantId);
            await _notificationService.SendCriticalActionNotificationAsync(
                adminEmail: null,
                tenantEmail: tenantEmail,
                subject: "New Invoice Generated",
                body: $"Your invoice for booking ID {bookingId} is ready. Amount due: ${dto.Amount}."
            );

            // Record audit log
            await _auditLogService.RecordActionAsync(
                action: "InvoiceGenerated",
                bookingId: bookingId.ToString(),
                description: $"Invoice generated for booking ID {bookingId}: Amount=${dto.Amount}, DueDate={dto.DueDate.ToShortDateString()}",
                userId: tenantEmail
            );

            return invoice;
        }
        catch (KeyNotFoundException ex)
        {
            throw; // Re-throw KeyNotFoundExceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice");
            throw; // Re-throw other exceptions after logging
        }
    }

    public async Task<Invoice> GetInvoiceByIdAsync(Guid id)
    {
        return await _billingRepository.GetByIdAsync(id);
    }
    public async Task<Invoice> GenerateInvoiceAsync(Guid bookingId, Guid tenantId, GenerateInvoiceDto dto)
    {
        // Your logic here (e.g., create an invoice and save to database)
        var invoice = new Invoice
        {
            BookingId = bookingId,
            TenantId = tenantId,
            Amount = dto.Amount,
            DueDate = dto.DueDate
        };
        await _context.Invoices.AddAsync(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<Payment> MakePaymentAsync(Guid invoiceId, MakePaymentDto dto)
    {
        // Your logic here (e.g., process a payment for an invoice)
        var payment = new Payment
        {
            InvoiceId = invoiceId,
            AmountPaid = dto.AmountPaid
        };
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<List<Payment>> GetPaymentsByInvoiceIdAsync(Guid invoiceId)
    {
        // Your logic here (e.g., retrieve payments for an invoice)
        return await _context.Payments
            .Where(p => p.InvoiceId == invoiceId)
            .ToListAsync();
    }

    public async Task CancelInvoiceAsync(Guid invoiceId)
    {
        // Your logic here (e.g., mark an invoice as cancelled)
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice != null)
        {
            invoice.Status = "Cancelled";
            await _context.SaveChangesAsync();
        }
    }


*/}
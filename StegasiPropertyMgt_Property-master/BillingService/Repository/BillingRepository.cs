using SharedKernel.Models;
using SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillingService.Repository
{
    public class BillingRepository : IBillingRepository
    {
        private readonly BillingDbContext _context;

        public BillingRepository(BillingDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Invoice> AddAsync(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var entry = await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }

        public Task AddInvoiceAsync(Invoice invoice)
        {
            return ExecuteInTransactionAsync(async () =>
            {
                await _context.Invoices.AddAsync(invoice);
                await _context.SaveChangesAsync();
            });
        }

        public Task AddPaymentAsync(Payment payment)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            return ExecuteInTransactionAsync(async () =>
            {
                await _context.Payments.AddAsync(payment);
                await _context.SaveChangesAsync();
            });
        }

        private async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await operation();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Database operation failed.", ex);
            }
        }

        private async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await operation();
                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Database operation failed.", ex);
            }
        }

        public async Task<Invoice> GetInvoiceByIdAsync(Guid id, bool includePayments = false)
        {
            if (id == Guid.Empty) throw new ArgumentException("Invoice ID cannot be empty.", nameof(id));

            var query = _context.Invoices.AsQueryable();
            if (includePayments)
            {
                query = query.Include(i => i.Payments);
            }

            return await query.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Invoice>> GetInvoicesByTenantAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

            return await _context.Invoices
                .Where(i => i.TenantId == tenantId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }
        public async Task<List<Invoice>> GetInvoicesByTenantIdAsync(Guid tenantId)
        {
            return await GetInvoicesByTenantAsync(tenantId);
        }

        public async Task<List<Payment>> GetPaymentsByInvoiceIdAsync(Guid invoiceId)
        {
            if (invoiceId == Guid.Empty) throw new ArgumentException("Invoice ID cannot be empty.", nameof(invoiceId));

            return await _context.Payments
                .Where(p => p.InvoiceId == invoiceId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<(Payment payment, Invoice invoice)> ProcessPaymentAsync(Guid invoiceId, decimal amount)
        {
            if (invoiceId == Guid.Empty) throw new ArgumentException("Invoice ID cannot be empty.", nameof(invoiceId));
            if (amount <= 0) throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Payments)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId)
                    ?? throw new KeyNotFoundException("Invoice not found.");

                if (invoice.Status == InvoiceStatus.Cancelled)
                {
                    throw new InvalidOperationException("Cannot make payment on a cancelled invoice.");
                }

                var payment = new Payment
                {
                    InvoiceId = invoiceId,
                    AmountPaid = amount,
                    Status = PaymentStatus.Successful,
                    ProcessedAt = DateTime.UtcNow
                };

                await _context.Payments.AddAsync(payment);

                decimal totalPaid = invoice.Payments.Sum(p => p.AmountPaid) + amount;
                invoice.Status = (invoice.Amount - totalPaid) <= 0
                    ? InvoiceStatus.Paid
                    : InvoiceStatus.PartiallyPaid;
                invoice.LastUpdated = DateTime.UtcNow;

                _context.Invoices.Update(invoice);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (payment, invoice);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Invoice was modified by another process. Please retry.", ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to process payment.", ex);
            }
        }

        public async Task UpdateInvoiceAsync(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            try
            {
                _context.Invoices.Update(invoice);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync();
                if (databaseValues == null)
                {
                    throw new InvalidOperationException("Invoice no longer exists in the database.", ex);
                }
                throw new InvalidOperationException("Invoice was modified by another process. Please retry.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to update invoice in the database.", ex);
            }
        }
    }
}
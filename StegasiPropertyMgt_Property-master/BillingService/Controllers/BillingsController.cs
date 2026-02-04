using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedKernel.Models;
using SharedKernel.Dto;
using SharedKernel.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;
        private readonly ILogger<BillingController> _logger;

        public BillingController(
            IBillingService billingService,
            ILogger<BillingController> logger)
        {
            _billingService = billingService ?? throw new ArgumentNullException(nameof(billingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("{bookingId}/generate-invoice")]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        public async Task<IActionResult> GenerateInvoice(Guid bookingId, [FromBody] GenerateInvoiceDto dto)
        {
            if (!ModelState.IsValid || bookingId == Guid.Empty || dto == null)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid input data."));
            }

            try
            {
                var tenantId = Guid.Parse(dto.TenantId); // Assumes TenantId is a string in DTO
                var invoice = await _billingService.GenerateInvoiceAsync(bookingId, tenantId, dto);
                return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, new ApiResponse<Invoice>(true, "Invoice generated successfully.", invoice));
            }
            catch (FormatException)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid Tenant ID format."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>(false, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice for booking {BookingId}", bookingId);
                return StatusCode(500, new ApiResponse<object>(false, "An error occurred while generating the invoice."));
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetInvoice(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<object>(false, "Invoice ID cannot be empty."));
            }

            try
            {
                var invoice = await _billingService.GetInvoiceByIdAsync(id);
                return Ok(new ApiResponse<Invoice>(true, "Invoice retrieved successfully.", invoice));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoice {InvoiceId}", id);
                return StatusCode(500, new ApiResponse<object>(false, "An error occurred while fetching the invoice."));
            }
        }

        [HttpPost("{invoiceId}/make-payment")]
        [Authorize(Roles = "Tenant,Property Manager")]
        public async Task<IActionResult> MakePayment(Guid invoiceId, [FromBody] MakePaymentDto dto)
        {
            if (!ModelState.IsValid || invoiceId == Guid.Empty || dto == null)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid input data."));
            }

            try
            {
                // Optional: Add check to ensure tenant is paying their own invoice
                var payment = await _billingService.MakePaymentAsync(invoiceId, dto);
                return CreatedAtAction(nameof(GetPayments), new { invoiceId = payment.InvoiceId }, new ApiResponse<Payment>(true, "Payment processed successfully.", payment));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>(false, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(false, ex.Message));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict while processing payment for invoice {InvoiceId}", invoiceId);
                return Conflict(new ApiResponse<object>(false, "Invoice was modified by another process. Please retry."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for invoice {InvoiceId}", invoiceId);
                return StatusCode(500, new ApiResponse<object>(false, "An error occurred while processing the payment."));
            }
        }

        [HttpGet("{invoiceId}/payments")]
        [Authorize]
        public async Task<IActionResult> GetPayments(Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
            {
                return BadRequest(new ApiResponse<object>(false, "Invoice ID cannot be empty."));
            }

            try
            {
                var payments = await _billingService.GetPaymentsByInvoiceIdAsync(invoiceId);
                return Ok(new ApiResponse<List<Payment>>(true, "Payments retrieved successfully.", payments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payments for invoice {InvoiceId}", invoiceId);
                return StatusCode(500, new ApiResponse<object>(false, "An error occurred while fetching payments."));
            }
        }

        [HttpDelete("{invoiceId}/cancel-invoice")]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        public async Task<IActionResult> CancelInvoice(Guid invoiceId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid input data."));
            }

            try
            {
                await _billingService.CancelInvoiceAsync(invoiceId);
                return Ok(new ApiResponse<object>(true, "Invoice cancelled successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling invoice");
                return StatusCode(500, new ApiResponse<object>(false, "An error occurred while cancelling the invoice."));
            }
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }

        public ApiResponse(bool success, string message, T? data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}
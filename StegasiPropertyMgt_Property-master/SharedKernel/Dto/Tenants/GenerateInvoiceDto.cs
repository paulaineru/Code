// SharedKernel/Models/Billing/GenerateInvoiceDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Dto
{
    public class GenerateInvoiceDto
    {
        public Guid? BookingId { get; set; }
        public string? PropertyId { get; set; }
        [Required]
        public string? TenantId { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
    }
}
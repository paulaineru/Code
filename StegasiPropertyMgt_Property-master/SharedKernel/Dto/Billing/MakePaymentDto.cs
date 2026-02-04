// SharedKernel/Models/Billing/MakePaymentDto.cs
using System;

namespace SharedKernel.Dto
{
    public class MakePaymentDto
    {
        public decimal AmountPaid { get; set; }
        public string? PaymentMethod { get; set; } // e.g., "Credit Card", "Bank Transfer"
    }
}
// SharedKernel/Models/Billing/PaymentDto.cs
using System;

namespace SharedKernel.Dto
{
    public class PaymentDto
    {
        public Guid BillId { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string PaymentMethod { get; set; } // e.g., "Bank Transfer", "Credit Card"
    }
    public enum PaymentMethod
    {
        CreditCard,
        BankTransfer,
        PayPal,
        Cash
    }
}
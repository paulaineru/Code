namespace SharedKernel.Dto
{
    public class ProcessPaymentRequest
    {
        public Guid InvoiceId { get; set; } // ID of the invoice to pay
        public decimal Amount { get; set; } // Payment amount
        public PaymentMethod PaymentMethod { get; set; } // e.g., CreditCard, BankTransfer
        public string TransactionReference { get; set; } // Optional: Payment gateway reference
        public DateTime PaymentDate { get; set; } // Date of payment
    }
}
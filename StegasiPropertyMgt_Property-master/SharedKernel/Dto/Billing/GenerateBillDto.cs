// SharedKernel/Models/Billing/GenerateBillDto.cs
using System;

namespace SharedKernel.Dto
{
    public class GenerateBillDto
    {
        public Guid TenantId { get; set; }
        public Guid PropertyId { get; set; }
        public DateTime BillDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public decimal AmountDue { get; set; }
        public string Description { get; set; }
    }
}
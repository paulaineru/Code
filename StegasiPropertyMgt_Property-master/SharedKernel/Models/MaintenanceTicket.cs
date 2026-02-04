using System;

namespace SharedKernel.Models
{
    public class MaintenanceTicket
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string Description { get; set; }
        public decimal RepairCost { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
} 
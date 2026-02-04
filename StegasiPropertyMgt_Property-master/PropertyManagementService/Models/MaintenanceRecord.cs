using System;

namespace PropertyManagementService.Models
{
    public class MaintenanceRecord
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string Description { get; set; }
        public DateTime DateReported { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public decimal? RepairCost { get; set; }
        public DateTime? DateResolved { get; set; }
    }
} 
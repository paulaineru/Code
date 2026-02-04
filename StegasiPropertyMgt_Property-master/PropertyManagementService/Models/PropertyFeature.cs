using System;

namespace PropertyManagementService.Models
{
    public class PropertyFeature
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 
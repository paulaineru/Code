using System;

namespace PropertyManagementService.Models
{
    public class PropertyImage
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
} 
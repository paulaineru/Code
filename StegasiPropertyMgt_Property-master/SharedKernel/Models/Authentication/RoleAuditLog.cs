using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedKernel.Models
{
    public class RoleAuditLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, ASSIGN, REMOVE

        [Required]
        public string PerformedBy { get; set; } = string.Empty; // User ID who performed the action

        [Required]
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "jsonb")]
        public string? OldValues { get; set; }

        [Column(TypeName = "jsonb")]
        public string? NewValues { get; set; }

        public Guid? AffectedUserId { get; set; } // For role assignment/removal actions

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation property
        public virtual Role Role { get; set; } = null!;
    }
} 
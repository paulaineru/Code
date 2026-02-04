using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedKernel.Models
{
    public class RoleHierarchy
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ParentRoleId { get; set; }

        [Required]
        public Guid ChildRoleId { get; set; }

        public int HierarchyLevel { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("ParentRoleId")]
        public virtual Role ParentRole { get; set; } = null!;

        [ForeignKey("ChildRoleId")]
        public virtual Role ChildRole { get; set; } = null!;
    }
} 
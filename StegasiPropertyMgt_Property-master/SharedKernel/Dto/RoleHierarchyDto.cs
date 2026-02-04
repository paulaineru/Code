using System;
using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Dto
{
    public class RoleHierarchyDto
    {
        [Required]
        public Guid ParentRoleId { get; set; }
        [Required]
        public Guid ChildRoleId { get; set; }
        public int HierarchyLevel { get; set; } = 1;
    }
} 
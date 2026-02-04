using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Dto
{
    public class CreateRoleDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        public List<Guid> PermissionIds { get; set; } = new List<Guid>();
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class UpdateRoleDto
    {
        [StringLength(50)]
        public string? Name { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        public List<Guid>? PermissionIds { get; set; }
        public List<string>? Permissions { get; set; }
    }

    public class RoleResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<PermissionResponseDto> Permissions { get; set; } = new List<PermissionResponseDto>();
    }

    public class PermissionResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class AssignRoleDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RoleId { get; set; }
    }

    public class UserRoleResponseDto
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public string? AssignedBy { get; set; }
    }

    public class AdminUserResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public string? AssignedBy { get; set; }
    }
} 
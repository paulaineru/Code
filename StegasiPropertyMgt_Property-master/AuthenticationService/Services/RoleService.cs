using SharedKernel.Dto;
using SharedKernel.Models;
using AuthenticationService.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;
using System.Text.Json;
using AuthenticationService.Configuration;

namespace AuthenticationService.Services
{
    public interface IRoleService
    {
        Task<RoleResponseDto> CreateRoleAsync(CreateRoleDto dto, string performedBy);
        Task<RoleResponseDto> UpdateRoleAsync(Guid roleId, UpdateRoleDto dto, string performedBy);
        Task DeleteRoleAsync(Guid roleId, string performedBy);
        Task<RoleResponseDto> GetRoleAsync(Guid roleId);
        Task<List<RoleResponseDto>> GetAllRolesAsync();
        Task<List<PermissionResponseDto>> GetAllPermissionsAsync();
        Task<UserRoleResponseDto> AssignRoleToUserAsync(AssignRoleDto dto, string performedBy);
        Task RemoveRoleFromUserAsync(Guid userId, Guid roleId, string performedBy);
        Task<List<UserRoleResponseDto>> GetUserRolesAsync(Guid userId);
        Task<bool> AdminExistsAsync();
        Task<List<AdminUserResponseDto>> GetAllAdminUsersAsync();
    }

    public class RoleService : IRoleService
    {
        private readonly IAuthDbContext _context;
        private readonly ILogger<RoleService> _logger;
        private readonly IRoleAuditService _auditService;
        private readonly RoleConfiguration _roleConfig;

        public RoleService(
            IAuthDbContext context, 
            ILogger<RoleService> logger, 
            IRoleAuditService auditService,
            RoleConfiguration roleConfig)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
            _roleConfig = roleConfig;
        }

        public async Task<RoleResponseDto> CreateRoleAsync(CreateRoleDto dto, string performedBy)
        {
            if (await _context.Roles.AnyAsync(r => r.Name == dto.Name))
            {
                throw new InvalidOperationException("Role name already exists");
            }

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            // Add permissions
            if (dto.PermissionIds != null && dto.PermissionIds.Any())
            {
                var permissions = await _context.Permissions
                    .Where(p => dto.PermissionIds.Contains(p.Id))
                    .ToListAsync();

                foreach (var permission in permissions)
                {
                    role.Permissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            await _auditService.LogRoleActionAsync(
                role.Id,
                "CREATE",
                performedBy,
                null,
                new { role.Name, role.Description, PermissionIds = dto.PermissionIds }
            );

            _logger.LogInformation("Created new role: {RoleName}", role.Name);
            return MapToRoleResponseDto(role);
        }

        public async Task<RoleResponseDto> UpdateRoleAsync(Guid roleId, UpdateRoleDto dto, string performedBy)
        {
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
            {
                throw new NotFoundException($"Role with ID {roleId} not found");
            }

            if (role.IsSystemRole)
            {
                throw new InvalidOperationException("Cannot modify system roles");
            }

            var oldValues = new
            {
                role.Name,
                role.Description,
                PermissionIds = role.Permissions.Select(rp => rp.PermissionId).ToList()
            };

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                role.Name = dto.Name;
            }

            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                role.Description = dto.Description;
            }

            if (dto.PermissionIds != null)
            {
                var currentPermissionIds = role.Permissions.Select(rp => rp.PermissionId).ToList();
                var permissionsToAdd = dto.PermissionIds.Except(currentPermissionIds);
                var permissionsToRemove = currentPermissionIds.Except(dto.PermissionIds);

                foreach (var permissionId in permissionsToAdd)
                {
                    var permission = await _context.Permissions.FindAsync(permissionId);
                    if (permission != null)
                    {
                        role.Permissions.Add(new RolePermission { Permission = permission });
                    }
                }

                foreach (var permissionId in permissionsToRemove)
                {
                    var rolePermission = role.Permissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
                    if (rolePermission != null)
                    {
                        role.Permissions.Remove(rolePermission);
                    }
                }
            }

            role.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogRoleActionAsync(
                roleId,
                "UPDATE",
                performedBy,
                oldValues,
                new
                {
                    role.Name,
                    role.Description,
                    PermissionIds = role.Permissions.Select(rp => rp.PermissionId).ToList()
                }
            );

            _logger.LogInformation("Updated role: {RoleName}", role.Name);
            return MapToRoleResponseDto(role);
        }

        public async Task DeleteRoleAsync(Guid roleId, string performedBy)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                throw new NotFoundException($"Role with ID {roleId} not found");
            }

            if (role.IsSystemRole)
            {
                throw new InvalidOperationException("Cannot delete system roles");
            }

            await _auditService.LogRoleActionAsync(
                roleId,
                "DELETE",
                performedBy,
                new { role.Name, role.Description }
            );

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted role: {RoleName}", role.Name);
        }

        private async Task<List<Permission>> GetAllPermissionsForRoleAsync(Guid roleId)
        {
            var visited = new HashSet<Guid>();
            var permissions = new List<Permission>();
            await CollectPermissionsRecursive(roleId, visited, permissions);
            return permissions.DistinctBy(p => p.Id).ToList();
        }

        private async Task CollectPermissionsRecursive(Guid roleId, HashSet<Guid> visited, List<Permission> permissions)
        {
            if (visited.Contains(roleId)) return;
            visited.Add(roleId);

            var role = await _context.Roles
                .Include(r => r.Permissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId);
            if (role != null)
            {
                permissions.AddRange(role.Permissions.Select(rp => rp.Permission));
            }

            var parentIds = await _context.RoleHierarchies
                .Where(rh => rh.ChildRoleId == roleId)
                .Select(rh => rh.ParentRoleId)
                .ToListAsync();
            foreach (var parentId in parentIds)
            {
                await CollectPermissionsRecursive(parentId, visited, permissions);
            }
        }

        public async Task<RoleResponseDto> GetRoleAsync(Guid roleId)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
            {
                throw new NotFoundException($"Role with ID {roleId} not found");
            }
            var allPermissions = await GetAllPermissionsForRoleAsync(roleId);
            return new RoleResponseDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt,
                Permissions = allPermissions.Select(p => new PermissionResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category
                }).ToList()
            };
        }

        public async Task<List<RoleResponseDto>> GetAllRolesAsync()
        {
            var roles = await _context.Roles.ToListAsync();
            var result = new List<RoleResponseDto>();
            foreach (var role in roles)
            {
                var allPermissions = await GetAllPermissionsForRoleAsync(role.Id);
                result.Add(new RoleResponseDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    IsSystemRole = role.IsSystemRole,
                    CreatedAt = role.CreatedAt,
                    UpdatedAt = role.UpdatedAt,
                    Permissions = allPermissions.Select(p => new PermissionResponseDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Category = p.Category
                    }).ToList()
                });
            }
            return result;
        }

        public async Task<List<PermissionResponseDto>> GetAllPermissionsAsync()
        {
            var permissions = await _context.Permissions.ToListAsync();
            return permissions.Select(p => new PermissionResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category
            }).ToList();
        }

        public async Task<UserRoleResponseDto> AssignRoleToUserAsync(AssignRoleDto dto, string performedBy)
        {
            try
            {
                // Check if this is the first user being assigned a role
                var existingUserRoles = await _context.UserRoles.CountAsync();
                var isFirstUserRole = existingUserRoles == 0;

                // If this is the first user role assignment and it's for the admin role, allow it
                if (isFirstUserRole && dto.RoleId == _roleConfig.AdminRoleId)
                {
                    _logger.LogInformation(
                        "First admin user role assignment. UserId: {UserId}, AssignedBy: {AssignedBy}",
                        dto.UserId,
                        performedBy
                    );

                    var newUserRole = new UserRole
                    {
                        UserId = dto.UserId,
                        RoleId = dto.RoleId,
                        AssignedAt = DateTime.UtcNow,
                        AssignedBy = performedBy
                    };

                    _context.UserRoles.Add(newUserRole);
                    await _context.SaveChangesAsync();

                    var role = await _context.Roles.FindAsync(dto.RoleId);
                    return new UserRoleResponseDto
                    {
                        UserId = dto.UserId,
                        RoleId = dto.RoleId,
                        RoleName = role?.Name,
                        AssignedAt = newUserRole.AssignedAt,
                        AssignedBy = performedBy
                    };
                }

                // For subsequent role assignments, check if the user already has the role
                var existingUserRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);

                if (existingUserRole != null)
                {
                    _logger.LogWarning(
                        "Attempt to assign existing role. UserId: {UserId}, RoleId: {RoleId}, AssignedBy: {AssignedBy}",
                        dto.UserId,
                        dto.RoleId,
                        performedBy
                    );
                    throw new InvalidOperationException("User already has this role");
                }

                var userRole = new UserRole
                {
                    UserId = dto.UserId,
                    RoleId = dto.RoleId,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = performedBy
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();

                var assignedRole = await _context.Roles.FindAsync(dto.RoleId);
                
                _logger.LogInformation(
                    "Role assigned successfully. UserId: {UserId}, RoleId: {RoleId}, RoleName: {RoleName}, AssignedBy: {AssignedBy}",
                    dto.UserId,
                    dto.RoleId,
                    assignedRole?.Name,
                    performedBy
                );

                return new UserRoleResponseDto
                {
                    UserId = dto.UserId,
                    RoleId = dto.RoleId,
                    RoleName = assignedRole?.Name,
                    AssignedAt = userRole.AssignedAt,
                    AssignedBy = performedBy
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error assigning role. UserId: {UserId}, RoleId: {RoleId}, AssignedBy: {AssignedBy}",
                    dto.UserId,
                    dto.RoleId,
                    performedBy
                );
                throw;
            }
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId, string performedBy)
        {
            var userRole = await _context.UserRoles
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole == null)
            {
                throw new NotFoundException("User role assignment not found");
            }

            await _auditService.LogRoleActionAsync(
                roleId,
                "REMOVE",
                performedBy,
                affectedUserId: userId,
                notes: $"Role {userRole.Role.Name} removed from user {userId}"
            );

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed role {RoleId} from user {UserId}", roleId, userId);
        }

        public async Task<List<UserRoleResponseDto>> GetUserRolesAsync(Guid userId)
        {
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            return userRoles.Select(ur => MapToUserRoleResponseDto(ur, ur.Role)).ToList();
        }

        public async Task<bool> AdminExistsAsync()
        {
            var exists = await _context.UserRoles
                .AnyAsync(ur => ur.RoleId == _roleConfig.AdminRoleId);
            
            _logger.LogInformation(
                "Admin existence check performed. Admin exists: {AdminExists}",
                exists
            );
            
            return exists;
        }

        public async Task<List<AdminUserResponseDto>> GetAllAdminUsersAsync()
        {
            var adminUsers = await _context.UserRoles
                .Include(ur => ur.User)
                .Where(ur => ur.RoleId == _roleConfig.AdminRoleId)
                .Select(ur => new AdminUserResponseDto
                {
                    UserId = ur.UserId,
                    Email = ur.User.Email,
                    UserName = ur.User.Username,
                    FirstName = ur.User.FirstName,
                    LastName = ur.User.LastName,
                    AssignedAt = ur.AssignedAt,
                    AssignedBy = ur.AssignedBy
                })
                .ToListAsync();

            _logger.LogInformation(
                "Retrieved {Count} admin users. Admin IDs: {AdminIds}",
                adminUsers.Count,
                string.Join(", ", adminUsers.Select(a => a.UserId))
            );

            return adminUsers;
        }

        private static RoleResponseDto MapToRoleResponseDto(Role role)
        {
            return new RoleResponseDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt,
                Permissions = role.Permissions.Select(rp => new PermissionResponseDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    Description = rp.Permission.Description,
                    Category = rp.Permission.Category
                }).ToList()
            };
        }

        private static UserRoleResponseDto MapToUserRoleResponseDto(UserRole userRole, Role role)
        {
            return new UserRoleResponseDto
            {
                UserId = userRole.UserId,
                RoleId = userRole.RoleId,
                RoleName = role.Name,
                AssignedAt = userRole.AssignedAt,
                AssignedBy = userRole.AssignedBy
            };
        }
    }
} 
using SharedKernel.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Repository
{
    public interface IAuthDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<Permission> Permissions { get; set; }
        DbSet<RolePermission> RolePermissions { get; set; }
        DbSet<UserRole> UserRoles { get; set; }
        DbSet<RoleAuditLog> RoleAuditLogs { get; set; }
        DbSet<RoleHierarchy> RoleHierarchies { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
} 
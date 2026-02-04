using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharedKernel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationService.Repository
{
    public class AuthDbContext : DbContext, IAuthDbContext
    {
        private readonly IConfiguration _configuration;

        private static class SeedData
        {
            // Role IDs
            public static readonly Guid AdminRoleId = new("00000000-0000-0000-0000-000000000001");
            public static readonly Guid UserRoleId = new("00000000-0000-0000-0000-000000000002");
            public static readonly Guid EstatesOfficerRoleId = new("00000000-0000-0000-0000-000000000003");
            public static readonly Guid PropertyManagerRoleId = new("00000000-0000-0000-0000-000000000004");
            public static readonly Guid MaintenanceOfficerRoleId = new("00000000-0000-0000-0000-000000000005");
            public static readonly Guid FinanceTeamRoleId = new("00000000-0000-0000-0000-000000000006");
            public static readonly Guid SalesOfficerRoleId = new("00000000-0000-0000-0000-000000000007");
            public static readonly Guid SalesManagerRoleId = new("00000000-0000-0000-0000-000000000008");
            public static readonly Guid TenantRoleId = new("00000000-0000-0000-0000-000000000009");

            // Permission IDs - User Management
            public static readonly Guid ViewUsersPermissionId = new("00000000-0000-0000-0001-000000000001");
            public static readonly Guid CreateUsersPermissionId = new("00000000-0000-0000-0001-000000000002");
            public static readonly Guid EditUsersPermissionId = new("00000000-0000-0000-0001-000000000003");
            public static readonly Guid DeleteUsersPermissionId = new("00000000-0000-0000-0001-000000000004");

            // Permission IDs - Role Management
            public static readonly Guid ViewRolesPermissionId = new("00000000-0000-0000-0002-000000000001");
            public static readonly Guid CreateRolesPermissionId = new("00000000-0000-0000-0002-000000000002");
            public static readonly Guid EditRolesPermissionId = new("00000000-0000-0000-0002-000000000003");
            public static readonly Guid DeleteRolesPermissionId = new("00000000-0000-0000-0002-000000000004");
            public static readonly Guid AssignRolesPermissionId = new("00000000-0000-0000-0002-000000000005");

            // Permission IDs - Property Management
            public static readonly Guid ViewPropertiesPermissionId = new("00000000-0000-0000-0003-000000000001");
            public static readonly Guid CreatePropertiesPermissionId = new("00000000-0000-0000-0003-000000000002");
            public static readonly Guid EditPropertiesPermissionId = new("00000000-0000-0000-0003-000000000003");
            public static readonly Guid DeletePropertiesPermissionId = new("00000000-0000-0000-0003-000000000004");
            public static readonly Guid ApprovePropertiesPermissionId = new("00000000-0000-0000-0003-000000000005");
            public static readonly Guid ManageMaintenancePermissionId = new("00000000-0000-0000-0003-000000000006");
            public static readonly Guid ManageBudgetPermissionId = new("00000000-0000-0000-0003-000000000007");

            // Permission IDs - Tenant Management
            public static readonly Guid ViewTenantsPermissionId = new("00000000-0000-0000-0004-000000000001");
            public static readonly Guid CreateTenantsPermissionId = new("00000000-0000-0000-0004-000000000002");
            public static readonly Guid EditTenantsPermissionId = new("00000000-0000-0000-0004-000000000003");
            public static readonly Guid DeleteTenantsPermissionId = new("00000000-0000-0000-0004-000000000004");
            public static readonly Guid ManageLeasesPermissionId = new("00000000-0000-0000-0004-000000000005");
            public static readonly Guid ProcessPaymentsPermissionId = new("00000000-0000-0000-0004-000000000006");
            public static readonly Guid ViewFinancialReportsPermissionId = new("00000000-0000-0000-0004-000000000007");

            // Permission IDs - Sales Management
            public static readonly Guid ViewSalesPermissionId = new("00000000-0000-0000-0005-000000000001");
            public static readonly Guid CreateSalesPermissionId = new("00000000-0000-0000-0005-000000000002");
            public static readonly Guid EditSalesPermissionId = new("00000000-0000-0000-0005-000000000003");
            public static readonly Guid ApproveSalesPermissionId = new("00000000-0000-0000-0005-000000000004");
            public static readonly Guid ProcessDownPaymentsPermissionId = new("00000000-0000-0000-0005-000000000005");
            public static readonly Guid ScheduleToursPermissionId = new("00000000-0000-0000-0005-000000000006");

            // Permission IDs - Tenant Portal
            public static readonly Guid ViewPropertiesPortalPermissionId = new("00000000-0000-0000-0006-000000000001");
            public static readonly Guid ScheduleTourPortalPermissionId = new("00000000-0000-0000-0006-000000000002");
            public static readonly Guid ReservePropertyPortalPermissionId = new("00000000-0000-0000-0006-000000000003");
            public static readonly Guid MakeDownPaymentPortalPermissionId = new("00000000-0000-0000-0006-000000000004");
            public static readonly Guid ViewRentSchedulePortalPermissionId = new("00000000-0000-0000-0006-000000000005");
            public static readonly Guid MakePaymentPortalPermissionId = new("00000000-0000-0000-0006-000000000006");
        }

        public AuthDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RoleAuditLog> RoleAuditLogs { get; set; }
        public DbSet<RoleHierarchy> RoleHierarchies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"), 
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory", "authdb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("authdb");
            
            modelBuilder.Entity<Permission>().ToTable("Auth_Permissions");
            modelBuilder.Entity<Role>().ToTable("Auth_Roles");
            modelBuilder.Entity<User>().ToTable("Auth_Users");
            modelBuilder.Entity<RoleAuditLog>().ToTable("Auth_RoleAuditLogs");
            modelBuilder.Entity<RoleHierarchy>().ToTable("Auth_RoleHierarchies");
            modelBuilder.Entity<RolePermission>().ToTable("Auth_RolePermissions");
            modelBuilder.Entity<UserRole>().ToTable("Auth_UserRoles");
            
            base.OnModelCreating(modelBuilder);

            // Configure RolePermission many-to-many relationship
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

                entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.Permissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UserRole many-to-many relationship
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.User)
                    .WithMany()
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure RoleHierarchy relationships
            modelBuilder.Entity<RoleHierarchy>(entity =>
            {
                entity.HasOne(rh => rh.ParentRole)
                    .WithMany(r => r.ChildRoles)
                    .HasForeignKey(rh => rh.ParentRoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(rh => rh.ChildRole)
                    .WithMany(r => r.ParentRoles)
                    .HasForeignKey(rh => rh.ChildRoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure RoleAuditLog relationship
            modelBuilder.Entity<RoleAuditLog>(entity =>
            {
                entity.HasOne(ral => ral.Role)
                    .WithMany()
                    .HasForeignKey(ral => ral.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data for permissions
            var permissions = new List<Permission>
            {
                // User management permissions
                new Permission { Id = SeedData.ViewUsersPermissionId, Name = "users.view", Description = "View users", Category = "User Management" },
                new Permission { Id = SeedData.CreateUsersPermissionId, Name = "users.create", Description = "Create users", Category = "User Management" },
                new Permission { Id = SeedData.EditUsersPermissionId, Name = "users.edit", Description = "Edit users", Category = "User Management" },
                new Permission { Id = SeedData.DeleteUsersPermissionId, Name = "users.delete", Description = "Delete users", Category = "User Management" },

                // Role management permissions
                new Permission { Id = SeedData.ViewRolesPermissionId, Name = "roles.view", Description = "View roles", Category = "Role Management" },
                new Permission { Id = SeedData.CreateRolesPermissionId, Name = "roles.create", Description = "Create roles", Category = "Role Management" },
                new Permission { Id = SeedData.EditRolesPermissionId, Name = "roles.edit", Description = "Edit roles", Category = "Role Management" },
                new Permission { Id = SeedData.DeleteRolesPermissionId, Name = "roles.delete", Description = "Delete roles", Category = "Role Management" },
                new Permission { Id = SeedData.AssignRolesPermissionId, Name = "roles.assign", Description = "Assign roles to users", Category = "Role Management" },

                // Property management permissions
                new Permission { Id = SeedData.ViewPropertiesPermissionId, Name = "properties.view", Description = "View properties", Category = "Property Management" },
                new Permission { Id = SeedData.CreatePropertiesPermissionId, Name = "properties.create", Description = "Create properties", Category = "Property Management" },
                new Permission { Id = SeedData.EditPropertiesPermissionId, Name = "properties.edit", Description = "Edit properties", Category = "Property Management" },
                new Permission { Id = SeedData.DeletePropertiesPermissionId, Name = "properties.delete", Description = "Delete properties", Category = "Property Management" },
                new Permission { Id = SeedData.ApprovePropertiesPermissionId, Name = "properties.approve", Description = "Approve properties", Category = "Property Management" },
                new Permission { Id = SeedData.ManageMaintenancePermissionId, Name = "properties.maintenance", Description = "Manage property maintenance", Category = "Property Management" },
                new Permission { Id = SeedData.ManageBudgetPermissionId, Name = "properties.budget", Description = "Manage property budget", Category = "Property Management" },

                // Tenant management permissions
                new Permission { Id = SeedData.ViewTenantsPermissionId, Name = "tenants.view", Description = "View tenants", Category = "Tenant Management" },
                new Permission { Id = SeedData.CreateTenantsPermissionId, Name = "tenants.create", Description = "Create tenants", Category = "Tenant Management" },
                new Permission { Id = SeedData.EditTenantsPermissionId, Name = "tenants.edit", Description = "Edit tenants", Category = "Tenant Management" },
                new Permission { Id = SeedData.DeleteTenantsPermissionId, Name = "tenants.delete", Description = "Delete tenants", Category = "Tenant Management" },
                new Permission { Id = SeedData.ManageLeasesPermissionId, Name = "tenants.leases", Description = "Manage tenant leases", Category = "Tenant Management" },
                new Permission { Id = SeedData.ProcessPaymentsPermissionId, Name = "tenants.payments", Description = "Process tenant payments", Category = "Tenant Management" },
                new Permission { Id = SeedData.ViewFinancialReportsPermissionId, Name = "tenants.reports", Description = "View financial reports", Category = "Tenant Management" },

                // Sales management permissions
                new Permission { Id = SeedData.ViewSalesPermissionId, Name = "sales.view", Description = "View sales", Category = "Sales Management" },
                new Permission { Id = SeedData.CreateSalesPermissionId, Name = "sales.create", Description = "Create sales", Category = "Sales Management" },
                new Permission { Id = SeedData.EditSalesPermissionId, Name = "sales.edit", Description = "Edit sales", Category = "Sales Management" },
                new Permission { Id = SeedData.ApproveSalesPermissionId, Name = "sales.approve", Description = "Approve sales", Category = "Sales Management" },
                new Permission { Id = SeedData.ProcessDownPaymentsPermissionId, Name = "sales.downpayments", Description = "Process down payments", Category = "Sales Management" },
                new Permission { Id = SeedData.ScheduleToursPermissionId, Name = "sales.tours", Description = "Schedule property tours", Category = "Sales Management" },

                // Tenant portal permissions
                new Permission { Id = SeedData.ViewPropertiesPortalPermissionId, Name = "portal.properties.view", Description = "View properties in portal", Category = "Tenant Portal" },
                new Permission { Id = SeedData.ScheduleTourPortalPermissionId, Name = "portal.tours.schedule", Description = "Schedule property tours", Category = "Tenant Portal" },
                new Permission { Id = SeedData.ReservePropertyPortalPermissionId, Name = "portal.properties.reserve", Description = "Reserve properties", Category = "Tenant Portal" },
                new Permission { Id = SeedData.MakeDownPaymentPortalPermissionId, Name = "portal.payments.down", Description = "Make down payments", Category = "Tenant Portal" },
                new Permission { Id = SeedData.ViewRentSchedulePortalPermissionId, Name = "portal.rent.schedule", Description = "View rent schedule", Category = "Tenant Portal" },
                new Permission { Id = SeedData.MakePaymentPortalPermissionId, Name = "portal.payments.make", Description = "Make payments", Category = "Tenant Portal" }
            };

            modelBuilder.Entity<Permission>().HasData(permissions);

            // Seed data for roles
            var roles = new List<Role>
            {
                new Role
                {
                    Id = SeedData.AdminRoleId,
                    Name = "Admin",
                    Description = "System administrator with full access",
                    IsSystemRole = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Role
                {
                    Id = SeedData.UserRoleId,
                    Name = "User",
                    Description = "Regular user with basic access",
                    IsSystemRole = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Role
                {
                    Id = SeedData.EstatesOfficerRoleId,
                    Name = "Estates Officer",
                    Description = "Manages property registrations, tenant onboarding, and lease administration",
                    IsSystemRole = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Role
                {
                    Id = SeedData.PropertyManagerRoleId,
                    Name = "Property Manager",
                    Description = "Oversees property approvals, maintenance, and budget management",
                    IsSystemRole = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Role
                {
                    Id = SeedData.MaintenanceOfficerRoleId,
                    Name = "Maintenance Officer",
                    Description = "Addresses tenant maintenance requests and manages work orders",
                    IsSystemRole = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Role
                {
                    Id = SeedData.FinanceTeamRoleId,
                    Name = "Finance Team",
                    Description = "Processes rent payments, issues invoices, and handles financial reporting",
                    IsSystemRole = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Role
                {
                    Id = SeedData.SalesOfficerRoleId,
                    Name = "Sales Officer",
                    Description = "Handles property tours, bookings, and client onboarding",
                    IsSystemRole = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Role
                {
                    Id = SeedData.SalesManagerRoleId,
                    Name = "Sales Manager",
                    Description = "Manages sales team and property down-payment processing",
                    IsSystemRole = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Role
                {
                    Id = SeedData.TenantRoleId,
                    Name = "Tenant",
                    Description = "Accesses tenant portal for property viewing, payments, and maintenance requests",
                    IsSystemRole = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            };

            modelBuilder.Entity<Role>().HasData(roles);

            // Seed role permissions for Admin role (all permissions)
            var adminRolePermissions = permissions.Select(p => new RolePermission
            {
                RoleId = SeedData.AdminRoleId,
                PermissionId = p.Id
            });

            // Seed role permissions for Estates Officer
            var estatesOfficerPermissions = new List<Guid>
            {
                SeedData.ViewPropertiesPermissionId,
                SeedData.CreatePropertiesPermissionId,
                SeedData.EditPropertiesPermissionId,
                SeedData.ViewTenantsPermissionId,
                SeedData.CreateTenantsPermissionId,
                SeedData.EditTenantsPermissionId,
                SeedData.ManageLeasesPermissionId
            };

            // Seed role permissions for Property Manager
            var propertyManagerPermissions = new List<Guid>
            {
                SeedData.ViewPropertiesPermissionId,
                SeedData.CreatePropertiesPermissionId,
                SeedData.EditPropertiesPermissionId,
                SeedData.ApprovePropertiesPermissionId,
                SeedData.ManageMaintenancePermissionId,
                SeedData.ManageBudgetPermissionId,
                SeedData.ViewTenantsPermissionId,
                SeedData.CreateTenantsPermissionId,
                SeedData.EditTenantsPermissionId
            };

            // Seed role permissions for Maintenance Officer
            var maintenanceOfficerPermissions = new List<Guid>
            {
                SeedData.ViewPropertiesPermissionId,
                SeedData.ManageMaintenancePermissionId,
                SeedData.ViewTenantsPermissionId
            };

            // Seed role permissions for Finance Team
            var financeTeamPermissions = new List<Guid>
            {
                SeedData.ViewPropertiesPermissionId,
                SeedData.ViewTenantsPermissionId,
                SeedData.ProcessPaymentsPermissionId,
                SeedData.ViewFinancialReportsPermissionId
            };

            // Seed role permissions for Sales Officer
            var salesOfficerPermissions = new List<Guid>
            {
                SeedData.ViewPropertiesPermissionId,
                SeedData.ViewSalesPermissionId,
                SeedData.CreateSalesPermissionId,
                SeedData.ScheduleToursPermissionId
            };

            // Seed role permissions for Sales Manager
            var salesManagerPermissions = new List<Guid>
            {
                SeedData.ViewPropertiesPermissionId,
                SeedData.ViewSalesPermissionId,
                SeedData.CreateSalesPermissionId,
                SeedData.EditSalesPermissionId,
                SeedData.ApproveSalesPermissionId,
                SeedData.ProcessDownPaymentsPermissionId,
                SeedData.ScheduleToursPermissionId
            };

            // Seed role permissions for Tenant
            var tenantPermissions = new List<Guid>
            {
                SeedData.ViewPropertiesPortalPermissionId,
                SeedData.ScheduleTourPortalPermissionId,
                SeedData.ReservePropertyPortalPermissionId,
                SeedData.MakeDownPaymentPortalPermissionId,
                SeedData.ViewRentSchedulePortalPermissionId,
                SeedData.MakePaymentPortalPermissionId
            };

            // Combine all role permissions
            var allRolePermissions = new List<RolePermission>();
            allRolePermissions.AddRange(adminRolePermissions);
            allRolePermissions.AddRange(estatesOfficerPermissions.Select(p => new RolePermission { RoleId = SeedData.EstatesOfficerRoleId, PermissionId = p }));
            allRolePermissions.AddRange(propertyManagerPermissions.Select(p => new RolePermission { RoleId = SeedData.PropertyManagerRoleId, PermissionId = p }));
            allRolePermissions.AddRange(maintenanceOfficerPermissions.Select(p => new RolePermission { RoleId = SeedData.MaintenanceOfficerRoleId, PermissionId = p }));
            allRolePermissions.AddRange(financeTeamPermissions.Select(p => new RolePermission { RoleId = SeedData.FinanceTeamRoleId, PermissionId = p }));
            allRolePermissions.AddRange(salesOfficerPermissions.Select(p => new RolePermission { RoleId = SeedData.SalesOfficerRoleId, PermissionId = p }));
            allRolePermissions.AddRange(salesManagerPermissions.Select(p => new RolePermission { RoleId = SeedData.SalesManagerRoleId, PermissionId = p }));
            allRolePermissions.AddRange(tenantPermissions.Select(p => new RolePermission { RoleId = SeedData.TenantRoleId, PermissionId = p }));

            modelBuilder.Entity<RolePermission>().HasData(allRolePermissions);
        }
    }
}
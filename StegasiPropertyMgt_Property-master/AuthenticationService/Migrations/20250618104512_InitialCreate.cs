using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthenticationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "authdb");

            migrationBuilder.CreateTable(
                name: "Auth_Permissions",
                schema: "authdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auth_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Auth_Roles",
                schema: "authdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auth_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Auth_Users",
                schema: "authdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    District = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auth_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Auth_RoleAuditLogs",
                schema: "authdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PerformedBy = table.Column<string>(type: "text", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OldValues = table.Column<string>(type: "jsonb", nullable: true),
                    NewValues = table.Column<string>(type: "jsonb", nullable: true),
                    AffectedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auth_RoleAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Auth_RoleAuditLogs_Auth_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "authdb",
                        principalTable: "Auth_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Auth_RoleHierarchies",
                schema: "authdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    HierarchyLevel = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auth_RoleHierarchies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Auth_RoleHierarchies_Auth_Roles_ChildRoleId",
                        column: x => x.ChildRoleId,
                        principalSchema: "authdb",
                        principalTable: "Auth_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Auth_RoleHierarchies_Auth_Roles_ParentRoleId",
                        column: x => x.ParentRoleId,
                        principalSchema: "authdb",
                        principalTable: "Auth_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Auth_RolePermissions",
                schema: "authdb",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auth_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_Auth_RolePermissions_Auth_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "authdb",
                        principalTable: "Auth_Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Auth_RolePermissions_Auth_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "authdb",
                        principalTable: "Auth_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Auth_UserRoles",
                schema: "authdb",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auth_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_Auth_UserRoles_Auth_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "authdb",
                        principalTable: "Auth_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Auth_UserRoles_Auth_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "authdb",
                        principalTable: "Auth_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "authdb",
                table: "Auth_Permissions",
                columns: new[] { "Id", "Category", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000001"), "User Management", "View users", "users.view" },
                    { new Guid("00000000-0000-0000-0001-000000000002"), "User Management", "Create users", "users.create" },
                    { new Guid("00000000-0000-0000-0001-000000000003"), "User Management", "Edit users", "users.edit" },
                    { new Guid("00000000-0000-0000-0001-000000000004"), "User Management", "Delete users", "users.delete" },
                    { new Guid("00000000-0000-0000-0002-000000000001"), "Role Management", "View roles", "roles.view" },
                    { new Guid("00000000-0000-0000-0002-000000000002"), "Role Management", "Create roles", "roles.create" },
                    { new Guid("00000000-0000-0000-0002-000000000003"), "Role Management", "Edit roles", "roles.edit" },
                    { new Guid("00000000-0000-0000-0002-000000000004"), "Role Management", "Delete roles", "roles.delete" },
                    { new Guid("00000000-0000-0000-0002-000000000005"), "Role Management", "Assign roles to users", "roles.assign" },
                    { new Guid("00000000-0000-0000-0003-000000000001"), "Property Management", "View properties", "properties.view" },
                    { new Guid("00000000-0000-0000-0003-000000000002"), "Property Management", "Create properties", "properties.create" },
                    { new Guid("00000000-0000-0000-0003-000000000003"), "Property Management", "Edit properties", "properties.edit" },
                    { new Guid("00000000-0000-0000-0003-000000000004"), "Property Management", "Delete properties", "properties.delete" },
                    { new Guid("00000000-0000-0000-0003-000000000005"), "Property Management", "Approve properties", "properties.approve" },
                    { new Guid("00000000-0000-0000-0003-000000000006"), "Property Management", "Manage property maintenance", "properties.maintenance" },
                    { new Guid("00000000-0000-0000-0003-000000000007"), "Property Management", "Manage property budget", "properties.budget" },
                    { new Guid("00000000-0000-0000-0004-000000000001"), "Tenant Management", "View tenants", "tenants.view" },
                    { new Guid("00000000-0000-0000-0004-000000000002"), "Tenant Management", "Create tenants", "tenants.create" },
                    { new Guid("00000000-0000-0000-0004-000000000003"), "Tenant Management", "Edit tenants", "tenants.edit" },
                    { new Guid("00000000-0000-0000-0004-000000000004"), "Tenant Management", "Delete tenants", "tenants.delete" },
                    { new Guid("00000000-0000-0000-0004-000000000005"), "Tenant Management", "Manage tenant leases", "tenants.leases" },
                    { new Guid("00000000-0000-0000-0004-000000000006"), "Tenant Management", "Process tenant payments", "tenants.payments" },
                    { new Guid("00000000-0000-0000-0004-000000000007"), "Tenant Management", "View financial reports", "tenants.reports" },
                    { new Guid("00000000-0000-0000-0005-000000000001"), "Sales Management", "View sales", "sales.view" },
                    { new Guid("00000000-0000-0000-0005-000000000002"), "Sales Management", "Create sales", "sales.create" },
                    { new Guid("00000000-0000-0000-0005-000000000003"), "Sales Management", "Edit sales", "sales.edit" },
                    { new Guid("00000000-0000-0000-0005-000000000004"), "Sales Management", "Approve sales", "sales.approve" },
                    { new Guid("00000000-0000-0000-0005-000000000005"), "Sales Management", "Process down payments", "sales.downpayments" },
                    { new Guid("00000000-0000-0000-0005-000000000006"), "Sales Management", "Schedule property tours", "sales.tours" },
                    { new Guid("00000000-0000-0000-0006-000000000001"), "Tenant Portal", "View properties in portal", "portal.properties.view" },
                    { new Guid("00000000-0000-0000-0006-000000000002"), "Tenant Portal", "Schedule property tours", "portal.tours.schedule" },
                    { new Guid("00000000-0000-0000-0006-000000000003"), "Tenant Portal", "Reserve properties", "portal.properties.reserve" },
                    { new Guid("00000000-0000-0000-0006-000000000004"), "Tenant Portal", "Make down payments", "portal.payments.down" },
                    { new Guid("00000000-0000-0000-0006-000000000005"), "Tenant Portal", "View rent schedule", "portal.rent.schedule" },
                    { new Guid("00000000-0000-0000-0006-000000000006"), "Tenant Portal", "Make payments", "portal.payments.make" }
                });

            migrationBuilder.InsertData(
                schema: "authdb",
                table: "Auth_Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "IsSystemRole", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System administrator with full access", true, "Admin", null },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Regular user with basic access", true, "User", null },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manages property registrations, tenant onboarding, and lease administration", true, "Estates Officer", null },
                    { new Guid("00000000-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Oversees property approvals, maintenance, and budget management", true, "Property Manager", null },
                    { new Guid("00000000-0000-0000-0000-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Addresses tenant maintenance requests and manages work orders", true, "Maintenance Officer", null },
                    { new Guid("00000000-0000-0000-0000-000000000006"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Processes rent payments, issues invoices, and handles financial reporting", true, "Finance Team", null },
                    { new Guid("00000000-0000-0000-0000-000000000007"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Handles property tours, bookings, and client onboarding", true, "Sales Officer", null },
                    { new Guid("00000000-0000-0000-0000-000000000008"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manages sales team and property down-payment processing", true, "Sales Manager", null },
                    { new Guid("00000000-0000-0000-0000-000000000009"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Accesses tenant portal for property viewing, payments, and maintenance requests", true, "Tenant", null }
                });

            migrationBuilder.InsertData(
                schema: "authdb",
                table: "Auth_RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000001"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0001-000000000002"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0001-000000000003"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0001-000000000004"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0002-000000000003"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0002-000000000005"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0003-000000000002"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0003-000000000003"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0003-000000000004"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0003-000000000005"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0003-000000000006"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0003-000000000007"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0004-000000000001"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0004-000000000002"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0004-000000000003"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0004-000000000004"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0004-000000000005"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0004-000000000006"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0004-000000000007"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0005-000000000001"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0005-000000000002"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0005-000000000003"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0005-000000000004"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0005-000000000005"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0005-000000000006"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0006-000000000001"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0006-000000000002"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0006-000000000003"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0006-000000000004"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0006-000000000005"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0006-000000000006"), new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0000-000000000003") },
                    { new Guid("00000000-0000-0000-0003-000000000002"), new Guid("00000000-0000-0000-0000-000000000003") },
                    { new Guid("00000000-0000-0000-0003-000000000003"), new Guid("00000000-0000-0000-0000-000000000003") },
                    { new Guid("00000000-0000-0000-0004-000000000001"), new Guid("00000000-0000-0000-0000-000000000003") },
                    { new Guid("00000000-0000-0000-0004-000000000002"), new Guid("00000000-0000-0000-0000-000000000003") },
                    { new Guid("00000000-0000-0000-0004-000000000003"), new Guid("00000000-0000-0000-0000-000000000003") },
                    { new Guid("00000000-0000-0000-0004-000000000005"), new Guid("00000000-0000-0000-0000-000000000003") },
                    { new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0000-000000000004") },
                    { new Guid("00000000-0000-0000-0003-000000000002"), new Guid("00000000-0000-0000-0000-000000000004") },
                    { new Guid("00000000-0000-0000-0003-000000000003"), new Guid("00000000-0000-0000-0000-000000000004") },
                    { new Guid("00000000-0000-0000-0003-000000000005"), new Guid("00000000-0000-0000-0000-000000000004") },
                    { new Guid("00000000-0000-0000-0003-000000000006"), new Guid("00000000-0000-0000-0000-000000000004") },
                    { new Guid("00000000-0000-0000-0003-000000000007"), new Guid("00000000-0000-0000-0000-000000000004") },
                    { new Guid("00000000-0000-0000-0004-000000000001"), new Guid("00000000-0000-0000-0000-000000000004") },
                    { new Guid("00000000-0000-0000-0004-000000000002"), new Guid("00000000-0000-0000-0000-000000000004") },
                    { new Guid("00000000-0000-0000-0004-000000000003"), new Guid("00000000-0000-0000-0000-000000000004") },
                    { new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0000-000000000005") },
                    { new Guid("00000000-0000-0000-0003-000000000006"), new Guid("00000000-0000-0000-0000-000000000005") },
                    { new Guid("00000000-0000-0000-0004-000000000001"), new Guid("00000000-0000-0000-0000-000000000005") },
                    { new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0000-000000000006") },
                    { new Guid("00000000-0000-0000-0004-000000000001"), new Guid("00000000-0000-0000-0000-000000000006") },
                    { new Guid("00000000-0000-0000-0004-000000000006"), new Guid("00000000-0000-0000-0000-000000000006") },
                    { new Guid("00000000-0000-0000-0004-000000000007"), new Guid("00000000-0000-0000-0000-000000000006") },
                    { new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0000-000000000007") },
                    { new Guid("00000000-0000-0000-0005-000000000001"), new Guid("00000000-0000-0000-0000-000000000007") },
                    { new Guid("00000000-0000-0000-0005-000000000002"), new Guid("00000000-0000-0000-0000-000000000007") },
                    { new Guid("00000000-0000-0000-0005-000000000006"), new Guid("00000000-0000-0000-0000-000000000007") },
                    { new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0000-000000000008") },
                    { new Guid("00000000-0000-0000-0005-000000000001"), new Guid("00000000-0000-0000-0000-000000000008") },
                    { new Guid("00000000-0000-0000-0005-000000000002"), new Guid("00000000-0000-0000-0000-000000000008") },
                    { new Guid("00000000-0000-0000-0005-000000000003"), new Guid("00000000-0000-0000-0000-000000000008") },
                    { new Guid("00000000-0000-0000-0005-000000000004"), new Guid("00000000-0000-0000-0000-000000000008") },
                    { new Guid("00000000-0000-0000-0005-000000000005"), new Guid("00000000-0000-0000-0000-000000000008") },
                    { new Guid("00000000-0000-0000-0005-000000000006"), new Guid("00000000-0000-0000-0000-000000000008") },
                    { new Guid("00000000-0000-0000-0006-000000000001"), new Guid("00000000-0000-0000-0000-000000000009") },
                    { new Guid("00000000-0000-0000-0006-000000000002"), new Guid("00000000-0000-0000-0000-000000000009") },
                    { new Guid("00000000-0000-0000-0006-000000000003"), new Guid("00000000-0000-0000-0000-000000000009") },
                    { new Guid("00000000-0000-0000-0006-000000000004"), new Guid("00000000-0000-0000-0000-000000000009") },
                    { new Guid("00000000-0000-0000-0006-000000000005"), new Guid("00000000-0000-0000-0000-000000000009") },
                    { new Guid("00000000-0000-0000-0006-000000000006"), new Guid("00000000-0000-0000-0000-000000000009") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auth_RoleAuditLogs_RoleId",
                schema: "authdb",
                table: "Auth_RoleAuditLogs",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Auth_RoleHierarchies_ChildRoleId",
                schema: "authdb",
                table: "Auth_RoleHierarchies",
                column: "ChildRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Auth_RoleHierarchies_ParentRoleId",
                schema: "authdb",
                table: "Auth_RoleHierarchies",
                column: "ParentRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Auth_RolePermissions_PermissionId",
                schema: "authdb",
                table: "Auth_RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Auth_UserRoles_RoleId",
                schema: "authdb",
                table: "Auth_UserRoles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auth_RoleAuditLogs",
                schema: "authdb");

            migrationBuilder.DropTable(
                name: "Auth_RoleHierarchies",
                schema: "authdb");

            migrationBuilder.DropTable(
                name: "Auth_RolePermissions",
                schema: "authdb");

            migrationBuilder.DropTable(
                name: "Auth_UserRoles",
                schema: "authdb");

            migrationBuilder.DropTable(
                name: "Auth_Permissions",
                schema: "authdb");

            migrationBuilder.DropTable(
                name: "Auth_Roles",
                schema: "authdb");

            migrationBuilder.DropTable(
                name: "Auth_Users",
                schema: "authdb");
        }
    }
}

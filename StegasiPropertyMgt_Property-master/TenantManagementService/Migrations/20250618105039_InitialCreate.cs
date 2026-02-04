using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenantManagementService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tenantdb");

            migrationBuilder.CreateTable(
                name: "Tenant_Tenants",
                schema: "tenantdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PrimaryEmail = table.Column<string>(type: "text", nullable: false),
                    PrimaryTelephone = table.Column<string>(type: "text", nullable: false),
                    TaxIdentificationNumber = table.Column<string>(type: "text", nullable: false),
                    BusinessRegistrationNumber = table.Column<string>(type: "text", nullable: true),
                    NotificationPreferences = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TenantType = table.Column<int>(type: "integer", nullable: false),
                    BillingEntity = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactDetail",
                schema: "tenantdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactDetail_Tenant_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "tenantdb",
                        principalTable: "Tenant_Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tenant_Bookings",
                schema: "tenantdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DownPaymentAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    BookedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenant_Bookings_Tenant_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "tenantdb",
                        principalTable: "Tenant_Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tenant_LeaseAgreements",
                schema: "tenantdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MonthlyRent = table.Column<decimal>(type: "numeric", nullable: false),
                    Terms = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant_LeaseAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenant_LeaseAgreements_Tenant_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "tenantdb",
                        principalTable: "Tenant_Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tenant_RenewalRequests",
                schema: "tenantdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaseAgreementId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    NewTerms = table.Column<string>(type: "text", nullable: false),
                    NewMonthlyRent = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant_RenewalRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenant_RenewalRequests_Tenant_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "tenantdb",
                        principalTable: "Tenant_Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tenant_Invoices",
                schema: "tenantdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenant_Invoices_Tenant_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "tenantdb",
                        principalTable: "Tenant_Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tenant_TerminationProcesses",
                schema: "tenantdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaseAgreementId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    SecurityDepositDeduction = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant_TerminationProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenant_TerminationProcesses_Tenant_LeaseAgreements_LeaseAgr~",
                        column: x => x.LeaseAgreementId,
                        principalSchema: "tenantdb",
                        principalTable: "Tenant_LeaseAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tenant_TerminationProcesses_Tenant_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "tenantdb",
                        principalTable: "Tenant_Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "tenantdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric", nullable: false),
                    PaidOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Tenant_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "tenantdb",
                        principalTable: "Tenant_Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionReports",
                schema: "tenantdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TerminationProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDetails = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionReports_Tenant_TerminationProcesses_TerminationPr~",
                        column: x => x.TerminationProcessId,
                        principalSchema: "tenantdb",
                        principalTable: "Tenant_TerminationProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactDetail_TenantId",
                schema: "tenantdb",
                table: "ContactDetail",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_TerminationProcessId",
                schema: "tenantdb",
                table: "InspectionReports",
                column: "TerminationProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                schema: "tenantdb",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_Bookings_TenantId",
                schema: "tenantdb",
                table: "Tenant_Bookings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_Invoices_BookingId",
                schema: "tenantdb",
                table: "Tenant_Invoices",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_LeaseAgreements_TenantId",
                schema: "tenantdb",
                table: "Tenant_LeaseAgreements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_RenewalRequests_TenantId",
                schema: "tenantdb",
                table: "Tenant_RenewalRequests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_TerminationProcesses_LeaseAgreementId",
                schema: "tenantdb",
                table: "Tenant_TerminationProcesses",
                column: "LeaseAgreementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_TerminationProcesses_TenantId",
                schema: "tenantdb",
                table: "Tenant_TerminationProcesses",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactDetail",
                schema: "tenantdb");

            migrationBuilder.DropTable(
                name: "InspectionReports",
                schema: "tenantdb");

            migrationBuilder.DropTable(
                name: "Payments",
                schema: "tenantdb");

            migrationBuilder.DropTable(
                name: "Tenant_RenewalRequests",
                schema: "tenantdb");

            migrationBuilder.DropTable(
                name: "Tenant_TerminationProcesses",
                schema: "tenantdb");

            migrationBuilder.DropTable(
                name: "Tenant_Invoices",
                schema: "tenantdb");

            migrationBuilder.DropTable(
                name: "Tenant_LeaseAgreements",
                schema: "tenantdb");

            migrationBuilder.DropTable(
                name: "Tenant_Bookings",
                schema: "tenantdb");

            migrationBuilder.DropTable(
                name: "Tenant_Tenants",
                schema: "tenantdb");
        }
    }
}

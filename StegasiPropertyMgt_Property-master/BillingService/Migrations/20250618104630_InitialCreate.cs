using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "billingdb");

            migrationBuilder.CreateTable(
                name: "Tenant",
                schema: "billingdb",
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
                    table.PrimaryKey("PK_Tenant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Booking",
                schema: "billingdb",
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
                    table.PrimaryKey("PK_Booking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Booking_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "billingdb",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactDetail",
                schema: "billingdb",
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
                        name: "FK_ContactDetail_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "billingdb",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaseAgreement",
                schema: "billingdb",
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
                    table.PrimaryKey("PK_LeaseAgreement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaseAgreement_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "billingdb",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RenewalRequest",
                schema: "billingdb",
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
                    table.PrimaryKey("PK_RenewalRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RenewalRequest_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "billingdb",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                schema: "billingdb",
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
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Booking_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "billingdb",
                        principalTable: "Booking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TerminationProcess",
                schema: "billingdb",
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
                    table.PrimaryKey("PK_TerminationProcess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerminationProcess_LeaseAgreement_LeaseAgreementId",
                        column: x => x.LeaseAgreementId,
                        principalSchema: "billingdb",
                        principalTable: "LeaseAgreement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerminationProcess_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "billingdb",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "billingdb",
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
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "billingdb",
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionReport",
                schema: "billingdb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TerminationProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDetails = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionReport_TerminationProcess_TerminationProcessId",
                        column: x => x.TerminationProcessId,
                        principalSchema: "billingdb",
                        principalTable: "TerminationProcess",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_TenantId",
                schema: "billingdb",
                table: "Booking",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactDetail_TenantId",
                schema: "billingdb",
                table: "ContactDetail",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReport_TerminationProcessId",
                schema: "billingdb",
                table: "InspectionReport",
                column: "TerminationProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BookingId",
                schema: "billingdb",
                table: "Invoices",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseAgreement_TenantId",
                schema: "billingdb",
                table: "LeaseAgreement",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                schema: "billingdb",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RenewalRequest_TenantId",
                schema: "billingdb",
                table: "RenewalRequest",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminationProcess_LeaseAgreementId",
                schema: "billingdb",
                table: "TerminationProcess",
                column: "LeaseAgreementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TerminationProcess_TenantId",
                schema: "billingdb",
                table: "TerminationProcess",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactDetail",
                schema: "billingdb");

            migrationBuilder.DropTable(
                name: "InspectionReport",
                schema: "billingdb");

            migrationBuilder.DropTable(
                name: "Payments",
                schema: "billingdb");

            migrationBuilder.DropTable(
                name: "RenewalRequest",
                schema: "billingdb");

            migrationBuilder.DropTable(
                name: "TerminationProcess",
                schema: "billingdb");

            migrationBuilder.DropTable(
                name: "Invoices",
                schema: "billingdb");

            migrationBuilder.DropTable(
                name: "LeaseAgreement",
                schema: "billingdb");

            migrationBuilder.DropTable(
                name: "Booking",
                schema: "billingdb");

            migrationBuilder.DropTable(
                name: "Tenant",
                schema: "billingdb");
        }
    }
}

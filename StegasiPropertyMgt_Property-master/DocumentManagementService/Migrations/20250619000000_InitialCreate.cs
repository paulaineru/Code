using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentManagementService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the document_management schema
            migrationBuilder.EnsureSchema(name: "document_management");

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                schema: "document_management",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentCategories",
                schema: "document_management",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    ValidationRules = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AllowedFileTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpiryWarningDays = table.Column<int>(type: "integer", nullable: true),
                    MaxFileSizeInBytes = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentCategories_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalSchema: "document_management",
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentSubCategories",
                schema: "document_management",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    ValidationRules = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AllowedFileTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpiryWarningDays = table.Column<int>(type: "integer", nullable: true),
                    MaxFileSizeInBytes = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentSubCategories_DocumentCategories_DocumentCategoryId",
                        column: x => x.DocumentCategoryId,
                        principalSchema: "document_management",
                        principalTable: "DocumentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentRequirements",
                schema: "document_management",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubProcess = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentSubCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ValidationRules = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ExpiryWarningDays = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentRequirements_DocumentCategories_DocumentCategoryId",
                        column: x => x.DocumentCategoryId,
                        principalSchema: "document_management",
                        principalTable: "DocumentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentRequirements_DocumentSubCategories_DocumentSubCategoryId",
                        column: x => x.DocumentSubCategoryId,
                        principalSchema: "document_management",
                        principalTable: "DocumentSubCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentRequirements_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalSchema: "document_management",
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PropertyDocuments",
                schema: "document_management",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentSubCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaintenanceTicketId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    RenewalRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    TerminationProcessId = table.Column<Guid>(type: "uuid", nullable: true),
                    BillId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Tags = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyDocuments_DocumentCategories_DocumentCategoryId",
                        column: x => x.DocumentCategoryId,
                        principalSchema: "document_management",
                        principalTable: "DocumentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyDocuments_DocumentSubCategories_DocumentSubCategoryId",
                        column: x => x.DocumentSubCategoryId,
                        principalSchema: "document_management",
                        principalTable: "DocumentSubCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyDocuments_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalSchema: "document_management",
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCategories_Code",
                table: "DocumentCategories",
                schema: "document_management",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCategories_DocumentTypeId_Name",
                table: "DocumentCategories",
                schema: "document_management",
                columns: new[] { "DocumentTypeId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCategories_DisplayOrder",
                table: "DocumentCategories",
                schema: "document_management",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCategories_IsActive",
                table: "DocumentCategories",
                schema: "document_management",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRequirements_DisplayOrder",
                table: "DocumentRequirements",
                schema: "document_management",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRequirements_IsActive",
                table: "DocumentRequirements",
                schema: "document_management",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRequirements_ProcessType",
                table: "DocumentRequirements",
                schema: "document_management",
                column: "ProcessType");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRequirements_ProcessType_SubProcess_DocumentTypeId_DocumentCategoryId_DocumentSubCategoryId",
                table: "DocumentRequirements",
                schema: "document_management",
                columns: new[] { "ProcessType", "SubProcess", "DocumentTypeId", "DocumentCategoryId", "DocumentSubCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSubCategories_Code",
                table: "DocumentSubCategories",
                schema: "document_management",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSubCategories_DocumentCategoryId_Name",
                table: "DocumentSubCategories",
                schema: "document_management",
                columns: new[] { "DocumentCategoryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSubCategories_DisplayOrder",
                table: "DocumentSubCategories",
                schema: "document_management",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSubCategories_IsActive",
                table: "DocumentSubCategories",
                schema: "document_management",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_DisplayOrder",
                table: "DocumentTypes",
                schema: "document_management",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_IsActive",
                table: "DocumentTypes",
                schema: "document_management",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Name",
                table: "DocumentTypes",
                schema: "document_management",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_BillId",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_BookingId",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_CreatedAt",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_DocumentCategoryId",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "DocumentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_DocumentSubCategoryId",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "DocumentSubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_DocumentTypeId",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_ExpiryDate",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_IsRequired",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "IsRequired");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_IsVerified",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_LeaseId",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "LeaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_MaintenanceTicketId",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "MaintenanceTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_PaymentId",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_PropertyId",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_RenewalRequestId",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "RenewalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_Status",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_TenantId",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_TerminationProcessId",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "TerminationProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_UploadedAt",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_UploadedBy",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyDocuments_VerifiedBy",
                table: "PropertyDocuments",
                schema: "document_management",
                column: "VerifiedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentRequirements",
                schema: "document_management");

            migrationBuilder.DropTable(
                name: "PropertyDocuments",
                schema: "document_management");

            migrationBuilder.DropTable(
                name: "DocumentSubCategories",
                schema: "document_management");

            migrationBuilder.DropTable(
                name: "DocumentCategories",
                schema: "document_management");

            migrationBuilder.DropTable(
                name: "DocumentTypes",
                schema: "document_management");
        }
    }
} 
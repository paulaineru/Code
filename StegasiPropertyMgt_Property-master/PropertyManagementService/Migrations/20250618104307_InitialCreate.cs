using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyManagementService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "propertydb");

            migrationBuilder.CreateTable(
                name: "Amenities",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prop_MaintenanceRecords",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DateReported = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RepairCost = table.Column<decimal>(type: "numeric", nullable: true),
                    DateResolved = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prop_MaintenanceRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prop_Properties",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyManagerId = table.Column<string>(type: "text", nullable: true),
                    YearOfCommissionOrPurchase = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "EXTRACT(YEAR FROM CURRENT_TIMESTAMP)"),
                    FairValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    InsurableValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    OwnershipStatus = table.Column<string>(type: "text", nullable: false),
                    SalePrice = table.Column<decimal>(type: "numeric", nullable: true),
                    IsRentable = table.Column<bool>(type: "boolean", nullable: false),
                    IsSaleable = table.Column<bool>(type: "boolean", nullable: false),
                    RentPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    PropertyType = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    NumberOfStories = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    NumberOfBedrooms = table.Column<int>(type: "integer", nullable: true),
                    NumberOfBathrooms = table.Column<int>(type: "integer", nullable: true),
                    NumberOfWings = table.Column<int>(type: "integer", nullable: true),
                    TotalParkingAreaPerFloor = table.Column<decimal>(type: "numeric", nullable: true),
                    NumberOfUnitsPerFloor = table.Column<int>(type: "integer", nullable: true),
                    HOAFees = table.Column<decimal>(type: "numeric", nullable: true),
                    NumberOfClusters = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    BlockNumber = table.Column<string>(type: "text", nullable: true),
                    PlotNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Acreage = table.Column<decimal>(type: "numeric", maxLength: 50, nullable: true),
                    VillaProperty_NumberOfBedrooms = table.Column<int>(type: "integer", nullable: true),
                    VillaProperty_NumberOfBathrooms = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Properties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prop_PropertyAmenities",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prop_PropertyAmenities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prop_PropertyFeatures",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prop_PropertyFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prop_PropertyImages",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prop_PropertyImages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenant",
                schema: "propertydb",
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
                name: "CondominiumUnits",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitNumber = table.Column<string>(type: "text", nullable: false),
                    Bedrooms = table.Column<int>(type: "integer", nullable: false),
                    Bathrooms = table.Column<int>(type: "integer", nullable: false),
                    FloorNumber = table.Column<int>(type: "integer", nullable: false),
                    SizeSquareFeet = table.Column<int>(type: "integer", nullable: false),
                    MonthlyRent = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CondominiumUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CondominiumUnits_Prop_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "propertydb",
                        principalTable: "Prop_Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyAmenities",
                schema: "propertydb",
                columns: table => new
                {
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmenityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyAmenities", x => new { x.PropertyId, x.AmenityId });
                    table.ForeignKey(
                        name: "FK_PropertyAmenities_Amenities_AmenityId",
                        column: x => x.AmenityId,
                        principalSchema: "propertydb",
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyAmenities_Prop_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "propertydb",
                        principalTable: "Prop_Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyCertifications",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IssuingAuthority = table.Column<string>(type: "text", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DocumentUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyCertifications_Prop_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "propertydb",
                        principalTable: "Prop_Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyCompliance",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegulationName = table.Column<string>(type: "text", nullable: false),
                    ComplianceStatus = table.Column<string>(type: "text", nullable: false),
                    LastInspectionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextInspectionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InspectionNotes = table.Column<string>(type: "text", nullable: false),
                    Violations = table.Column<string>(type: "text", nullable: false),
                    CorrectiveActions = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyCompliance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyCompliance_Prop_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "propertydb",
                        principalTable: "Prop_Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyFeatures",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    HasParking = table.Column<bool>(type: "boolean", nullable: false),
                    HasGarden = table.Column<bool>(type: "boolean", nullable: false),
                    HasPool = table.Column<bool>(type: "boolean", nullable: false),
                    HasGym = table.Column<bool>(type: "boolean", nullable: false),
                    HasSecurity = table.Column<bool>(type: "boolean", nullable: false),
                    HasAirConditioning = table.Column<bool>(type: "boolean", nullable: false),
                    HasHeating = table.Column<bool>(type: "boolean", nullable: false),
                    HasInternet = table.Column<bool>(type: "boolean", nullable: false),
                    IsFurnished = table.Column<bool>(type: "boolean", nullable: false),
                    PetsAllowed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyFeatures_Prop_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "propertydb",
                        principalTable: "Prop_Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyRegulations",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Jurisdiction = table.Column<string>(type: "text", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ComplianceRequirements = table.Column<string>(type: "text", nullable: false),
                    Penalties = table.Column<string>(type: "text", nullable: false),
                    DocumentationRequirements = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyRegulations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyRegulations_Prop_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "propertydb",
                        principalTable: "Prop_Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyServices",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceType = table.Column<string>(type: "text", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric", nullable: false),
                    Frequency = table.Column<string>(type: "text", nullable: false),
                    LastServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyServices_Prop_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "propertydb",
                        principalTable: "Prop_Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyStandards",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Requirements = table.Column<string>(type: "text", nullable: false),
                    Specifications = table.Column<string>(type: "text", nullable: false),
                    ComplianceLevel = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyStandards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyStandards_Prop_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "propertydb",
                        principalTable: "Prop_Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TownhouseClusters",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClusterName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NumberOfUnits = table.Column<int>(type: "integer", nullable: false),
                    CommonAreaSize = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TownhouseClusters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TownhouseClusters_Prop_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "propertydb",
                        principalTable: "Prop_Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WingDetails",
                schema: "propertydb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    WingName = table.Column<string>(type: "text", nullable: false),
                    FloorArea = table.Column<int>(type: "integer", nullable: false),
                    CommonArea = table.Column<int>(type: "integer", nullable: false),
                    UsageType = table.Column<string>(type: "text", nullable: false),
                    RentalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    FloorNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WingDetails_Prop_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "propertydb",
                        principalTable: "Prop_Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Booking",
                schema: "propertydb",
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
                        principalSchema: "propertydb",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactDetail",
                schema: "propertydb",
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
                        principalSchema: "propertydb",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaseAgreement",
                schema: "propertydb",
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
                        name: "FK_LeaseAgreement_Prop_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "propertydb",
                        principalTable: "Prop_Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaseAgreement_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "propertydb",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RenewalRequest",
                schema: "propertydb",
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
                        principalSchema: "propertydb",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TerminationProcess",
                schema: "propertydb",
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
                        principalSchema: "propertydb",
                        principalTable: "LeaseAgreement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerminationProcess_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "propertydb",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionReport",
                schema: "propertydb",
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
                        principalSchema: "propertydb",
                        principalTable: "TerminationProcess",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_Name",
                schema: "propertydb",
                table: "Amenities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Booking_TenantId",
                schema: "propertydb",
                table: "Booking",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CondominiumUnits_PropertyId",
                schema: "propertydb",
                table: "CondominiumUnits",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactDetail_TenantId",
                schema: "propertydb",
                table: "ContactDetail",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReport_TerminationProcessId",
                schema: "propertydb",
                table: "InspectionReport",
                column: "TerminationProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseAgreement_PropertyId",
                schema: "propertydb",
                table: "LeaseAgreement",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseAgreements_TenantId",
                schema: "propertydb",
                table: "LeaseAgreement",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_ApprovalStatus",
                schema: "propertydb",
                table: "Prop_Properties",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_OwnerId",
                schema: "propertydb",
                table: "Prop_Properties",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAmenities_AmenityId",
                schema: "propertydb",
                table: "PropertyAmenities",
                column: "AmenityId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyCertifications_PropertyId",
                schema: "propertydb",
                table: "PropertyCertifications",
                column: "PropertyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyCompliance_PropertyId",
                schema: "propertydb",
                table: "PropertyCompliance",
                column: "PropertyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFeatures_PropertyId",
                schema: "propertydb",
                table: "PropertyFeatures",
                column: "PropertyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyRegulations_PropertyId",
                schema: "propertydb",
                table: "PropertyRegulations",
                column: "PropertyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyServices_PropertyId",
                schema: "propertydb",
                table: "PropertyServices",
                column: "PropertyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyStandards_PropertyId",
                schema: "propertydb",
                table: "PropertyStandards",
                column: "PropertyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RenewalRequest_TenantId",
                schema: "propertydb",
                table: "RenewalRequest",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminationProcess_LeaseAgreementId",
                schema: "propertydb",
                table: "TerminationProcess",
                column: "LeaseAgreementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TerminationProcess_TenantId",
                schema: "propertydb",
                table: "TerminationProcess",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TownhouseClusters_PropertyId",
                schema: "propertydb",
                table: "TownhouseClusters",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_WingDetails_PropertyId",
                schema: "propertydb",
                table: "WingDetails",
                column: "PropertyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Booking",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "CondominiumUnits",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "ContactDetail",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "InspectionReport",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "Prop_MaintenanceRecords",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "Prop_PropertyAmenities",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "Prop_PropertyFeatures",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "Prop_PropertyImages",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "PropertyAmenities",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "PropertyCertifications",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "PropertyCompliance",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "PropertyFeatures",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "PropertyRegulations",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "PropertyServices",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "PropertyStandards",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "RenewalRequest",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "TownhouseClusters",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "WingDetails",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "TerminationProcess",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "Amenities",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "LeaseAgreement",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "Prop_Properties",
                schema: "propertydb");

            migrationBuilder.DropTable(
                name: "Tenant",
                schema: "propertydb");
        }
    }
}

-- Create schema
CREATE SCHEMA IF NOT EXISTS tenantdb;

-- Create migrations history table
CREATE TABLE IF NOT EXISTS tenantdb."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Create tenants table
CREATE TABLE IF NOT EXISTS tenantdb."Tenant_Tenants" (
    "Id" uuid PRIMARY KEY,
    "Name" text NOT NULL,
    "PrimaryEmail" text NOT NULL,
    "PrimaryTelephone" text NOT NULL,
    "TaxIdentificationNumber" text NOT NULL,
    "BusinessRegistrationNumber" text,
    "NotificationPreferences" integer NOT NULL,
    "Status" integer NOT NULL,
    "TenantType" integer NOT NULL,
    "BillingEntity" text NOT NULL
);

-- Create lease agreements table
CREATE TABLE IF NOT EXISTS tenantdb."Tenant_LeaseAgreements" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "PropertyId" uuid NOT NULL,
    "StartDate" timestamp with time zone NOT NULL,
    "EndDate" timestamp with time zone NOT NULL,
    "MonthlyRent" numeric NOT NULL,
    "Status" text NOT NULL,
    "Terms" text NOT NULL,
    "ApproverId" uuid,
    CONSTRAINT "FK_Tenant_LeaseAgreements_Tenant_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES tenantdb."Tenant_Tenants"("Id") ON DELETE CASCADE
);

-- Create bookings table
CREATE TABLE IF NOT EXISTS tenantdb."Tenant_Bookings" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "PropertyId" uuid NOT NULL,
    "StartDate" timestamp with time zone NOT NULL,
    "EndDate" timestamp with time zone NOT NULL,
    "BookedOn" timestamp with time zone NOT NULL,
    "DownPaymentAmount" numeric NOT NULL,
    "Status" integer NOT NULL,
    CONSTRAINT "FK_Tenant_Bookings_Tenant_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES tenantdb."Tenant_Tenants"("Id") ON DELETE CASCADE
);

-- Create invoices table
CREATE TABLE IF NOT EXISTS tenantdb."Tenant_Invoices" (
    "Id" uuid PRIMARY KEY,
    "BookingId" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "Amount" numeric NOT NULL,
    "DueDate" timestamp with time zone NOT NULL,
    "StartDate" timestamp with time zone NOT NULL,
    "EndDate" timestamp with time zone NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastUpdated" timestamp with time zone NOT NULL,
    CONSTRAINT "FK_Tenant_Invoices_Tenant_Bookings_BookingId" FOREIGN KEY ("BookingId") REFERENCES tenantdb."Tenant_Bookings"("Id") ON DELETE CASCADE
);

-- Create renewal requests table
CREATE TABLE IF NOT EXISTS tenantdb."Tenant_RenewalRequests" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "LeaseAgreementId" uuid NOT NULL,
    "NewMonthlyRent" numeric,
    "NewTerms" text NOT NULL,
    "Status" integer NOT NULL,
    CONSTRAINT "FK_Tenant_RenewalRequests_Tenant_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES tenantdb."Tenant_Tenants"("Id") ON DELETE CASCADE
);

-- Create termination processes table
CREATE TABLE IF NOT EXISTS tenantdb."Tenant_TerminationProcesses" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "LeaseAgreementId" uuid NOT NULL UNIQUE,
    "InitiatedOn" timestamp with time zone NOT NULL,
    "OutstandingAmount" numeric NOT NULL,
    "SecurityDepositDeduction" numeric NOT NULL,
    "Status" integer NOT NULL,
    CONSTRAINT "FK_Tenant_TerminationProcesses_Tenant_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES tenantdb."Tenant_Tenants"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Tenant_TerminationProcesses_Tenant_LeaseAgreements_LeaseAgreementId" FOREIGN KEY ("LeaseAgreementId") REFERENCES tenantdb."Tenant_LeaseAgreements"("Id") ON DELETE CASCADE
);

-- Create inspection reports table
CREATE TABLE IF NOT EXISTS tenantdb."InspectionReports" (
    "Id" uuid PRIMARY KEY,
    "TerminationProcessId" uuid NOT NULL,
    "ReportDetails" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "FK_InspectionReports_Tenant_TerminationProcesses_TerminationProcessId" FOREIGN KEY ("TerminationProcessId") REFERENCES tenantdb."Tenant_TerminationProcesses"("Id") ON DELETE CASCADE
);

-- Create contact details table
CREATE TABLE IF NOT EXISTS tenantdb."ContactDetail" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "Type" text NOT NULL,
    "Value" text NOT NULL,
    CONSTRAINT "FK_ContactDetail_Tenant_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES tenantdb."Tenant_Tenants"("Id") ON DELETE CASCADE
);

-- Create payments table
CREATE TABLE IF NOT EXISTS tenantdb."Payments" (
    "Id" uuid PRIMARY KEY,
    "InvoiceId" uuid NOT NULL,
    "AmountPaid" numeric NOT NULL,
    "PaymentDate" timestamp with time zone NOT NULL,
    "PaidOn" timestamp with time zone NOT NULL,
    "PaymentMethod" text NOT NULL,
    "Status" integer NOT NULL,
    "Reference" text,
    "ProcessedAt" timestamp with time zone,
    CONSTRAINT "FK_Payments_Tenant_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES tenantdb."Tenant_Invoices"("Id") ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_Tenant_LeaseAgreements_TenantId" ON tenantdb."Tenant_LeaseAgreements"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_Tenant_Bookings_TenantId" ON tenantdb."Tenant_Bookings"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_Tenant_Invoices_BookingId" ON tenantdb."Tenant_Invoices"("BookingId");
CREATE INDEX IF NOT EXISTS "IX_Tenant_RenewalRequests_TenantId" ON tenantdb."Tenant_RenewalRequests"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_Tenant_TerminationProcesses_TenantId" ON tenantdb."Tenant_TerminationProcesses"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_InspectionReports_TerminationProcessId" ON tenantdb."InspectionReports"("TerminationProcessId");
CREATE INDEX IF NOT EXISTS "IX_ContactDetail_TenantId" ON tenantdb."ContactDetail"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_Payments_InvoiceId" ON tenantdb."Payments"("InvoiceId");

-- Insert migration record
INSERT INTO tenantdb."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250618094712_TenantDbInitial', '8.0.0')
ON CONFLICT DO NOTHING; 
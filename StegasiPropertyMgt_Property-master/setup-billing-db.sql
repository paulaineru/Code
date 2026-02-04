-- Create schema
CREATE SCHEMA IF NOT EXISTS billingdb;

-- Create migrations history table
CREATE TABLE IF NOT EXISTS billingdb."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Create bills table
CREATE TABLE IF NOT EXISTS billingdb."Billing_Bills" (
    "Id" SERIAL PRIMARY KEY,
    "TenantId" integer NOT NULL,
    "PropertyId" integer NOT NULL,
    "Amount" numeric NOT NULL,
    "DueDate" timestamp with time zone NOT NULL,
    "Status" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create payments table
CREATE TABLE IF NOT EXISTS billingdb."Billing_Payments" (
    "Id" SERIAL PRIMARY KEY,
    "BillId" integer NOT NULL,
    "Amount" numeric NOT NULL,
    "PaymentDate" timestamp with time zone NOT NULL,
    "PaymentMethod" text NOT NULL,
    "Status" text NOT NULL,
    "TransactionId" text,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_Billing_Payments_Billing_Bills_BillId" FOREIGN KEY ("BillId") REFERENCES billingdb."Billing_Bills"("Id") ON DELETE CASCADE
);

-- Create tenant table
CREATE TABLE IF NOT EXISTS billingdb."Tenant" (
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

-- Create booking table
CREATE TABLE IF NOT EXISTS billingdb."Booking" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "PropertyId" uuid NOT NULL,
    "StartDate" timestamp with time zone NOT NULL,
    "EndDate" timestamp with time zone NOT NULL,
    "BookedOn" timestamp with time zone NOT NULL,
    "DownPaymentAmount" numeric NOT NULL,
    "Status" integer NOT NULL,
    CONSTRAINT "FK_Booking_Tenant_TenantId" FOREIGN KEY ("TenantId") REFERENCES billingdb."Tenant"("Id") ON DELETE CASCADE
);

-- Create invoices table
CREATE TABLE IF NOT EXISTS billingdb."Invoices" (
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
    CONSTRAINT "FK_Invoices_Booking_BookingId" FOREIGN KEY ("BookingId") REFERENCES billingdb."Booking"("Id") ON DELETE CASCADE
);

-- Create payments table (different from Billing_Payments)
CREATE TABLE IF NOT EXISTS billingdb."Payments" (
    "Id" uuid PRIMARY KEY,
    "InvoiceId" uuid NOT NULL,
    "AmountPaid" numeric NOT NULL,
    "PaymentDate" timestamp with time zone NOT NULL,
    "PaidOn" timestamp with time zone NOT NULL,
    "PaymentMethod" text NOT NULL,
    "Status" integer NOT NULL,
    "Reference" text,
    "ProcessedAt" timestamp with time zone,
    CONSTRAINT "FK_Payments_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES billingdb."Invoices"("Id") ON DELETE CASCADE
);

-- Create lease agreement table
CREATE TABLE IF NOT EXISTS billingdb."LeaseAgreement" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "PropertyId" uuid NOT NULL,
    "StartDate" timestamp with time zone NOT NULL,
    "EndDate" timestamp with time zone NOT NULL,
    "MonthlyRent" numeric NOT NULL,
    "Status" text NOT NULL,
    "Terms" text NOT NULL,
    "ApproverId" uuid,
    CONSTRAINT "FK_LeaseAgreement_Tenant_TenantId" FOREIGN KEY ("TenantId") REFERENCES billingdb."Tenant"("Id") ON DELETE CASCADE
);

-- Create renewal request table
CREATE TABLE IF NOT EXISTS billingdb."RenewalRequest" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "LeaseAgreementId" uuid NOT NULL,
    "NewMonthlyRent" numeric,
    "NewTerms" text NOT NULL,
    "Status" integer NOT NULL,
    CONSTRAINT "FK_RenewalRequest_Tenant_TenantId" FOREIGN KEY ("TenantId") REFERENCES billingdb."Tenant"("Id") ON DELETE CASCADE
);

-- Create termination process table
CREATE TABLE IF NOT EXISTS billingdb."TerminationProcess" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "LeaseAgreementId" uuid NOT NULL UNIQUE,
    "InitiatedOn" timestamp with time zone NOT NULL,
    "OutstandingAmount" numeric NOT NULL,
    "SecurityDepositDeduction" numeric NOT NULL,
    "Status" integer NOT NULL,
    CONSTRAINT "FK_TerminationProcess_Tenant_TenantId" FOREIGN KEY ("TenantId") REFERENCES billingdb."Tenant"("Id") ON DELETE CASCADE
);

-- Create inspection report table
CREATE TABLE IF NOT EXISTS billingdb."InspectionReport" (
    "Id" uuid PRIMARY KEY,
    "TerminationProcessId" uuid NOT NULL,
    "ReportDetails" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "FK_InspectionReport_TerminationProcess_TerminationProcessId" FOREIGN KEY ("TerminationProcessId") REFERENCES billingdb."TerminationProcess"("Id") ON DELETE CASCADE
);

-- Create contact detail table
CREATE TABLE IF NOT EXISTS billingdb."ContactDetail" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "Type" text NOT NULL,
    "Value" text NOT NULL,
    CONSTRAINT "FK_ContactDetail_Tenant_TenantId" FOREIGN KEY ("TenantId") REFERENCES billingdb."Tenant"("Id") ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_Billing_Payments_BillId" ON billingdb."Billing_Payments"("BillId");
CREATE INDEX IF NOT EXISTS "IX_Booking_TenantId" ON billingdb."Booking"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_Invoices_BookingId" ON billingdb."Invoices"("BookingId");
CREATE INDEX IF NOT EXISTS "IX_Payments_InvoiceId" ON billingdb."Payments"("InvoiceId");
CREATE INDEX IF NOT EXISTS "IX_LeaseAgreement_TenantId" ON billingdb."LeaseAgreement"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_RenewalRequest_TenantId" ON billingdb."RenewalRequest"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_TerminationProcess_TenantId" ON billingdb."TerminationProcess"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_InspectionReport_TerminationProcessId" ON billingdb."InspectionReport"("TerminationProcessId");
CREATE INDEX IF NOT EXISTS "IX_ContactDetail_TenantId" ON billingdb."ContactDetail"("TenantId");

-- Insert migration record
INSERT INTO billingdb."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250618094812_BillingDbInitial', '8.0.0')
ON CONFLICT DO NOTHING; 
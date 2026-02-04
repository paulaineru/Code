-- Create schema
CREATE SCHEMA IF NOT EXISTS approvalworkflow;

-- Create migrations history table
CREATE TABLE IF NOT EXISTS approvalworkflow."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Create approval workflows table
CREATE TABLE IF NOT EXISTS approvalworkflow."ApprovalWorkflows" (
    "Id" uuid PRIMARY KEY,
    "Module" text NOT NULL,
    "EntityId" uuid NOT NULL,
    "EntityType" text NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastUpdatedAt" timestamp with time zone,
    "CreatedBy" uuid NOT NULL,
    "Comments" text,
    "Metadata" text NOT NULL
);

-- Create approval stages table
CREATE TABLE IF NOT EXISTS approvalworkflow."ApprovalStages" (
    "Id" uuid PRIMARY KEY,
    "StageNumber" integer NOT NULL,
    "Role" text NOT NULL,
    "Status" integer NOT NULL,
    "ApprovedAt" timestamp with time zone,
    "ApprovedBy" uuid,
    "Comments" text,
    "IsRequired" boolean NOT NULL,
    "Order" integer NOT NULL,
    "WorkflowId" uuid,
    CONSTRAINT "FK_ApprovalStages_ApprovalWorkflows_WorkflowId" FOREIGN KEY ("WorkflowId") REFERENCES approvalworkflow."ApprovalWorkflows"("Id") ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_ApprovalStages_WorkflowId" ON approvalworkflow."ApprovalStages"("WorkflowId");
CREATE INDEX IF NOT EXISTS "IX_ApprovalWorkflows_Module" ON approvalworkflow."ApprovalWorkflows"("Module");
CREATE INDEX IF NOT EXISTS "IX_ApprovalWorkflows_EntityId" ON approvalworkflow."ApprovalWorkflows"("EntityId");
CREATE INDEX IF NOT EXISTS "IX_ApprovalWorkflows_Status" ON approvalworkflow."ApprovalWorkflows"("Status");

-- Insert migration record
INSERT INTO approvalworkflow."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250616142837_InitialApprovalWorkflowMigration', '8.0.2')
ON CONFLICT DO NOTHING; 
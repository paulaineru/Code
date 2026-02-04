-- Create schema if it doesn't exist
CREATE SCHEMA IF NOT EXISTS approvalworkflow;

-- Verify if tables exist and create them if they don't
DO $$
BEGIN
    -- Check if ApprovalWorkflows table exists
    IF NOT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'approvalworkflow' 
        AND table_name = 'approvalworkflows'
    ) THEN
        CREATE TABLE approvalworkflow."ApprovalWorkflows" (
            "Id" uuid NOT NULL,
            "Module" text NOT NULL,
            "EntityId" uuid NOT NULL,
            "EntityType" text NOT NULL,
            "Status" integer NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "LastUpdatedAt" timestamp with time zone NULL,
            "CreatedBy" uuid NOT NULL,
            "Comments" text NULL,
            "Metadata" text NOT NULL,
            CONSTRAINT "PK_ApprovalWorkflows" PRIMARY KEY ("Id")
        );
    END IF;

    -- Check if ApprovalStages table exists
    IF NOT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'approvalworkflow' 
        AND table_name = 'approvalstages'
    ) THEN
        CREATE TABLE approvalworkflow."ApprovalStages" (
            "Id" uuid NOT NULL,
            "StageNumber" integer NOT NULL,
            "Role" text NOT NULL,
            "Status" integer NOT NULL,
            "ApprovedAt" timestamp with time zone NULL,
            "ApprovedBy" uuid NULL,
            "Comments" text NULL,
            "IsRequired" boolean NOT NULL,
            "Order" integer NOT NULL,
            "WorkflowId" uuid NULL,
            CONSTRAINT "PK_ApprovalStages" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_ApprovalStages_ApprovalWorkflows_WorkflowId" FOREIGN KEY ("WorkflowId")
                REFERENCES approvalworkflow."ApprovalWorkflows" ("Id") ON DELETE CASCADE
        );

        -- Create index for foreign key
        CREATE INDEX "IX_ApprovalStages_WorkflowId" ON approvalworkflow."ApprovalStages" ("WorkflowId");
    END IF;
END $$; 
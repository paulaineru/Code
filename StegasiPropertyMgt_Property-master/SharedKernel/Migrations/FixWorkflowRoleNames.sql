-- Fix role names in existing workflow data
-- Update "EstatesOfficer" to "Estates Officer" to match the actual role names in the database

UPDATE "ApprovalStages" 
SET "Role" = 'Estates Officer' 
WHERE "Role" = 'EstatesOfficer';

-- Verify the update
SELECT "Id", "StageNumber", "Role", "Status" 
FROM "ApprovalStages" 
WHERE "Role" LIKE '%Estates%'; 
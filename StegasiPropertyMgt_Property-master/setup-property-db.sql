-- Create schema
CREATE SCHEMA IF NOT EXISTS propertydb;

-- Create migrations history table
CREATE TABLE IF NOT EXISTS propertydb."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Create properties table
CREATE TABLE IF NOT EXISTS propertydb."Prop_Properties" (
    "Id" SERIAL PRIMARY KEY,
    "Name" text NOT NULL,
    "Description" text NOT NULL,
    "Address" text NOT NULL,
    "Type" text NOT NULL,
    "Status" text NOT NULL,
    "Price" numeric NOT NULL,
    "Size" numeric NOT NULL,
    "Bedrooms" integer NOT NULL,
    "Bathrooms" integer NOT NULL,
    "YearBuilt" integer NOT NULL,
    "LastRenovated" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "PropertyType" text NOT NULL,
    "IsAvailable" boolean NOT NULL DEFAULT true,
    "Location" text NOT NULL,
    "City" text NOT NULL,
    "State" text NOT NULL,
    "ZipCode" text NOT NULL,
    "Country" text NOT NULL
);

-- Create property features table
CREATE TABLE IF NOT EXISTS propertydb."Prop_PropertyFeatures" (
    "Id" SERIAL PRIMARY KEY,
    "PropertyId" integer NOT NULL,
    "HasParking" boolean NOT NULL DEFAULT false,
    "HasGarden" boolean NOT NULL DEFAULT false,
    "HasPool" boolean NOT NULL DEFAULT false,
    "HasGym" boolean NOT NULL DEFAULT false,
    "HasSecurity" boolean NOT NULL DEFAULT false,
    "HasAirConditioning" boolean NOT NULL DEFAULT false,
    "HasHeating" boolean NOT NULL DEFAULT false,
    "HasInternet" boolean NOT NULL DEFAULT false,
    "IsFurnished" boolean NOT NULL DEFAULT false,
    "PetsAllowed" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_Prop_PropertyFeatures_Prop_Properties_PropertyId" FOREIGN KEY ("PropertyId") REFERENCES propertydb."Prop_Properties"("Id") ON DELETE CASCADE
);

-- Create property images table
CREATE TABLE IF NOT EXISTS propertydb."Prop_PropertyImages" (
    "Id" SERIAL PRIMARY KEY,
    "PropertyId" integer NOT NULL,
    "ImageUrl" text NOT NULL,
    "IsPrimary" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_Prop_PropertyImages_Prop_Properties_PropertyId" FOREIGN KEY ("PropertyId") REFERENCES propertydb."Prop_Properties"("Id") ON DELETE CASCADE
);

-- Create property amenities table
CREATE TABLE IF NOT EXISTS propertydb."Prop_PropertyAmenities" (
    "Id" SERIAL PRIMARY KEY,
    "PropertyId" integer NOT NULL,
    "Name" text NOT NULL,
    "Description" text,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_Prop_PropertyAmenities_Prop_Properties_PropertyId" FOREIGN KEY ("PropertyId") REFERENCES propertydb."Prop_Properties"("Id") ON DELETE CASCADE
);

-- Create property maintenance records table
CREATE TABLE IF NOT EXISTS propertydb."Prop_MaintenanceRecords" (
    "Id" SERIAL PRIMARY KEY,
    "PropertyId" integer NOT NULL,
    "Description" text NOT NULL,
    "MaintenanceDate" timestamp with time zone NOT NULL,
    "Cost" numeric NOT NULL,
    "Status" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_Prop_MaintenanceRecords_Prop_Properties_PropertyId" FOREIGN KEY ("PropertyId") REFERENCES propertydb."Prop_Properties"("Id") ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_Prop_PropertyFeatures_PropertyId" ON propertydb."Prop_PropertyFeatures"("PropertyId");
CREATE INDEX IF NOT EXISTS "IX_Prop_PropertyImages_PropertyId" ON propertydb."Prop_PropertyImages"("PropertyId");
CREATE INDEX IF NOT EXISTS "IX_Prop_PropertyAmenities_PropertyId" ON propertydb."Prop_PropertyAmenities"("PropertyId");
CREATE INDEX IF NOT EXISTS "IX_Prop_MaintenanceRecords_PropertyId" ON propertydb."Prop_MaintenanceRecords"("PropertyId");
CREATE INDEX IF NOT EXISTS "IX_Prop_Properties_Type" ON propertydb."Prop_Properties"("Type");
CREATE INDEX IF NOT EXISTS "IX_Prop_Properties_Status" ON propertydb."Prop_Properties"("Status");
CREATE INDEX IF NOT EXISTS "IX_Prop_Properties_City" ON propertydb."Prop_Properties"("City");

-- Insert migration record
INSERT INTO propertydb."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250618091035_InitialCreate', '8.0.0')
ON CONFLICT DO NOTHING; 
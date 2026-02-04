using System;
using System.Collections.Generic;

namespace SharedKernel.Dto
{
    public class PropertyCertificationDto
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string Name { get; set; }
        public string IssuingAuthority { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string DocumentUrl { get; set; }
    }

    public class PropertyComplianceDto
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string RegulationName { get; set; }
        public string ComplianceStatus { get; set; }
        public DateTime LastInspectionDate { get; set; }
        public DateTime NextInspectionDate { get; set; }
        public string InspectionNotes { get; set; }
        public string Violations { get; set; }
        public string CorrectiveActions { get; set; }
    }

    public class PropertyRegulationDto
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Jurisdiction { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string ComplianceRequirements { get; set; }
        public string Penalties { get; set; }
        public string DocumentationRequirements { get; set; }
    }

    public class PropertyStandardDto
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Version { get; set; }
        public DateTime ImplementationDate { get; set; }
        public string Requirements { get; set; }
        public string ComplianceStatus { get; set; }
    }

    public class PropertyFeatureDto
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsAvailable { get; set; }
        public string Specifications { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
        public string MaintenanceStatus { get; set; }
        public List<string> Images { get; set; }
    }

    public class PropertyServiceDto
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
        public string Provider { get; set; }
        public string ContactInformation { get; set; }
        public string ServiceLevel { get; set; }
        public string OperatingHours { get; set; }
        public decimal Cost { get; set; }
        public string BillingFrequency { get; set; }
    }
} 
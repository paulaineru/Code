using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PropertyManagementService.Repository;
using SharedKernel.Dto;
using System;
using System.Threading.Tasks;

namespace PropertyManagementService.Services
{
    public class PropertyFeaturesService : IPropertyFeaturesService
    {
        private readonly PropertyDbContext _context;
        private readonly ILogger<PropertyFeaturesService> _logger;

        public PropertyFeaturesService(
            PropertyDbContext context,
            ILogger<PropertyFeaturesService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PropertyCertificationDto> GetPropertyCertificationsAsync(Guid propertyId)
        {
            var certification = await _context.PropertyCertifications
                .FirstOrDefaultAsync(c => c.PropertyId == propertyId);

            if (certification == null)
            {
                throw new KeyNotFoundException($"Property certification for property ID {propertyId} not found");
            }

            return new PropertyCertificationDto
            {
                Id = certification.Id,
                PropertyId = propertyId,
                Name = certification.Name,
                IssuingAuthority = certification.IssuingAuthority,
                IssueDate = certification.IssueDate,
                ExpiryDate = certification.ExpiryDate,
                Status = certification.Status,
                Description = certification.Description,
                DocumentUrl = certification.DocumentUrl
            };
        }

        public async Task<PropertyComplianceDto> GetPropertyComplianceAsync(Guid propertyId)
        {
            var compliance = await _context.PropertyCompliance
                .FirstOrDefaultAsync(c => c.PropertyId == propertyId);

            if (compliance == null)
            {
                throw new KeyNotFoundException($"Property compliance for property ID {propertyId} not found");
            }

            return new PropertyComplianceDto
            {
                Id = compliance.Id,
                PropertyId = propertyId,
                RegulationName = compliance.RegulationName,
                ComplianceStatus = compliance.ComplianceStatus,
                LastInspectionDate = compliance.LastInspectionDate,
                NextInspectionDate = compliance.NextInspectionDate,
                InspectionNotes = compliance.InspectionNotes,
                Violations = compliance.Violations,
                CorrectiveActions = compliance.CorrectiveActions
            };
        }

        public async Task<PropertyRegulationDto> GetPropertyRegulationsAsync(Guid propertyId)
        {
            var regulation = await _context.PropertyRegulations
                .FirstOrDefaultAsync(r => r.PropertyId == propertyId);

            if (regulation == null)
            {
                throw new KeyNotFoundException($"Property regulation for property ID {propertyId} not found");
            }

            return new PropertyRegulationDto
            {
                Id = regulation.Id,
                PropertyId = propertyId,
                Name = regulation.Name,
                Description = regulation.Description,
                Jurisdiction = regulation.Jurisdiction,
                EffectiveDate = regulation.EffectiveDate,
                ComplianceRequirements = regulation.ComplianceRequirements,
                Penalties = regulation.Penalties,
                DocumentationRequirements = regulation.DocumentationRequirements
            };
        }

        public async Task<PropertyStandardDto> GetPropertyStandardsAsync(Guid propertyId)
        {
            var standard = await _context.PropertyStandards
                .FirstOrDefaultAsync(s => s.PropertyId == propertyId);

            if (standard == null)
            {
                throw new KeyNotFoundException($"Property standard for property ID {propertyId} not found");
            }

            return new PropertyStandardDto
            {
                Id = standard.Id,
                PropertyId = propertyId,
                Name = standard.Name,
                Description = standard.Description,
                Category = standard.Category,
                // Version = standard.Version,
                // ImplementationDate = standard.ImplementationDate,
                // ComplianceStatus = standard.ComplianceStatus,
                Requirements = standard.Requirements
            };
        }

        public async Task<PropertyFeatureDto> GetPropertyFeaturesAsync(Guid propertyId)
        {
            var feature = await _context.PropertyFeatures
                .FirstOrDefaultAsync(f => f.PropertyId == propertyId);

            if (feature == null)
            {
                throw new KeyNotFoundException($"Property feature for property ID {propertyId} not found");
            }

            return new PropertyFeatureDto
            {
                Id = feature.Id,
                PropertyId = propertyId,
                Name = feature.Name,
                Description = feature.Description,
                Category = feature.Category,
                // IsAvailable = feature.IsAvailable,
                // Specifications = feature.Specifications,
                // LastMaintenanceDate = feature.LastMaintenanceDate,
                // MaintenanceStatus = feature.MaintenanceStatus,
                // Images = feature.Images
            };
        }

        public async Task<PropertyServiceDto> GetPropertyServicesAsync(Guid propertyId)
        {
            var service = await _context.PropertyServices
                .FirstOrDefaultAsync(s => s.PropertyId == propertyId);

            if (service == null)
            {
                throw new KeyNotFoundException($"Property service for property ID {propertyId} not found");
            }

            return new PropertyServiceDto
            {
                Id = service.Id,
                PropertyId = propertyId,
                // Name = service.Name,
                // Description = service.Description,
                // Category = service.Category,
                // IsActive = service.IsActive,
                Provider = service.Provider,
                // ContactInformation = service.ContactInformation,
                // ServiceLevel = service.ServiceLevel,
                // OperatingHours = service.OperatingHours,
                Cost = service.Cost,
                // BillingFrequency = service.BillingFrequency
            };
        }
    }
} 
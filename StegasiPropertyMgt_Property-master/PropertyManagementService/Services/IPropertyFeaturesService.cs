using SharedKernel.Dto;
using System;
using System.Threading.Tasks;

namespace PropertyManagementService.Services
{
    public interface IPropertyFeaturesService
    {
        Task<PropertyCertificationDto> GetPropertyCertificationsAsync(Guid propertyId);
        Task<PropertyComplianceDto> GetPropertyComplianceAsync(Guid propertyId);
        Task<PropertyRegulationDto> GetPropertyRegulationsAsync(Guid propertyId);
        Task<PropertyStandardDto> GetPropertyStandardsAsync(Guid propertyId);
        Task<PropertyFeatureDto> GetPropertyFeaturesAsync(Guid propertyId);
        Task<PropertyServiceDto> GetPropertyServicesAsync(Guid propertyId);
    }
} 
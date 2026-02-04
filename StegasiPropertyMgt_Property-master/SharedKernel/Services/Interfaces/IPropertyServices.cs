// SharedKernel/Services/IPropertyService.cs
using SharedKernel.Models;
using SharedKernel.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedKernel.Services
{
    public interface IPropertyService
    {
        Task<Property> CreatePropertyAsync(CreatePropertyDto dto, string userId);
        Task<Property> GetPropertyByIdAsync(Guid id,string token =null);
        Task<PropertyDetailResponse> GetPropertyDetailByIdAsync(Guid id,string token =null);
        Task UpdatePropertyAsync(Guid id, CreatePropertyDto dto, string userId);
        Task DeletePropertyAsync(Guid id, string userId);
        Task<List<Property>> GetPropertiesByTypeAsync(string propertyType); // New method
        Task<List<Property>> GetAllPropertiesAsync(); 
        Task<List<Property>> GetPropertiesByFilterAsync(string status, string type);
    }
}
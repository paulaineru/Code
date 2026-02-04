// PropertyManagementService/Data/IPropertyRepository.cs
using SharedKernel.Models;
namespace PropertyManagementService.Repository
{
    public interface IPropertyRepository
    {
        Task AddAsync(Property property);
        Task<Property> GetByIdAsync(Guid id);
        Task UpdateAsync(Property property);
        Task DeleteAsync(Property property);
        Task<List<Property>> GetAvailablePropertiesAsync(); // For listing rentable/saleable properties
        Task<List<Property>> GetAllAsync(); // For retrieving all properties
    }
}
using SharedKernel.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TenantManagementService.Services.Interfaces
{
    public interface IPropertyService
    {
        Task<Property> GetPropertyByIdAsync(Guid id);
        Task<List<Property>> GetAvailablePropertiesAsync();
    }
}
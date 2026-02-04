using System;
using System.Threading.Tasks;
using SharedKernel.Models;

namespace BillingService.Services
{
    public interface IPropertyClient
    {
        Task<Property> GetPropertyAsync(Guid propertyId);
        Task<bool> ValidatePropertyAsync(Guid propertyId);
        Task<Property> GetPropertyByIdAsync(Guid propertyId);
    }
}

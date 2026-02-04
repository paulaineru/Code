// TenantManagementService/Data/ITenantRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedKernel.Models;

namespace SharedKernel.Services
{
    public interface ITenantRepository
    {
        Task AddAsync(Tenant tenant);
        Task<Tenant> GetByIdAsync(Guid id);
        Task UpdateAsync(Tenant tenant);
        Task DeleteAsync(Tenant tenant);
        Task<List<Tenant>> GetAllAsync();
    }
}
// TenantManagementService/Data/ITerminationProcessRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedKernel.Models.Tenants;

namespace SharedKernel.Services
{
    public interface ITerminationProcessRepository
    {
        Task AddAsync(TerminationProcess process);
        Task<TerminationProcess> GetByIdAsync(Guid id);
        Task UpdateAsync(TerminationProcess process);
        Task DeleteAsync(TerminationProcess process);
        Task<List<TerminationProcess>> GetByTenantIdAsync(Guid tenantId);
    }
}
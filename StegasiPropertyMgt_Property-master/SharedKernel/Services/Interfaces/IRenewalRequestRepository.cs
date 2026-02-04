using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedKernel.Models.Tenants;

namespace SharedKernel.Services
{
    public interface IRenewalRequestRepository
    {
        Task AddAsync(RenewalRequest request);
        Task<RenewalRequest> GetByIdAsync(Guid id);
        Task UpdateAsync(RenewalRequest request);
        Task DeleteAsync(RenewalRequest request);
        Task<List<RenewalRequest>> GetByTenantIdAsync(Guid tenantId);
    }
}
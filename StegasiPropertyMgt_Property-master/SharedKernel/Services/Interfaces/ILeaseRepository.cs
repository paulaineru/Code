using SharedKernel.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedKernel.Services
{
    public interface ILeaseRepository
    {
        Task AddLeaseAsync(LeaseAgreement lease);
        Task<List<LeaseAgreement>> GetLeasesByTenantAsync(Guid tenantId);
        Task<LeaseAgreement> GetLeaseByIdAsync(Guid leaseId);
        Task UpdateLeaseAsync(LeaseAgreement lease);
    }
}
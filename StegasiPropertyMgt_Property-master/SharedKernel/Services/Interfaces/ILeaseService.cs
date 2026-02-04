using SharedKernel.Dto;
using SharedKernel.Models;
using System.Threading.Tasks;

namespace SharedKernel.Services
{
    public interface ILeaseService
    {
        Task<LeaseResponse> CreateLeaseAsync(CreateLeaseRequest request);
        Task<List<LeaseResponse>> GetLeasesByTenantAsync(Guid tenantId);
        Task UpdateLeaseStatusAsync(Guid leaseId, string newStatus);
        Task ApproveLeaseAsync(Guid leaseId, ApproveLeaseRequest request);
        Task<LeaseResponse> GetLeaseByIdAsync(Guid id);
    }
}
using SharedKernel.Dto;
using SharedKernel.Models;
using SharedKernel.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TenantManagementService.Services
{
    public class LeaseService : ILeaseService
    {
        private readonly ILeaseRepository _repository;
        private readonly ILogger<LeaseService> _logger;

        public LeaseService(ILeaseRepository repository, ILogger<LeaseService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<LeaseResponse> CreateLeaseAsync(CreateLeaseRequest request)
        {
            try
            {
                var lease = new LeaseAgreement
                {
                    PropertyId = request.PropertyId,
                    TenantId = request.TenantId,
                    StartDate = EnsureUtc(request.StartDate),
                    EndDate = EnsureUtc(request.EndDate),
                    Terms = request.Terms,
                    Status = "Active" // Set initial status
                };

                await _repository.AddLeaseAsync(lease);

                return new LeaseResponse
                {
                    Id = lease.Id,
                    PropertyId = lease.PropertyId,
                    TenantId = lease.TenantId,
                    StartDate = lease.StartDate,
                    EndDate = lease.EndDate,
                    Terms = lease.Terms,
                    Status = lease.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create lease");
                throw new InvalidOperationException("Lease creation failed", ex);
            }
        }

        public async Task<List<LeaseResponse>> GetLeasesByTenantAsync(Guid tenantId)
        {
            var leases = await _repository.GetLeasesByTenantAsync(tenantId);

            return leases.Select(l => new LeaseResponse
            {
                Id = l.Id,
                PropertyId = l.PropertyId,
                TenantId = l.TenantId,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Terms = l.Terms,
                Status = l.Status
            }).ToList();
        }

        public async Task UpdateLeaseStatusAsync(Guid leaseId, string newStatus)
        {
            var lease = await _repository.GetLeaseByIdAsync(leaseId);
            if (lease == null)
                throw new KeyNotFoundException($"Lease with ID {leaseId} not found.");

            lease.Status = newStatus;
            await _repository.UpdateLeaseAsync(lease);
        }
        public async Task ApproveLeaseAsync(Guid leaseId, ApproveLeaseRequest request)
        {
            try
            {
                var lease = await _repository.GetLeaseByIdAsync(leaseId);
                if (lease == null)
                    throw new KeyNotFoundException($"Lease with ID {leaseId} not found.");

                // Update lease status
                lease.Status = request.ApprovalStatus;

                // Optionally log the approver ID
                if (request.ApproverId.HasValue)
                {
                    lease.ApproverId = request.ApproverId.Value;
                }

                // Save changes
                await _repository.UpdateLeaseAsync(lease);

                // Notify tenant (optional)
                await NotifyTenantOfLeaseApprovalAsync(lease.TenantId, request.ApprovalStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to approve lease {LeaseId}", leaseId);
                throw new InvalidOperationException("Lease approval failed", ex);
            }
        }
        public async Task<LeaseResponse> GetLeaseByIdAsync(Guid id)
        {
            var lease = await _repository.GetLeaseByIdAsync(id);
            if (lease == null)
                return null;

            return new LeaseResponse
            {
                Id = lease.Id,
                PropertyId = lease.PropertyId,
                TenantId = lease.TenantId,
                StartDate = lease.StartDate,
                EndDate = lease.EndDate,
                Terms = lease.Terms,
                Status = lease.Status,
                ApproverId = lease.ApproverId
            };
        }
        private async Task NotifyTenantOfLeaseApprovalAsync(Guid tenantId, string approvalStatus)
        {
            /*try
            {
                await _notificationClient.NotifyTenantAsync(new NotificationRequest
                {
                    TenantId = tenantId,
                    Message = $"Your lease has been {approvalStatus.ToLower()}."
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify tenant {TenantId} about lease approval", tenantId);
            }*/
        }

        private DateTime EnsureUtc(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

            return dateTime.ToUniversalTime();
        }
    }
}
using SharedKernel.Models;
using SharedKernel.Services;
using TenantManagementService.Repository;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace TenantManagementService.Repository
{
    public class LeaseRepository : ILeaseRepository
    {
        private readonly TenantDbContext _context;

        public LeaseRepository(TenantDbContext context)
        {
            _context = context;
        }

        public async Task AddLeaseAsync(LeaseAgreement lease)
        {
            await _context.LeaseAgreements.AddAsync(lease);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LeaseAgreement>> GetLeasesByTenantAsync(Guid tenantId)
        {
            return await _context.LeaseAgreements
                .Where(l => l.TenantId == tenantId)
                .ToListAsync();
        }

        public async Task<LeaseAgreement> GetLeaseByIdAsync(Guid leaseId)
        {
            return await _context.LeaseAgreements.FindAsync(leaseId);
        }

        public async Task UpdateLeaseAsync(LeaseAgreement lease)
        {
            _context.LeaseAgreements.Update(lease);
            await _context.SaveChangesAsync();
        }
    }
}
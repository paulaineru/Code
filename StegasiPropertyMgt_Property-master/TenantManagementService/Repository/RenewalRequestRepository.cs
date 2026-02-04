// TenantManagementService/Data/RenewalRequestRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Models.Tenants;
using SharedKernel.Services;

namespace TenantManagementService.Repository
{
    public class RenewalRequestRepository : IRenewalRequestRepository
    {
        private readonly TenantDbContext _context;

        public RenewalRequestRepository(TenantDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RenewalRequest request)
        {
            await _context.RenewalRequests.AddAsync(request);
            await _context.SaveChangesAsync();
        }

        public async Task<RenewalRequest> GetByIdAsync(Guid id)
        {
            return await _context.RenewalRequests.FindAsync(id);
        }

        public async Task UpdateAsync(RenewalRequest request)
        {
            _context.RenewalRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(RenewalRequest request)
        {
            _context.RenewalRequests.Remove(request);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RenewalRequest>> GetByTenantIdAsync(Guid tenantId)
        {
            return await _context.RenewalRequests.Where(r => r.TenantId == tenantId).ToListAsync();
        }
    }
}
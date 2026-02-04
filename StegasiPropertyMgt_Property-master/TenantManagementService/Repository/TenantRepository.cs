// TenantManagementService/Data/TenantRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Models;
using SharedKernel.Models.Tenants;
using SharedKernel.Services;


namespace TenantManagementService.Repository
{
    public class TenantRepository : ITenantRepository
    {
        private readonly TenantDbContext _context;

        public TenantRepository(TenantDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Tenant tenant)
        {
            await _context.Tenants.AddAsync(tenant);
            await _context.SaveChangesAsync();
        }

        public async Task<Tenant> GetByIdAsync(Guid id)
        {
            return await _context.Tenants.FindAsync(id);
        }

        public async Task UpdateAsync(Tenant tenant)
        {
            _context.Tenants.Update(tenant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Tenant tenant)
        {
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Tenant>> GetAllAsync()
        {
            return await _context.Tenants.ToListAsync();
        }
        // TenantManagementService/Data/TenantRepository.cs
        public async Task<List<Booking>> GetBookingsByTenantIdAsync(Guid tenantId)
        {
            return await _context.Bookings.Where(b => b.TenantId == tenantId).ToListAsync() ?? new List<Booking>();
        }

        public async Task<List<RenewalRequest>> GetRenewalRequestsByTenantIdAsync(Guid tenantId)
        {
            return await _context.RenewalRequests.Where(r => r.TenantId == tenantId).ToListAsync() ?? new List<RenewalRequest>();
        }

        public async Task<List<TerminationProcess>> GetTerminationProcessesByTenantIdAsync(Guid tenantId)
        {
            return await _context.TerminationProcesses.Where(t => t.TenantId == tenantId).ToListAsync() ?? new List<TerminationProcess>();
        }
    }
}
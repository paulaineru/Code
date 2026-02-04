using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Models.Tenants;
using SharedKernel.Services;

namespace TenantManagementService.Repository
{
    public class TerminationProcessRepository : ITerminationProcessRepository
    {
        private readonly TenantDbContext _context;

        public TerminationProcessRepository(TenantDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TerminationProcess process)
        {
            await _context.TerminationProcesses.AddAsync(process);
            await _context.SaveChangesAsync();
        }

        public async Task<TerminationProcess> GetByIdAsync(Guid id)
        {
            return await _context.TerminationProcesses.FindAsync(id);
        }

        public async Task UpdateAsync(TerminationProcess process)
        {
            _context.TerminationProcesses.Update(process);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TerminationProcess process)
        {
            _context.TerminationProcesses.Remove(process);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TerminationProcess>> GetByTenantIdAsync(Guid tenantId)
        {
            return await _context.TerminationProcesses.Where(t => t.TenantId == tenantId).ToListAsync();
        }
    }
}
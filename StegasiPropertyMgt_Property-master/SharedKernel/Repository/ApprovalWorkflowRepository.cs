using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Data;
using SharedKernel.Models;

namespace SharedKernel.Repository
{
    public class ApprovalWorkflowRepository : IApprovalWorkflowRepository
    {
        private readonly ApprovalWorkflowDbContext _context;

        public ApprovalWorkflowRepository(ApprovalWorkflowDbContext context)
        {
            _context = context;
        }

        public async Task<ApprovalWorkflow> CreateAsync(ApprovalWorkflow workflow)
        {
            await _context.ApprovalWorkflows.AddAsync(workflow);
            await _context.SaveChangesAsync();
            return workflow;
        }

        public async Task<ApprovalWorkflow> GetByIdAsync(Guid id)
        {
            return await _context.ApprovalWorkflows
                .Include(w => w.Stages)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<ApprovalWorkflow> GetByEntityAsync(string module, Guid entityId)
        {
            return await _context.ApprovalWorkflows
                .Include(w => w.Stages)
                .FirstOrDefaultAsync(w => w.Module == module && w.EntityId == entityId);
        }

        public async Task<List<ApprovalWorkflow>> GetPendingWorkflowsAsync(string module, string role)
        {
            return await _context.ApprovalWorkflows
                .Include(w => w.Stages)
                .Where(w => w.Module == module && 
                           w.Status == ApprovalStatus.Pending &&
                           w.Stages.Any(s => s.Role == role && s.Status == ApprovalStatus.Pending))
                .ToListAsync();
        }

        public async Task<List<ApprovalWorkflow>> GetWorkflowsByStatusAsync(string module, ApprovalStatus status)
        {
            return await _context.ApprovalWorkflows
                .Include(w => w.Stages)
                .Where(w => w.Module == module && w.Status == status)
                .ToListAsync();
        }

        public async Task UpdateAsync(ApprovalWorkflow workflow)
        {
            _context.ApprovalWorkflows.Update(workflow);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var workflow = await GetByIdAsync(id);
            if (workflow != null)
            {
                _context.ApprovalWorkflows.Remove(workflow);
                await _context.SaveChangesAsync();
            }
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedKernel.Models;

namespace SharedKernel.Repository
{
    public interface IApprovalWorkflowRepository
    {
        Task<ApprovalWorkflow> CreateAsync(ApprovalWorkflow workflow);
        Task<ApprovalWorkflow> GetByIdAsync(Guid id);
        Task<ApprovalWorkflow> GetByEntityAsync(string module, Guid entityId);
        Task<List<ApprovalWorkflow>> GetPendingWorkflowsAsync(string module, string role);
        Task<List<ApprovalWorkflow>> GetWorkflowsByStatusAsync(string module, ApprovalStatus status);
        Task UpdateAsync(ApprovalWorkflow workflow);
        Task DeleteAsync(Guid id);
    }
} 
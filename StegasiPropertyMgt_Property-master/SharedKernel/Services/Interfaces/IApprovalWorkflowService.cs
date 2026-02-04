using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedKernel.Models;

namespace SharedKernel.Services
{
    public interface IApprovalWorkflowService
    {
        Task<ApprovalWorkflow> CreateWorkflowAsync(string module, Guid entityId, string entityType, Guid createdBy);
        Task<ApprovalWorkflow> GetWorkflowAsync(Guid workflowId);
        Task<ApprovalWorkflow> GetWorkflowByEntityAsync(string module, Guid entityId);
        Task<ApprovalWorkflow> UpdateWorkflowStatusAsync(Guid workflowId, ApprovalStatus newStatus, Guid updatedBy, string? comments = null);
        Task<ApprovalStage> ApproveStageAsync(Guid workflowId, int stageNumber, Guid approverId, string? comments = null, string? userRole = null);
        Task<ApprovalStage> RejectStageAsync(Guid workflowId, int stageNumber, Guid rejectorId, string? comments = null, string? userRole = null);
        Task<ApprovalStage> RequestMoreInfoAsync(Guid workflowId, int stageNumber, Guid requesterId, string comments, string? userRole = null);
        Task<List<ApprovalWorkflow>> GetPendingWorkflowsAsync(string module, string role);
        Task<List<ApprovalWorkflow>> GetWorkflowsByStatusAsync(string module, ApprovalStatus status);
        Task<bool> CanApproveStageAsync(Guid workflowId, int stageNumber, Guid userId, string userRole = null);
        Task<ApprovalStage> GetCurrentStageAsync(Guid workflowId);
        Task<bool> IsWorkflowCompleteAsync(Guid workflowId);
        Task FixWorkflowRoleNamesAsync();
    }
} 
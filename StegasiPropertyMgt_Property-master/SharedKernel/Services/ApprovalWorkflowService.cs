using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharedKernel.Models;
using SharedKernel.Repository;
using SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Data;

namespace SharedKernel.Services
{
    public class ApprovalWorkflowService : IApprovalWorkflowService
    {
        private readonly ILogger<ApprovalWorkflowService> _logger;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private readonly IApprovalWorkflowRepository _repository;
        private readonly ApprovalWorkflowDbContext _context;

        public ApprovalWorkflowService(
            ILogger<ApprovalWorkflowService> logger,
            IUserService userService,
            INotificationService notificationService,
            IAuditLogService auditLogService,
            IApprovalWorkflowRepository repository,
            ApprovalWorkflowDbContext context)
        {
            _logger = logger;
            _userService = userService;
            _notificationService = notificationService;
            _auditLogService = auditLogService;
            _repository = repository;
            _context = context;
        }

        public async Task<ApprovalWorkflow> CreateWorkflowAsync(string module, Guid entityId, string entityType, Guid createdBy)
        {
            if (!ApprovalConfigurations.ModuleWorkflows.ContainsKey(module))
                throw new ArgumentException($"No workflow configuration found for module: {module}");

            var workflow = new ApprovalWorkflow
            {
                Module = module,
                EntityId = entityId,
                EntityType = entityType,
                CreatedBy = createdBy,
                Status = ApprovalStatus.Pending,
                Stages = ApprovalConfigurations.ModuleWorkflows[module]
                    .Select(s => new ApprovalStage
                    {
                        StageNumber = s.StageNumber,
                        Role = s.Role,
                        Order = s.Order,
                        IsRequired = s.IsRequired,
                        Status = ApprovalStatus.Pending
                    }).ToList()
            };

            workflow = await _repository.CreateAsync(workflow);

            // Log the creation
            await _auditLogService.RecordActionAsync(
                "WorkflowCreated",
                null,
                null,
                $"Created approval workflow for {module} {entityType} {entityId}",
                createdBy
            );

            return workflow;
        }

        public async Task<ApprovalWorkflow> GetWorkflowAsync(Guid workflowId)
        {
            var workflow = await _repository.GetByIdAsync(workflowId);
            if (workflow == null)
                throw new KeyNotFoundException($"Workflow with ID {workflowId} not found");
            return workflow;
        }

        public async Task<ApprovalWorkflow> GetWorkflowByEntityAsync(string module, Guid entityId)
        {
            var workflow = await _repository.GetByEntityAsync(module, entityId);
            if (workflow == null)
                throw new KeyNotFoundException($"No workflow found for {module} entity {entityId}");
            return workflow;
        }

        public async Task<ApprovalWorkflow> UpdateWorkflowStatusAsync(Guid workflowId, ApprovalStatus newStatus, Guid updatedBy, string? comments = null)
        {
            var workflow = await GetWorkflowAsync(workflowId);
            workflow.Status = newStatus;
            workflow.LastUpdatedAt = DateTime.UtcNow;
            workflow.Comments = comments;

            await _repository.UpdateAsync(workflow);

            // Log the status update
            await _auditLogService.RecordActionAsync(
                "WorkflowStatusUpdated",
                null,
                null,
                $"Updated workflow {workflowId} status to {newStatus}",
                updatedBy
            );

            return workflow;
        }

        public async Task<ApprovalStage> ApproveStageAsync(Guid workflowId, int stageNumber, Guid approverId, string? comments = null, string? userRole = null)
        {
            var workflow = await GetWorkflowAsync(workflowId);
            var stage = workflow.Stages.FirstOrDefault(s => s.StageNumber == stageNumber);
            
            if (stage == null)
                throw new ArgumentException($"Stage {stageNumber} not found in workflow {workflowId}");

            if (!await CanApproveStageAsync(workflowId, stageNumber, approverId, userRole))
                throw new UnauthorizedAccessException($"User {approverId} cannot approve stage {stageNumber}");

            stage.Status = ApprovalStatus.Approved;
            stage.ApprovedAt = DateTime.UtcNow;
            stage.ApprovedBy = approverId;
            stage.Comments = comments;

            // Check if workflow is complete
            if (await IsWorkflowCompleteAsync(workflowId))
            {
                await UpdateWorkflowStatusAsync(workflowId, ApprovalStatus.Approved, approverId);
            }

            await _repository.UpdateAsync(workflow);

            // Notify next approver if any
            var nextStage = workflow.Stages.FirstOrDefault(s => s.Order > stage.Order && s.Status == ApprovalStatus.Pending);
            if (nextStage != null)
            {
                await NotifyNextApproverAsync(workflow, nextStage);
            }

            // Log the approval
            await _auditLogService.RecordActionAsync(
                "StageApproved",
                null,
                null,
                $"Approved stage {stageNumber} in workflow {workflowId}",
                approverId
            );

            return stage;
        }

        public async Task<ApprovalStage> RejectStageAsync(Guid workflowId, int stageNumber, Guid rejectorId, string? comments = null, string? userRole = null)
        {
            var workflow = await GetWorkflowAsync(workflowId);
            var stage = workflow.Stages.FirstOrDefault(s => s.StageNumber == stageNumber);
            
            if (stage == null)
                throw new ArgumentException($"Stage {stageNumber} not found in workflow {workflowId}");

            if (!await CanApproveStageAsync(workflowId, stageNumber, rejectorId, userRole))
                throw new UnauthorizedAccessException($"User {rejectorId} cannot reject stage {stageNumber}");

            stage.Status = ApprovalStatus.Rejected;
            stage.ApprovedAt = DateTime.UtcNow;
            stage.ApprovedBy = rejectorId;
            stage.Comments = comments;

            // Update workflow status
            await UpdateWorkflowStatusAsync(workflowId, ApprovalStatus.Rejected, rejectorId);

            await _repository.UpdateAsync(workflow);

            // Notify creator
            await NotifyWorkflowCreatorAsync(workflow, "rejected");

            // Log the rejection
            await _auditLogService.RecordActionAsync(
                "StageRejected",
                null,
                null,
                $"Rejected stage {stageNumber} in workflow {workflowId}",
                rejectorId
            );

            return stage;
        }

        public async Task<ApprovalStage> RequestMoreInfoAsync(Guid workflowId, int stageNumber, Guid requesterId, string comments, string? userRole = null)
        {
            var workflow = await GetWorkflowAsync(workflowId);
            var stage = workflow.Stages.FirstOrDefault(s => s.StageNumber == stageNumber);
            
            if (stage == null)
                throw new ArgumentException($"Stage {stageNumber} not found in workflow {workflowId}");

            if (!await CanApproveStageAsync(workflowId, stageNumber, requesterId, userRole))
                throw new UnauthorizedAccessException($"User {requesterId} cannot request more info for stage {stageNumber}");

            stage.Status = ApprovalStatus.MoreInfoRequired;
            stage.Comments = comments;

            // Update workflow status
            await UpdateWorkflowStatusAsync(workflowId, ApprovalStatus.MoreInfoRequired, requesterId);

            await _repository.UpdateAsync(workflow);

            // Notify creator
            await NotifyWorkflowCreatorAsync(workflow, "requires more information");

            // Log the request
            await _auditLogService.RecordActionAsync(
                "MoreInfoRequested",
                null,
                null,
                $"Requested more info for stage {stageNumber} in workflow {workflowId}",
                requesterId
            );

            return stage;
        }

        public async Task<List<ApprovalWorkflow>> GetPendingWorkflowsAsync(string module, string role)
        {
            return await _repository.GetPendingWorkflowsAsync(module, role);
        }

        public async Task<List<ApprovalWorkflow>> GetWorkflowsByStatusAsync(string module, ApprovalStatus status)
        {
            return await _repository.GetWorkflowsByStatusAsync(module, status);
        }

        public async Task<bool> CanApproveStageAsync(Guid workflowId, int stageNumber, Guid userId, string userRole = null)
        {
            var workflow = await GetWorkflowAsync(workflowId);
            var stage = workflow.Stages.FirstOrDefault(s => s.StageNumber == stageNumber);
            
            if (stage == null)
            {
                _logger.LogWarning("Stage {StageNumber} not found in workflow {WorkflowId}", stageNumber, workflowId);
                return false;
            }

            _logger.LogInformation("Checking authorization - WorkflowId: {WorkflowId}, StageNumber: {StageNumber}, UserId: {UserId}, UserRole: {UserRole}, RequiredRole: {RequiredRole}", 
                workflowId, stageNumber, userId, userRole, stage.Role);

            // If userRole is provided (from JWT token), use it directly
            if (!string.IsNullOrEmpty(userRole))
            {
                var canApprove = userRole == stage.Role;
                _logger.LogInformation("Role comparison result: {UserRole} == {RequiredRole} = {CanApprove}", userRole, stage.Role, canApprove);
                return canApprove;
            }

            // Fallback: try to get user from AuthenticationService
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    var canApprove = user.Role == stage.Role;
                    _logger.LogInformation("Fallback role comparison result: {UserRole} == {RequiredRole} = {CanApprove}", user.Role, stage.Role, canApprove);
                    return canApprove;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to get user from AuthenticationService: {UserId}, Error: {Error}", userId, ex.Message);
            }

            // If user doesn't exist in AuthenticationService, we can't verify the role
            // This is a fallback - in a production system, you'd want to ensure user synchronization
            _logger.LogWarning("User {UserId} not found in AuthenticationService. Cannot verify role for stage approval.", userId);
            return false;
        }

        public async Task<ApprovalStage> GetCurrentStageAsync(Guid workflowId)
        {
            var workflow = await GetWorkflowAsync(workflowId);
            return workflow.Stages
                .OrderBy(s => s.Order)
                .FirstOrDefault(s => s.Status == ApprovalStatus.Pending);
        }

        public async Task<bool> IsWorkflowCompleteAsync(Guid workflowId)
        {
            var workflow = await GetWorkflowAsync(workflowId);
            return workflow.Stages.All(s => s.Status == ApprovalStatus.Approved);
        }

        private async Task NotifyNextApproverAsync(ApprovalWorkflow workflow, ApprovalStage nextStage)
        {
            try
            {
                var approvers = await _userService.GetUsersByRoleAsync(nextStage.Role);
                foreach (var approver in approvers)
                {
                    await _notificationService.SendNotificationAsync(
                        approver.Id,
                        "Approval Required",
                        $"A {workflow.Module} {workflow.EntityType} requires your approval.",
                        "ApprovalRequired"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify next approver for workflow {WorkflowId}", workflow.Id);
            }
        }

        private async Task NotifyWorkflowCreatorAsync(ApprovalWorkflow workflow, string status)
        {
            try
            {
                await _notificationService.SendNotificationAsync(
                    workflow.CreatedBy,
                    "Workflow Update",
                    $"Your {workflow.Module} {workflow.EntityType} has been {status}.",
                    "WorkflowUpdate"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify workflow creator {CreatorId}", workflow.CreatedBy);
            }
        }

        /// <summary>
        /// Fixes role names in existing workflow data to match the actual role names in the database
        /// </summary>
        public async Task FixWorkflowRoleNamesAsync()
        {
            try
            {
                var stagesToUpdate = await _context.ApprovalStages
                    .Where(s => s.Role == "EstatesOfficer")
                    .ToListAsync();

                if (stagesToUpdate.Any())
                {
                    _logger.LogInformation("Found {Count} stages with incorrect role name 'EstatesOfficer', updating to 'Estates Officer'", stagesToUpdate.Count);
                    
                    foreach (var stage in stagesToUpdate)
                    {
                        stage.Role = "Estates Officer";
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully updated {Count} stages with correct role names", stagesToUpdate.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing workflow role names");
            }
        }
    }
} 
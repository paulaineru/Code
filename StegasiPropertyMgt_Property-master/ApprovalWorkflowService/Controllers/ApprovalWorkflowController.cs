using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;
using SharedKernel.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace ApprovalWorkflowService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalWorkflowController : ControllerBase
{
    private readonly IApprovalWorkflowService _workflowService;
    private readonly ILogger<ApprovalWorkflowController> _logger;

    public ApprovalWorkflowController(IApprovalWorkflowService workflowService, ILogger<ApprovalWorkflowController> logger)
    {
        _workflowService = workflowService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkflow([FromBody] CreateWorkflowRequest request)
    {
        _logger.LogInformation("CreateWorkflow called");
        try
        {
            var result = await _workflowService.CreateWorkflowAsync(request.Module, request.EntityId, request.EntityType, request.CreatedBy);
            _logger.LogInformation("Workflow created successfully");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{workflowId}")]
    public async Task<IActionResult> GetWorkflow(Guid workflowId)
    {
        _logger.LogInformation("GetWorkflow called for {WorkflowId}", workflowId);
        try
        {
            var workflow = await _workflowService.GetWorkflowAsync(workflowId);
            if (workflow == null)
            {
                _logger.LogWarning("Workflow not found: {WorkflowId}", workflowId);
                return NotFound();
            }
            return Ok(workflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow {WorkflowId}", workflowId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("entity/{module}/{entityId}")]
    public async Task<IActionResult> GetWorkflowByEntity(string module, Guid entityId)
    {
        _logger.LogInformation("GetWorkflowByEntity called for {Module} {EntityId}", module, entityId);
        try
        {
            var workflow = await _workflowService.GetWorkflowByEntityAsync(module, entityId);
            if (workflow == null)
            {
                _logger.LogWarning("Workflow not found for entity: {Module} {EntityId}", module, entityId);
                return NotFound();
            }
            return Ok(workflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow by entity {Module} {EntityId}", module, entityId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{workflowId}/stages/{stageNumber}/approve")]
    public async Task<IActionResult> ApproveStage(Guid workflowId, int stageNumber, [FromBody] string comments)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("ApproveStage unauthorized access");
            return Unauthorized();
        }
        _logger.LogInformation("ApproveStage called by {UserId} for {WorkflowId} stage {StageNumber}", userId, workflowId, stageNumber);
        try
        {
            var result = await _workflowService.ApproveStageAsync(workflowId, stageNumber, Guid.Parse(userId), comments);
            _logger.LogInformation("Stage {StageNumber} of workflow {WorkflowId} approved by {UserId}", stageNumber, workflowId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving stage {StageNumber} of workflow {WorkflowId}", stageNumber, workflowId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{workflowId}/stages/{stageNumber}/reject")]
    public async Task<IActionResult> RejectStage(Guid workflowId, int stageNumber, [FromBody] string comments)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("RejectStage unauthorized access");
            return Unauthorized();
        }
        _logger.LogInformation("RejectStage called by {UserId} for {WorkflowId} stage {StageNumber}", userId, workflowId, stageNumber);
        try
        {
            var result = await _workflowService.RejectStageAsync(workflowId, stageNumber, Guid.Parse(userId), comments);
            _logger.LogInformation("Stage {StageNumber} of workflow {WorkflowId} rejected by {UserId}", stageNumber, workflowId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting stage {StageNumber} of workflow {WorkflowId}", stageNumber, workflowId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{workflowId}/stages/{stageNumber}/request-info")]
    public async Task<IActionResult> RequestMoreInfo(Guid workflowId, int stageNumber, [FromBody] string comments)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("RequestMoreInfo unauthorized access");
            return Unauthorized();
        }
        _logger.LogInformation("RequestMoreInfo called by {UserId} for {WorkflowId} stage {StageNumber}", userId, workflowId, stageNumber);
        try
        {
            var result = await _workflowService.RequestMoreInfoAsync(workflowId, stageNumber, Guid.Parse(userId), comments);
            _logger.LogInformation("More info requested for stage {StageNumber} of workflow {WorkflowId} by {UserId}", stageNumber, workflowId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting more info for stage {StageNumber} of workflow {WorkflowId}", stageNumber, workflowId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{workflowId}/current-stage")]
    public async Task<IActionResult> GetCurrentStage(Guid workflowId)
    {
        _logger.LogInformation("GetCurrentStage called for {WorkflowId}", workflowId);
        try
        {
            var stage = await _workflowService.GetCurrentStageAsync(workflowId);
            if (stage == null)
            {
                _logger.LogWarning("No current stage found for workflow: {WorkflowId}", workflowId);
                return NotFound();
            }
            return Ok(stage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current stage for workflow {WorkflowId}", workflowId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("pending/{module}")]
    public async Task<IActionResult> GetPendingWorkflows(string module)
    {
        _logger.LogInformation("GetPendingWorkflows called for module: {Module}", module);
        try
        {
            var workflows = await _workflowService.GetPendingWorkflowsAsync(module, null);
            return Ok(workflows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending workflows for module {Module}", module);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("fix-role-names")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> FixWorkflowRoleNames()
    {
        _logger.LogInformation("FixWorkflowRoleNames called");
        try
        {
            await _workflowService.FixWorkflowRoleNamesAsync();
            return Ok(new { message = "Workflow role names have been fixed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fixing workflow role names");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

public class CreateWorkflowRequest
{
    public string Module { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
} 
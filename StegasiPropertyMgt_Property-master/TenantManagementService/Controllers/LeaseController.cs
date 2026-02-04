using SharedKernel.Dto;
using SharedKernel.Services;
using Microsoft.AspNetCore.Mvc;
using TenantManagementService.Services;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class LeaseController : ControllerBase
{
    private readonly ILeaseService _leaseService;
    private readonly ILogger<LeaseController> _logger;
    public LeaseController(ILeaseService leaseService, ILogger<LeaseController> logger)
    {
        _leaseService = leaseService;
        _logger =logger;
    }

    // Endpoint: Create a new lease
    [HttpPost("create")]
    public async Task<IActionResult> CreateLeaseAsync([FromBody] CreateLeaseRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.BadRequest("Invalid lease request."));

        try
        {
            var lease = await _leaseService.CreateLeaseAsync(request);
            return CreatedAtAction(nameof(GetLeaseById), new { id = lease.Id }, ApiResponse<object>.Success(lease, "Lease created successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.NotFound(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.BadRequest(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.InternalServerError(ex.Message));
        }
    }

    // Endpoint: Fetch leases by tenant
    [HttpGet("tenant/{tenantId}")]
    public async Task<IActionResult> GetLeasesByTenantAsync(Guid tenantId)
    {
        try
        {
            var leases = await _leaseService.GetLeasesByTenantAsync(tenantId);
            if (leases == null || !leases.Any())
                return Ok(ApiResponse<IEnumerable<object>>.Success(new List<object>(), "No leases found for tenant."));

            return Ok(ApiResponse<IEnumerable<object>>.Success(leases, "Leases retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.InternalServerError(ex.Message));
        }
    }

    // Endpoint: Update lease status
    [HttpPut("{id}/update-status")]
    public async Task<IActionResult> UpdateLeaseStatusAsync(Guid id, [FromBody] UpdateLeaseStatusRequest request)
    {
        try
        {
            await _leaseService.UpdateLeaseStatusAsync(id, request.NewStatus);
            return Ok(ApiResponse<object>.Success(new { leaseId = id, newStatus = request.NewStatus }, "Lease status updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.InternalServerError(ex.Message));
        }
    }

    // Helper action for created leases
    [HttpGet("{id}")]
    public IActionResult GetLeaseById(Guid id)
    {
        return Ok(ApiResponse<object>.Success(new { message = $"Lease with ID {id}" }, "Lease retrieved successfully"));
    }
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveLeaseAsync(Guid id, [FromBody] ApproveLeaseRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.BadRequest("Invalid lease approval request."));

        try
        {
            await _leaseService.ApproveLeaseAsync(id, request);

            // Optionally return updated lease details
            var lease = await _leaseService.GetLeaseByIdAsync(id);
            if (lease == null)
                return NotFound(ApiResponse.NotFound("Lease not found"));

            return Ok(ApiResponse<object>.Success(lease, "Lease approved successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.NotFound(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.BadRequest(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve lease {LeaseId}", id);
            return StatusCode(500, ApiResponse.InternalServerError("An unexpected error occurred."));
        }
    }

    // Helper method to fetch lease details
    private async Task<LeaseResponse> GetLeaseByIdAsync(Guid id)
    {
        var lease = await _leaseService.GetLeaseByIdAsync(id);
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

}
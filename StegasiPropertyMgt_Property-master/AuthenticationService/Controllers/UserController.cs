// Authentication/Controllers/UserController.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AuthenticationService.Services;
using SharedKernel.Models;
using SharedKernel.Dto;
using SharedKernel.Services;
using SharedKernel.Utilities;
using AuthenticationService.Middleware;
using System.Collections.Generic;

namespace AuthenticationService.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, IAuditLogService auditLogService, ILogger<UserController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{id}/email")]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserEmailById([FromRoute] Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {id} not found.");
                var email = user.Email;
                // Record audit log
                await _auditLogService.RecordActionAsync(
                    action: "UserEmailFetched",
                    entityId: id,
                    userId: HttpContext.GetUserId(),
                    details: $"User email fetched for ID {id}",
                    moduleId: Guid.Empty // Set appropriate moduleId if available
                );
                return Ok(email);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found");

                // Record warning log in Elasticsearch
                await _auditLogService.RecordActionAsync(
                    action: "UserEmailFetchFailed",
                    entityId: id,
                    userId: HttpContext.GetUserId(),
                    details: $"Failed to fetch user email for ID {id}: {ex.Message}",
                    moduleId: Guid.Empty // Set appropriate moduleId if available
                );

                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user email");

                // Record error log in Elasticsearch
                await _auditLogService.RecordActionAsync(
                    action: "UserEmailFetchError",
                    entityId: id,
                    userId: HttpContext.GetUserId(),
                    details: $"Error fetching user email for ID {id}: {ex.Message}",
                    moduleId: Guid.Empty // Set appropriate moduleId if available
                );

                return StatusCode(500, new ErrorResponse { Message = "An error occurred while fetching the user email." });
            }
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new ErrorResponse { Message = "User not found" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user");
                return StatusCode(500, new ErrorResponse { Message = "An error occurred while fetching the user." });
            }
        }

        [HttpGet("by-username")]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserByUsername([FromQuery] string username)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(username);
                if (user == null)
                    return NotFound(new ErrorResponse { Message = $"User with email {username} not found." });
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return StatusCode(500, new ErrorResponse { Message = "An error occurred while deleting the user." });
            }
        }
    }
}
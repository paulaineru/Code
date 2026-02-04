using SharedKernel.Dto;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Exceptions;
using System.Security.Claims;

namespace AuthenticationService.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin,Property Manager")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;
        private readonly IRoleHierarchyService _roleHierarchyService;

        public RoleController(IRoleService roleService, ILogger<RoleController> logger, IRoleHierarchyService roleHierarchyService)
        {
            _roleService = roleService;
            _logger = logger;
            _roleHierarchyService = roleHierarchyService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RoleResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RoleResponseDto>> CreateRole([FromBody] CreateRoleDto dto)
        {
            try
            {
                var currentUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUser))
                {
                    return Unauthorized();
                }

                var role = await _roleService.CreateRoleAsync(dto, currentUser);
                return CreatedAtAction(nameof(GetRole), new { roleId = role.Id }, role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{roleId}")]
        [ProducesResponseType(typeof(RoleResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoleResponseDto>> UpdateRole(Guid roleId, [FromBody] UpdateRoleDto dto)
        {
            try
            {
                var currentUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUser))
                {
                    return Unauthorized();
                }

                var role = await _roleService.UpdateRoleAsync(roleId, dto, currentUser);
                return Ok(role);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {RoleId}", roleId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{roleId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(Guid roleId)
        {
            try
            {
                var currentUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUser))
                {
                    return Unauthorized();
                }

                await _roleService.DeleteRoleAsync(roleId, currentUser);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {RoleId}", roleId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{roleId}")]
        [ProducesResponseType(typeof(RoleResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoleResponseDto>> GetRole(Guid roleId)
        {
            try
            {
                var role = await _roleService.GetRoleAsync(roleId);
                return Ok(role);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role {RoleId}", roleId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<RoleResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<RoleResponseDto>>> GetAllRoles()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("permissions")]
        [ProducesResponseType(typeof(List<PermissionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<PermissionResponseDto>>> GetAllPermissions()
        {
            try
            {
                var permissions = await _roleService.GetAllPermissionsAsync();
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all permissions");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("assign")]
        [ProducesResponseType(typeof(UserRoleResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<ActionResult<UserRoleResponseDto>> AssignRole([FromBody] AssignRoleDto dto)
        {
            try
            {
                var currentUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUser))
                {
                    currentUser = "SYSTEM";
                }

                var userRole = await _roleService.AssignRoleToUserAsync(dto, currentUser);
                return Ok(userRole);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", dto.RoleId, dto.UserId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("users/{userId}/roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRoleFromUser(Guid userId, Guid roleId)
        {
            try
            {
                var currentUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUser))
                {
                    return Unauthorized();
                }

                await _roleService.RemoveRoleFromUserAsync(userId, roleId, currentUser);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role {RoleId} from user {UserId}", roleId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("users/{userId}/roles")]
        [ProducesResponseType(typeof(List<UserRoleResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<UserRoleResponseDto>>> GetUserRoles(Guid userId)
        {
            try
            {
                var userRoles = await _roleService.GetUserRolesAsync(userId);
                return Ok(userRoles);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("hierarchy")]
        public async Task<IActionResult> AddRoleHierarchy([FromBody] RoleHierarchyDto dto)
        {
            try
            {
                await _roleHierarchyService.AddHierarchyAsync(dto.ParentRoleId, dto.ChildRoleId, dto.HierarchyLevel);
                return Ok(new { message = "Hierarchy added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding role hierarchy");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("hierarchy")]
        public async Task<IActionResult> RemoveRoleHierarchy([FromBody] RoleHierarchyDto dto)
        {
            try
            {
                await _roleHierarchyService.RemoveHierarchyAsync(dto.ParentRoleId, dto.ChildRoleId);
                return Ok(new { message = "Hierarchy removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role hierarchy");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("hierarchy/parents/{roleId}")]
        public async Task<IActionResult> GetParentRoles(Guid roleId)
        {
            var parents = await _roleHierarchyService.GetAllParentRoleIdsAsync(roleId);
            return Ok(parents);
        }

        [HttpGet("hierarchy/children/{roleId}")]
        public async Task<IActionResult> GetChildRoles(Guid roleId)
        {
            var children = await _roleHierarchyService.GetAllChildRoleIdsAsync(roleId);
            return Ok(children);
        }

        /// <summary>
        /// Gets a list of all admin users in the system
        /// </summary>
        /// <returns>List of admin users with their details</returns>
        /// <response code="200">Successfully retrieved admin users</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        [HttpGet("admins")]
        [ProducesResponseType(typeof(List<AdminUserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<AdminUserResponseDto>>> GetAllAdminUsers()
        {
            try
            {
                var adminUsers = await _roleService.GetAllAdminUsersAsync();
                return Ok(adminUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin users");
                return StatusCode(500, new { message = "An error occurred while retrieving admin users" });
            }
        }
    }
} 
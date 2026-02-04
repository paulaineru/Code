// AuthenticationService/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;
using SharedKernel.Services;
using SharedKernel.Utilities;
using SharedKernel.Dto; // Add this line for RegisterUserDto and LoginUserDto
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AuthenticationService.Repository;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AuthenticationService.Configuration;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using SharedKernel.Exceptions;


namespace AuthenticationService.Controllers
{
    /// <summary>
    /// Controller for handling authentication and user management operations
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IRoleService _roleService;

        public AuthController(
            IAuthService authService, 
            ILogger<AuthController> logger,
            IRoleService roleService)
        {
            _authService = authService;
            _logger = logger;
            _roleService = roleService;
        }

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="dto">User registration information</param>
        /// <returns>Success message with user ID if registration is successful</returns>
        /// <response code="201">User successfully registered</response>
        /// <response code="400">Invalid registration data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            try
            {
                var user = await _authService.RegisterAsync(dto);
                var responseData = new { userId = user.Id, message = "User registered successfully" };
                return Ok(ApiResponse<object>.Success(responseData, "User registered successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration failed for username: {UserName}", dto.UserName);
                return BadRequest(ApiResponse.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred during registration"));
            }
        }

        /// <summary>
        /// Authenticates a user and returns JWT token
        /// </summary>
        /// <param name="dto">User login credentials</param>
        /// <returns>JWT token and user information if authentication is successful</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid credentials</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            try
            {
                var response = await _authService.LoginAsync(dto);
                return Ok(ApiResponse<LoginResponseDto>.Success(response, "Login successful"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Login failed for username: {UserName}", dto.UserName);
                return BadRequest(ApiResponse.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred during login"));
            }
        }

        /// <summary>
        /// Refreshes an expired JWT token
        /// </summary>
        /// <param name="dto">Refresh token information</param>
        /// <returns>New JWT token if refresh is successful</returns>
        /// <response code="200">Token refresh successful</response>
        /// <response code="400">Invalid refresh token</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(dto);
                return Ok(ApiResponse<LoginResponseDto>.Success(response, "Token refreshed successfully"));
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Invalid refresh token");
                return BadRequest(ApiResponse.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while refreshing token"));
            }
        }

        /// <summary>
        /// Validates a JWT token
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>Token validation result</returns>
        /// <response code="200">Token validation result</response>
        /// <response code="400">Invalid token</response>
        [HttpPost("validate-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateToken([FromBody] string token)
        {
            try
            {
                var isValid = await _authService.ValidateTokenAsync(token);
                var responseData = new { isValid };
                return Ok(ApiResponse<object>.Success(responseData, "Token validation completed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return BadRequest(ApiResponse.BadRequest("Invalid token"));
            }
        }

        /// <summary>
        /// Revokes a JWT token
        /// </summary>
        /// <param name="token">JWT token to revoke</param>
        /// <returns>Success status of token revocation</returns>
        /// <response code="200">Token revoked successfully</response>
        /// <response code="400">Error revoking token</response>
        [HttpPost("revoke-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RevokeToken([FromBody] string token)
        {
            try
            {
                var success = await _authService.RevokeTokenAsync(token);
                var responseData = new { success };
                return Ok(ApiResponse<object>.Success(responseData, "Token revoked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return BadRequest(ApiResponse.BadRequest("Error revoking token"));
            }
        }

        /// <summary>
        /// Changes user password
        /// </summary>
        /// <param name="dto">Password change information</param>
        /// <returns>Success status of password change</returns>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Invalid password data</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.Unauthorized());
                }

                var success = await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
                var responseData = new { success };
                return Ok(ApiResponse<object>.Success(responseData, "Password changed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Password change failed for user");
                return BadRequest(ApiResponse.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while changing password"));
            }
        }

        /// <summary>
        /// Initiates password reset process
        /// </summary>
        /// <param name="dto">Email address for password reset</param>
        /// <returns>Success status of password reset initiation</returns>
        /// <response code="200">Password reset initiated successfully</response>
        /// <response code="400">Error initiating password reset</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                var success = await _authService.ResetPasswordAsync(dto.Email);
                var responseData = new { success };
                return Ok(ApiResponse<object>.Success(responseData, "Password reset initiated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return BadRequest(ApiResponse.BadRequest("An error occurred while resetting password"));
            }
        }

        /// <summary>
        /// Verifies user email address
        /// </summary>
        /// <param name="dto">Email verification information</param>
        /// <returns>Success status of email verification</returns>
        /// <response code="200">Email verified successfully</response>
        /// <response code="400">Error verifying email</response>
        [HttpPost("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            try
            {
                var success = await _authService.VerifyEmailAsync(dto.UserId, dto.Token);
                var responseData = new { success };
                return Ok(ApiResponse<object>.Success(responseData, "Email verified successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email");
                return BadRequest(ApiResponse.BadRequest("An error occurred while verifying email"));
            }
        }

        /// <summary>
        /// Retrieves user information by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User information</returns>
        /// <response code="200">User found</response>
        /// <response code="404">User not found</response>
        [HttpGet("users/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _authService.GetUserAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.NotFound("User not found"));
            }

            return Ok(ApiResponse<User>.Success(user, "User retrieved successfully"));
        }

        /// <summary>
        /// Checks if an admin user exists in the system
        /// </summary>
        /// <returns>True if an admin exists, false otherwise</returns>
        /// <response code="200">Successfully checked admin existence</response>
        [HttpGet("admin-exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckAdminExists()
        {
            try
            {
                var adminExists = await _roleService.AdminExistsAsync();
                var responseData = new { adminExists };
                return Ok(ApiResponse<object>.Success(responseData, "Admin existence check completed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if admin exists");
                return StatusCode(500, ApiResponse.InternalServerError("An error occurred while checking admin existence"));
            }
        }
    }
}
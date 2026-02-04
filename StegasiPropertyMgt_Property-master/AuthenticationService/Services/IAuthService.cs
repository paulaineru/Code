using SharedKernel.Dto;
using SharedKernel.Models;

namespace AuthenticationService.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(RegisterUserDto dto);
        Task<LoginResponseDto> LoginAsync(LoginUserDto dto);
        Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> VerifyEmailAsync(string userId, string token);
        Task<User> GetUserAsync(string id);
    }
} 
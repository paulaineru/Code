using AuthenticationService.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Dto;
using SharedKernel.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthenticationService.Configuration;

namespace AuthenticationService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly RoleConfiguration _roleConfig;

        public AuthService(
            IAuthDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            RoleConfiguration roleConfig)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _roleConfig = roleConfig;
        }

        private async Task<bool> AdminExistsAsync()
        {
            return await _context.UserRoles
                .AnyAsync(ur => ur.RoleId == _roleConfig.AdminRoleId);
        }

        public async Task<User> RegisterAsync(RegisterUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Check if trying to register as admin
            if (dto.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true)
            {
                var adminExists = await AdminExistsAsync();
                if (adminExists)
                {
                    _logger.LogWarning("Attempt to register admin user when admin already exists. Email: {Email}", dto.Email);
                    throw new InvalidOperationException("Admin user already exists. Cannot create additional admin users through registration.");
                }
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Username = dto.UserName,
                PasswordHash = HashPassword(dto.Password),
                Role = dto.Role ?? "User",
                Address = dto.Address ?? string.Empty,
                District = dto.District ?? string.Empty,
                Country = dto.Country ?? string.Empty,
                Phone = dto.PhoneNumber ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assign role to user based on the requested role
            Guid roleId = GetRoleIdByName(dto.Role ?? "User");
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "SYSTEM"
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "User created successfully with role. UserId: {UserId}, Email: {Email}, Role: {Role}, CreatedAt: {CreatedAt}",
                user.Id,
                user.Email,
                dto.Role ?? "User",
                user.CreatedAt
            );

            return user;
        }

        private Guid GetRoleIdByName(string roleName)
        {
            return roleName?.ToLower() switch
            {
                "admin" => _roleConfig.AdminRoleId,
                "user" => _roleConfig.UserRoleId,
                "estates officer" => _roleConfig.EstatesOfficerRoleId,
                "property manager" => _roleConfig.PropertyManagerRoleId,
                "maintenance officer" => _roleConfig.MaintenanceOfficerRoleId,
                "finance team" => _roleConfig.FinanceTeamRoleId,
                "sales officer" => _roleConfig.SalesOfficerRoleId,
                "sales manager" => _roleConfig.SalesManagerRoleId,
                "tenant" => _roleConfig.TenantRoleId,
                _ => _roleConfig.UserRoleId // Default to User role
            };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginUserDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.UserName);

            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            {
                throw new InvalidOperationException("Invalid username or password");
            }

            if (user.DeletedAt.HasValue)
            {
                throw new InvalidOperationException("User account is deleted");
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = await GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return new LoginResponseDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresIn = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60")
            };
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                throw new SecurityTokenException("Invalid token");
            }

            var user = await _context.Users.FindAsync(userGuid);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var newToken = await GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            return new LoginResponseDto
            {
                AccessToken = newToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60")
            };
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
                var securityKey = new SymmetricSecurityKey(key);
                securityKey.KeyId = "stegasi-auth-key";

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            // Implement token revocation logic
            return true;
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new InvalidOperationException("Invalid user ID");
            }

            var user = await _context.Users.FindAsync(userGuid);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Current password is incorrect");
            }

            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return true; // Return true to prevent email enumeration
            }

            // Generate reset token and send email
            var resetToken = GeneratePasswordResetToken();
            // TODO: Send email with reset token
            return true;
        }

        public async Task<bool> VerifyEmailAsync(string userId, string token)
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new InvalidOperationException("Invalid user ID");
            }

            // Implement email verification logic
            return true;
        }

        public async Task<User> GetUserAsync(string id)
        {
            if (!Guid.TryParse(id, out var userGuid))
            {
                throw new InvalidOperationException("Invalid user ID");
            }

            var user = await _context.Users.FindAsync(userGuid);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            return user;
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            // Get all user roles and prioritize the most privileged one
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == user.Id)
                .ToListAsync();

            var roleName = GetHighestPriorityRole(userRoles.Select(ur => ur.Role.Name).ToList());

            _logger.LogInformation("JWT Token Generation - UserId: {UserId}, AllRoles: {AllRoles}, SelectedRole: {SelectedRole}", 
                user.Id, string.Join(", ", userRoles.Select(ur => ur.Role.Name)), roleName);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
            var securityKey = new SymmetricSecurityKey(key);
            securityKey.KeyId = "stegasi-auth-key";

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.GivenName, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),
                    new Claim(ClaimTypes.Role, roleName)
                }),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60")),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GetHighestPriorityRole(List<string> roles)
        {
            // Define role priority (higher index = higher priority)
            var rolePriority = new List<string>
            {
                "Tenant",
                "User", 
                "Sales Officer",
                "Maintenance Officer",
                "Finance Team",
                "Sales Manager",
                "Estates Officer",
                "Property Manager",
                "Admin"
            };

            if (!roles.Any())
                return "User"; // Default role

            // Find the role with the highest priority
            var highestPriorityRole = roles
                .OrderByDescending(role => rolePriority.IndexOf(role))
                .First();

            return highestPriorityRole;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"))),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        private string GeneratePasswordResetToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
} 
using System;
using System.Threading.Tasks;
using SharedKernel.Models;
using SharedKernel.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;

namespace SharedKernel.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly string _authServiceUrl;

        public UserService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _authServiceUrl = configuration["Services:AuthenticationService"] ?? "http://localhost:5031";
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync($"{_authServiceUrl}/api/v1.0/user/{userId}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<User>();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var response = await _httpClient.GetAsync($"{_authServiceUrl}/api/v1/users/email/{email}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<User>();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            var response = await _httpClient.GetAsync($"{_authServiceUrl}/api/v1/users/role/{role}");
            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<User>();

            return await response.Content.ReadFromJsonAsync<IEnumerable<User>>() ?? Enumerable.Empty<User>();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_authServiceUrl}/api/v1/users", user);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>() ?? throw new Exception("Failed to create user");
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_authServiceUrl}/api/users/{user.Id}", user);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>() ?? throw new Exception("Failed to update user");
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var response = await _httpClient.DeleteAsync($"{_authServiceUrl}/api/users/{userId}");
            response.EnsureSuccessStatusCode();
        }
    }
}

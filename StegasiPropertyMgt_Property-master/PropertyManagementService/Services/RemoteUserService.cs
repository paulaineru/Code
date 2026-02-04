using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using SharedKernel.Models;
using SharedKernel.Services;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace PropertyManagementService.Services
{
    public class RemoteUserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly string _authServiceUrl;

        public RemoteUserService(HttpClient httpClient, IConfiguration configuration)
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
            var response = await _httpClient.GetAsync($"{_authServiceUrl}/api/users/email/{email}");
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadFromJsonAsync<User>();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            var response = await _httpClient.GetAsync($"{_authServiceUrl}/api/users/role/{role}");
            if (!response.IsSuccessStatusCode)
                return new List<User>();
            return await response.Content.ReadFromJsonAsync<IEnumerable<User>>() ?? new List<User>();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_authServiceUrl}/api/users", user);
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

        public async Task<string> GetUserEmailByIdAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync($"/user/{userId}/email");

            if (!response.IsSuccessStatusCode)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            var email = await response.Content.ReadAsStringAsync();
            return email.Trim('"'); // Trim quotes from JSON string
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            var response = await _httpClient.GetAsync($"/user/username/{username}");

            if (!response.IsSuccessStatusCode)
            {
                throw new KeyNotFoundException($"User with username {username} not found.");
            }

            var user = await response.Content.ReadFromJsonAsync<User>();
            return user ?? throw new InvalidOperationException($"Failed to deserialize user with username {username}");
        }
    }
}
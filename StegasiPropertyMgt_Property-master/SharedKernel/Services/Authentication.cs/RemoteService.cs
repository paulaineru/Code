// SharedKernel/Services/RemoteUserService.cs
using System.Net.Http.Json;
using System.Threading.Tasks;
using SharedKernel.Models;
using System;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace SharedKernel.Services
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
                return Enumerable.Empty<User>();

            return await response.Content.ReadFromJsonAsync<IEnumerable<User>>() ?? Enumerable.Empty<User>();
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
    }
}
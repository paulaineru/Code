// SharedKernel/Services/IUserService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedKernel.Models;

namespace SharedKernel.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid userId);
    }
}
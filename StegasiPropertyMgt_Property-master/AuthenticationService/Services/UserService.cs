// SharedKernel/Services/UserService.cs
using System.Threading.Tasks;
using AuthenticationService.Repository;
using SharedKernel.Services;
using SharedKernel.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;

namespace AuthenticationService.Services
{
    public class UserService : IUserService, IUserRegistrationService
    {
        private readonly AuthDbContext _authDbContext;

        public UserService(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _authDbContext.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _authDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            return await _authDbContext.Users.Where(u => u.Role == role).ToListAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            await _authDbContext.Users.AddAsync(user);
            await _authDbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _authDbContext.Users.Update(user);
            await _authDbContext.SaveChangesAsync();
            return user;
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _authDbContext.Users.FindAsync(userId);
            if (user != null)
            {
                _authDbContext.Users.Remove(user);
                await _authDbContext.SaveChangesAsync();
            }
        }

        public async Task<string> GetUserEmailByIdAsync(Guid userId)
        {
            var user = await _authDbContext.Users.FindAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            return user.Email;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _authDbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddUserAsync(User user)
        {
            await _authDbContext.Users.AddAsync(user);
            await _authDbContext.SaveChangesAsync();
        }
    }
}
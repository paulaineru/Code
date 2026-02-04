

using System.Threading.Tasks;
using SharedKernel.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using SharedKernel.Utilities;
using Microsoft.AspNetCore.Http;


namespace AuthenticationService.Repository
{
    public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User> GetByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }
}
}
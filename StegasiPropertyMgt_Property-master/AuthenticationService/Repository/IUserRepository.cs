using System;
using System.Threading.Tasks;
using SharedKernel.Models;
namespace AuthenticationService.Repository
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByIdAsync(Guid userId);
    }
}




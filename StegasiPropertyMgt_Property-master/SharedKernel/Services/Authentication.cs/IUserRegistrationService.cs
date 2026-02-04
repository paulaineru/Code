// SharedKernel/Services/IUserRegistrationService.cs
using SharedKernel.Models;
using System.Threading.Tasks;

namespace SharedKernel.Services
{
    public interface IUserRegistrationService
    {
        Task AddUserAsync(User user); // Define the AddUserAsync method here
    }
}
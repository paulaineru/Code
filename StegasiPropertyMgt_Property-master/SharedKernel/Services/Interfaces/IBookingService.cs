using SharedKernel.Dto;
using SharedKernel.Models;
using SharedKernel.Models.Tenants;
using System.Threading.Tasks;
using SharedKernel.Dto.Tenants;

namespace SharedKernel.Services
{


    public interface IBookingService
    {
       
        Task<Booking> BookPropertyAsync(BookPropertyRequest dto,Guid? tenantId = null,string token=null); // BookPropertyRequest
        Task<Booking> GetBookingByIdAsync(Guid id);
        Task<List<Booking>> GetAllBookingsAsync();
        Task UpdateBookingAsync(Booking booking);
        Task<List<Booking>> GetBookingsByTenantAsync(Guid tenantId);
    }
}
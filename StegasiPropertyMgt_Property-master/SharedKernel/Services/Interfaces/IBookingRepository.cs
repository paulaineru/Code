// TenantManagementService/Data/IBookingRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedKernel.Models;

namespace SharedKernel.Services
{
    public interface IBookingRepository
    {
        Task AddAsync(Booking booking);
        Task<Booking> GetByIdAsync(Guid id);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(Booking booking);
        Task<List<Booking>> GetByTenantIdAsync(Guid tenantId);

        Task<Booking> AddBookingAsync(Booking booking);
        Task<List<Booking>> GetAllBookingsAsync();
        
        Task<Booking> GetBookingByIdAsync(Guid id);
        
        // New method to check for existing bookings for a property within a date range
        Task<List<Booking>> GetExistingBookingsForPropertyAsync(Guid propertyId, DateTime startDate, DateTime endDate);
    }
}
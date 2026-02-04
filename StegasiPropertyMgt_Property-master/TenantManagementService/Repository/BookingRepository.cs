// TenantManagementService/Data/BookingRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Models;
using SharedKernel.Services;

namespace TenantManagementService.Repository
{
    public class BookingRepository : IBookingRepository
    {
        private readonly TenantDbContext _context;

        public BookingRepository(TenantDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<Booking> GetByIdAsync(Guid id)
        {
            return await _context.Bookings.FindAsync(id);
        }

        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Booking booking)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Booking>> GetByTenantIdAsync(Guid tenantId)
        {
            return await _context.Bookings
                .Where(b => b.TenantId == tenantId)
                .ToListAsync();
        }
        // Implement AddBookingAsync:
        public async Task<Booking> AddBookingAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        // Implement GetAllBookingsAsync:
        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings.ToListAsync();
        }
        // All required methods:
        public async Task<Booking> GetBookingByIdAsync(Guid id)
        {
            return await _context.Bookings.FindAsync(id);
        }
        
        // Check for existing bookings for a property within a date range
        public async Task<List<Booking>> GetExistingBookingsForPropertyAsync(Guid propertyId, DateTime startDate, DateTime endDate)
        {
            return await _context.Bookings
                .Where(b => b.PropertyId == propertyId && 
                           // Check for overlapping date ranges
                           ((b.StartDate <= startDate && b.EndDate > startDate) || // New booking starts during existing booking
                            (b.StartDate < endDate && b.EndDate >= endDate) || // New booking ends during existing booking
                            (b.StartDate >= startDate && b.EndDate <= endDate) || // New booking completely contains existing booking
                            (b.StartDate <= startDate && b.EndDate >= endDate))) // Existing booking completely contains new booking
                .ToListAsync();
        }
    }
}
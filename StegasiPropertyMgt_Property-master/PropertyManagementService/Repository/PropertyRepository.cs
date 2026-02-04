
using SharedKernel.Models;
using SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PropertyManagementService.Repository
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly PropertyDbContext _context;

        public PropertyRepository(PropertyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Property property)
        {
            await _context.Properties.AddAsync(property);
            await _context.SaveChangesAsync();
        }

        public async Task<Property> GetByIdAsync(Guid id)
        {
            return await _context.Properties.FindAsync(id);
        }

        public async Task UpdateAsync(Property property)
        {
            _context.Properties.Update(property);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Property property)
        {
            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Property>> GetAvailablePropertiesAsync()
        {
            return await _context.Properties
                .Where(p => p.IsRentable || p.IsSaleable) // Only include rentable or saleable properties
                .ToListAsync();
        }

        public async Task<List<Property>> GetAllAsync()
        {
            return await _context.Properties.ToListAsync();
        }
    }
}
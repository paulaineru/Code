using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedKernel.Models;

namespace ReportingService.Repository
{
    public interface IPropertyRepository
    {
        Task<Property> GetByIdAsync(Guid id);
        Task<List<Property>> GetAllAsync();
    }
} 
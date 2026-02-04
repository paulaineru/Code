using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedKernel.Dto;
using SharedKernel.Models;
namespace SharedKernel.Services
{
    public interface IAmenityService
    {
        Task<Amenity> CreateAmenityAsync(CreateAmenityDto dto, string userId);
        Task<Amenity> GetAmenityByIdAsync(Guid id);
        Task<List<Amenity>> GetAllAmenitiesAsync();
        Task UpdateAmenityAsync(Guid id, CreateAmenityDto dto, string userId);
        Task DeleteAmenityAsync(Guid id, string userId);
        Task AssociateAmenityWithPropertyAsync(Guid amenityId, Guid propertyId, string userId);
        Task DissociateAmenityFromPropertyAsync(Guid amenityId, Guid propertyId, string userId);
    }
}
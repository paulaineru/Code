using Microsoft.EntityFrameworkCore;
using SharedKernel.Models;
using PropertyManagementService.Repository;
using SharedKernel.Services;
using SharedKernel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PropertyManagementService.Services
{


    public class AmenityService : IAmenityService
    {
        private readonly PropertyDbContext _context;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AmenityService> _logger;

        public AmenityService(
            PropertyDbContext context,
            IAuditLogService auditLogService,
            ILogger<AmenityService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Amenity> CreateAmenityAsync(CreateAmenityDto dto, string userId)
        {
            ValidateAmenityDto(dto);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var amenity = new Amenity
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description
                };

                _context.Amenities.Add(amenity);
                await _context.SaveChangesAsync();
                Guid? guidUserId = Guid.TryParse(userId, out Guid parsedId) ? parsedId : (Guid?)null;
                await _auditLogService.RecordActionAsync(
                    "AmenityCreated",
                    amenity.Id,
                    guidUserId,
                    $"Created amenity with name: {dto.Name}",
                    Guid.Empty);

                await transaction.CommitAsync();
                return amenity;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create amenity: {Name}", dto.Name);
                throw;
            }
        }

        public async Task<Amenity> GetAmenityByIdAsync(Guid id)
        {
            var amenity = await _context.Amenities
                // .Include(a => a.Properties)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (amenity == null)
            {
                _logger.LogWarning("Amenity not found with ID: {Id}", id);
                throw new KeyNotFoundException($"Amenity with ID {id} not found.");
            }

            return amenity;
        }

        public async Task<List<Amenity>> GetAllAmenitiesAsync()
        {
            try
            {
                var amenities = await _context.Amenities
                    .AsNoTracking()
                    // .Include(a => a.Properties)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} amenities", amenities.Count);
                return amenities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all amenities");
                throw;
            }
        }

        public async Task UpdateAmenityAsync(Guid id, CreateAmenityDto dto, string userId)
        {
            ValidateAmenityDto(dto);

            var amenity = await GetAmenityByIdAsync(id);
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                amenity.Name = dto.Name;
                amenity.Description = dto.Description;

                _context.Amenities.Update(amenity);
                await _context.SaveChangesAsync();
                Guid? guidUserId = Guid.TryParse(userId, out Guid parsedId) ? parsedId : (Guid?)null;
                await _auditLogService.RecordActionAsync(
                    "AmenityUpdated",
                    amenity.Id,
                    guidUserId,
                    $"Updated amenity with name: {dto.Name}",
                    Guid.Empty);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update amenity: {Id}", id);
                throw;
            }
        }

        public async Task DeleteAmenityAsync(Guid id, string userId)
        {
            var amenity = await GetAmenityByIdAsync(id);
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Amenities.Remove(amenity);
                await _context.SaveChangesAsync();
                Guid? guidUserId = Guid.TryParse(userId, out Guid parsedId) ? parsedId : (Guid?)null;
                await _auditLogService.RecordActionAsync(
                    "AmenityDeleted",
                    amenity.Id,
                    guidUserId,
                    $"Deleted amenity with name: {amenity.Name}",
                    Guid.Empty);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to delete amenity: {Id}", id);
                throw;
            }
        }

        public async Task AssociateAmenityWithPropertyAsync(Guid amenityId, Guid propertyId, string userId)
        {
            var amenity = await _context.Amenities
                // .Include(a => a.Properties)
                .FirstOrDefaultAsync(a => a.Id == amenityId);

            if (amenity == null)
            {
                _logger.LogWarning("Amenity not found with ID: {Id}", amenityId);
                throw new KeyNotFoundException($"Amenity with ID {amenityId} not found.");
            }

            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null)
            {
                _logger.LogWarning("Property not found with ID: {Id}", propertyId);
                throw new KeyNotFoundException($"Property with ID {propertyId} not found.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // if (!amenity.Properties.Any(p => p.Id == propertyId))
                // {
                //     amenity.Properties.Add(property);
                //     await _context.SaveChangesAsync();
                //     Guid? guidUserId = Guid.TryParse(userId, out Guid parsedId) ? parsedId : (Guid?)null;

                //     await _auditLogService.RecordActionAsync(
                //         "AmenityAssociated",
                //         propertyId,
                //         guidUserId,
                //         $"Associated amenity with property with ID: {propertyId}",
                //         Guid.Empty);
                // }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to associate amenity {AmenityId} with property {PropertyId}", amenityId, propertyId);
                throw;
            }
        }

        public async Task DissociateAmenityFromPropertyAsync(Guid amenityId, Guid propertyId, string userId)
        {
            var amenity = await _context.Amenities
                // .Include(a => a.Properties)
                .FirstOrDefaultAsync(a => a.Id == amenityId);

            if (amenity == null)
            {
                _logger.LogWarning("Amenity not found with ID: {Id}", amenityId);
                throw new KeyNotFoundException($"Amenity with ID {amenityId} not found.");
            }

            // var property = amenity.Properties.FirstOrDefault(p => p.Id == propertyId);
            // if (property == null)
            // {
            //     _logger.LogWarning("Property {PropertyId} not associated with amenity {AmenityId}", propertyId, amenityId);
            //     return; // No association exists, so no action needed
            // }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // amenity.Properties.Remove(property);
                // await _context.SaveChangesAsync();
                // Guid? guidUserId = Guid.TryParse(userId, out Guid parsedId) ? parsedId : (Guid?)null;

                // await _auditLogService.RecordActionAsync(
                //     "AmenityDissociated",
                //     propertyId,
                //     guidUserId,
                //     $"Dissociated amenity with property with ID: {propertyId}",
                //     Guid.Empty);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to dissociate amenity {AmenityId} from property {PropertyId}", amenityId, propertyId);
                throw;
            }
        }

        private void ValidateAmenityDto(CreateAmenityDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Amenity DTO cannot be null");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required and cannot be empty", nameof(dto.Name));
        }
    }
}
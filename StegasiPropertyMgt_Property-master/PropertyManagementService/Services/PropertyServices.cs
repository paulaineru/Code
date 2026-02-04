using Microsoft.EntityFrameworkCore;
using SharedKernel.Dto;
using SharedKernel.Models;
using SharedKernel.Services;
using PropertyManagementService.Repository;
using PropertyManagementService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;

namespace PropertyManagementService.Services
{

    public class PropertyService : IPropertyService
    {
        private readonly PropertyDbContext _context;
        private readonly IAuditLogService _auditLogService;
        private readonly IAmenityService _amenityService;
        private readonly IApprovalWorkflowService _approvalWorkflowService;
        private readonly ILogger<PropertyService> _logger;

        public PropertyService(
            PropertyDbContext context,
            IAuditLogService auditLogService,
            IAmenityService amenityService,
            IApprovalWorkflowService approvalWorkflowService,
            ILogger<PropertyService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _amenityService = amenityService ?? throw new ArgumentNullException(nameof(amenityService));
            _approvalWorkflowService = approvalWorkflowService ?? throw new ArgumentNullException(nameof(approvalWorkflowService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Property> CreatePropertyAsync(CreatePropertyDto dto, string userId)
        {
            ValidatePropertyDto(dto);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var property = PropertyFactory.CreateProperty(dto);
                property.ApprovalStatus = "Pending"; // Initial status before workflow creation

                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                switch (property)
                {
                    case CommercialProperty commercial:
                        if (commercial.Wings != null)
                        {
                            foreach (var wing in commercial.Wings)
                            {
                                wing.PropertyId = commercial.Id;
                                if (wing.Id != Guid.Empty) // Log if Id is unexpectedly set
                                    _logger.LogWarning("WingDetails Id was pre-set: {Id}", wing.Id);
                                wing.Id = Guid.Empty;
                                
                            }
                               
                            _context.WingDetails.AddRange(commercial.Wings);
                        }
                        break;

                    case CondominiumProperty condominium:
                        if (condominium.Units != null)
                        {
                            foreach (var unit in condominium.Units)
                            {
                                unit.PropertyId = condominium.Id;
                                if (unit.Id != Guid.Empty) // Log if Id is unexpectedly set
                                    _logger.LogWarning("CondominiumUnit Id was pre-set: {Id}", unit.Id);
                                unit.Id = Guid.Empty;
                            }
                                
                            _context.CondominiumUnits.AddRange(condominium.Units);
                        }
                        break;

                    case TownhouseProperty townhouse:
                        if (townhouse.Clusters != null)
                        {
                            foreach (var cluster in townhouse.Clusters)
                            {
                                cluster.PropertyId = townhouse.Id;
                                 _logger.LogWarning("Cluster Id was pre-set: {Id}", cluster.Id);
                                cluster.Id = Guid.Empty;

                            }
                            
                            _context.TownhouseClusters.AddRange(townhouse.Clusters);
                        }
                        break;
                        // Bungalow, Villa, and VacantLand do not have additional collections
                }

                await _context.SaveChangesAsync();

                if (dto.AmenityIds != null && dto.AmenityIds.Any())
                {
                    foreach (var amenityId in dto.AmenityIds)
                        await _amenityService.AssociateAmenityWithPropertyAsync(amenityId, property.Id, userId);
                }
                Guid? guidUserId = Guid.TryParse(userId, out Guid parsedId) ? parsedId : (Guid?)null;
                await _auditLogService.RecordActionAsync(
                    "PropertyCreated",
                    property.Id,
                    guidUserId,
                    $"Created property {property.Name}",
                    Guid.Empty);

                await transaction.CommitAsync();
                return property;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create property: {Name}", dto.Name);
                throw;
            }
        }

        public async Task<Property> GetPropertyByIdAsync(Guid id, string? token = null)
        {
            var property = await _context.Properties
                .Include(p => p.LeaseAgreements)
                .Include(p => p.Amenities)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                _logger.LogWarning("Property not found with ID: {Id}", id);
                throw new KeyNotFoundException($"Property with ID {id} not found.");
            }

            switch (property)
            {
                case CommercialProperty commercial:
                    await _context.Entry(commercial).Collection(c => c.Wings).LoadAsync();
                    break;
                case CondominiumProperty condominium:
                    await _context.Entry(condominium).Collection(c => c.Units).LoadAsync();
                    break;
                case TownhouseProperty townhouse:
                    await _context.Entry(townhouse).Collection(t => t.Clusters).LoadAsync();
                    break;
            }

            if (property.Amenities != null)
            {
                var amenities = property.Amenities.Where(a => a != null).ToList();
                property.Amenities = amenities;
            }

            return property;
        }

         public async Task<PropertyDetailResponse> GetPropertyDetailByIdAsync(Guid id, string? token = null)
        {
            var property = await _context.Properties
                .Include(p => p.LeaseAgreements)
                .Include(p => p.Amenities)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                _logger.LogWarning("Property not found with ID: {Id}", id);
                throw new KeyNotFoundException($"Property with ID {id} not found.");
            }

            switch (property)
            {
                case CommercialProperty commercial:
                    await _context.Entry(commercial).Collection(c => c.Wings).LoadAsync();
                    break;
                case CondominiumProperty condominium:
                    await _context.Entry(condominium).Collection(c => c.Units).LoadAsync();
                    break;
                case TownhouseProperty townhouse:
                    await _context.Entry(townhouse).Collection(t => t.Clusters).LoadAsync();
                    break;
            }

            var PropertyDetailResponse = new PropertyDetailResponse()
            {
                Id = property.Id,
                Name = property.Name,
                Address = property.Address,
                OwnerId = property.OwnerId,
                PropertyManagerId = property.PropertyManagerId,
                YearOfCommissionOrPurchase = property.YearOfCommissionOrPurchase,
                FairValue = property.FairValue,
                InsurableValue = property.InsurableValue,
                OwnershipStatus = property.OwnershipStatus,
                SalePrice = (decimal)property.SalePrice,
                Type = property.PropertyType,
                Status = property.ApprovalStatus,
                RentPrice = property.RentPrice,
                IsRentable = property.IsRentable,
                Images = new List<string>(), // Initialize empty list for now
                PrimaryImageUrl = null // Initialize as null for now
            };
            return PropertyDetailResponse;
        }

        

        public async Task<List<Property>> GetAllPropertiesAsync()
        {
            try
            {
                var properties = await _context.Properties
                    .AsNoTracking()
                    .Include(p => p.LeaseAgreements)
                    .Include(p => p.Amenities)
                    .ToListAsync();

                foreach (var property in properties)
                {
                    switch (property)
                    {
                        case CommercialProperty commercial:
                            await _context.Entry(commercial).Collection(c => c.Wings).LoadAsync();
                            break;
                        case CondominiumProperty condominium:
                            await _context.Entry(condominium).Collection(c => c.Units).LoadAsync();
                            break;
                        case TownhouseProperty townhouse:
                            await _context.Entry(townhouse).Collection(t => t.Clusters).LoadAsync();
                            break;
                    }
                }

                return properties;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all properties");
                throw;
            }
        }

        public async Task<List<Property>> GetPropertiesByTypeAsync(string propertyType)
        {
            if (string.IsNullOrWhiteSpace(propertyType))
                throw new ArgumentException("Property type cannot be null or empty", nameof(propertyType));

            try
            {
                var normalizedType = propertyType.ToLower();
                var validTypes = new[] { "commercial", "condominium", "townhouse", "bungalow", "villa", "vacantland" };

                if (!validTypes.Contains(normalizedType))
                    throw new ArgumentException($"Invalid property type. Valid types are: {string.Join(", ", validTypes)}", nameof(propertyType));

                var properties = await _context.Properties
                    .AsNoTracking()
                    //.Where(p => p.GetPropertyType().ToLower() == normalizedType)
                    .Where(p => p.PropertyType.ToLower() == normalizedType)
                    .Include(p => p.LeaseAgreements)
                    .Include(p => p.Amenities)
                    .ToListAsync();

                foreach (var property in properties)
                {
                    switch (property)
                    {
                        case CommercialProperty commercial:
                            await _context.Entry(commercial).Collection(c => c.Wings).LoadAsync();
                            break;
                        case CondominiumProperty condominium:
                            await _context.Entry(condominium).Collection(c => c.Units).LoadAsync();
                            break;
                        case TownhouseProperty townhouse:
                            await _context.Entry(townhouse).Collection(t => t.Clusters).LoadAsync();
                            break;
                    }
                }

                _logger.LogInformation("Retrieved {Count} properties of type {Type}", properties.Count, normalizedType);
                return properties;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve properties of type: {Type}", propertyType);
                throw;
            }
        }

        public async Task<List<Property>> GetPropertiesByFilterAsync(string? status, string? type)
        {
            if (string.IsNullOrWhiteSpace(status) && string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("At least one filter parameter (status or type) must be provided");

            try
            {
                var query = _context.Properties.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(status))
                {
                    var normalizedStatus = status.ToLower();
                    var validStatuses = new[] { "pending", "approved", "rejected", "moreinfo", "active" };
                    if (!validStatuses.Contains(normalizedStatus))
                        throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}", nameof(status));
                    query = query.Where(p => p.ApprovalStatus.ToLower() == (normalizedStatus == "active" ? "approved" : normalizedStatus));
                }

                if (!string.IsNullOrWhiteSpace(type))
                {
                    var normalizedType = type.ToLower();
                    var validTypes = new[] { "commercial", "condominium", "townhouse", "bungalow", "villa", "vacantland", "residential" };
                    if (!validTypes.Contains(normalizedType))
                        throw new ArgumentException($"Invalid type. Valid types are: {string.Join(", ", validTypes)}", nameof(type));

                    if (normalizedType == "residential")
                        query = query.Where(p => p.PropertyType.ToLower() == "condominium" ||
                                              p.PropertyType.ToLower() == "townhouse" ||
                                              p.PropertyType.ToLower() == "bungalow" ||
                                              p.PropertyType.ToLower() == "villa");
                    else
                        query = query.Where(p => p.PropertyType.ToLower() == normalizedType);
                }

                var properties = await query
                    .Include(p => p.LeaseAgreements)
                    .Include(p => p.Amenities)
                    .ToListAsync();

                foreach (var property in properties)
                {
                    switch (property)
                    {
                        case CommercialProperty commercial:
                            await _context.Entry(commercial).Collection(c => c.Wings).LoadAsync();
                            break;
                        case CondominiumProperty condominium:
                            await _context.Entry(condominium).Collection(c => c.Units).LoadAsync();
                            break;
                        case TownhouseProperty townhouse:
                            await _context.Entry(townhouse).Collection(t => t.Clusters).LoadAsync();
                            break;
                    }
                }

                return properties;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve properties with status: {Status} and type: {Type}", status, type);
                throw;
            }
        }
        public async Task UpdatePropertyAsync(Guid id, CreatePropertyDto dto, string userId)
        {
            var property = await GetPropertyByIdAsync(id);
            if (property == null)
                throw new KeyNotFoundException($"Property with ID {id} not found");

            // Check if property is in a state that can be updated
            var workflow = await _approvalWorkflowService.GetWorkflowByEntityAsync("Property", id);
            if (workflow != null && workflow.Status == ApprovalStatus.Approved)
            {
                // Allow status-only updates during approval process
                // Check if this is just a status update (only ApprovalStatus is being changed)
                var isStatusOnlyUpdate = IsStatusOnlyUpdate(property, dto);
                if (!isStatusOnlyUpdate)
                {
                    throw new InvalidOperationException("Cannot update an approved property. Please create a new version or request a change.");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                PropertyMapper.UpdatePropertyFromDto(property, dto);

                // If property is in workflow, update its status
                if (workflow != null)
                {
                    property.ApprovalStatus = workflow.Status.ToString();
                }

                await _context.SaveChangesAsync();

                if (dto.AmenityIds != null)
                {
                    // Get current amenities
                    var currentAmenities = await _context.Properties
                        .Include(p => p.Amenities)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (currentAmenities != null)
                    {
                        // Remove amenities not in the new list
                        var amenitiesToRemove = currentAmenities.Amenities
                            .Where(a => !dto.AmenityIds.Contains(a.Id))
                            .ToList();
                        foreach (var amenity in amenitiesToRemove)
                        {
                            currentAmenities.Amenities.Remove(amenity);
                        }

                        // Add new amenities
                        var amenitiesToAdd = await _context.Amenities
                            .Where(a => dto.AmenityIds.Contains(a.Id) && !currentAmenities.Amenities.Any(pa => pa.Id == a.Id))
                            .ToListAsync();
                        foreach (var amenity in amenitiesToAdd)
                        {
                            currentAmenities.Amenities.Add(amenity);
                        }
                    }
                }

                Guid? guidUserId = Guid.TryParse(userId, out Guid parsedId) ? parsedId : (Guid?)null;
                await _auditLogService.RecordActionAsync(
                    "PropertyUpdated",
                    id,
                    guidUserId,
                    $"Updated property {property.Name}",
                    Guid.Empty);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update property: {Id}", id);
                throw;
            }
        }

        private bool IsStatusOnlyUpdate(Property property, CreatePropertyDto dto)
        {
            // Check if only the ApprovalStatus is being changed
            return property.Name == dto.Name &&
                   property.Address == dto.Address &&
                   property.OwnerId == dto.OwnerId &&
                   /*property.GetPropertyType() == dto.PropertyType &&*/
                   property.PropertyType == dto.PropertyType &&
                   property.FairValue == dto.FairValue &&
                   property.InsurableValue == dto.InsurableValue &&
                   property.OwnershipStatus == dto.OwnershipStatus &&
                   property.SalePrice == dto.SalePrice &&
                   property.IsRentable == dto.IsRentable &&
                   property.IsSaleable == dto.IsSaleable &&
                   property.RentPrice == dto.RentPrice &&
                   property.ApprovalStatus != dto.ApprovalStatus; // Only status is different
        }

        public async Task DeletePropertyAsync(Guid id, string userId)
        {
            var property = await GetPropertyByIdAsync(id);
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (property.LeaseAgreements.Any())
                    throw new InvalidOperationException("Cannot delete property with active lease agreements.");

                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
                Guid? guidUserId = Guid.TryParse(userId, out Guid parsedId) ? parsedId : (Guid?)null;
                await _auditLogService.RecordActionAsync(
                    "PropertyDeleted",
                    property.Id,
                    guidUserId,
                    $"Deleted property {property.Name}",
                    Guid.Empty);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to delete property: {Id}", id);
                throw;
            }
        }

        private void ValidatePropertyDto(CreatePropertyDto dto)
        {
            var validator = new PropertyDtoValidator();
            validator.Validate(dto);
        }
    }

    internal static class PropertyFactory
    {
        public static Property CreateProperty(CreatePropertyDto dto)
        {
            return dto.PropertyType.ToLower() switch
            {
                "commercial" => new CommercialProperty
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Address = dto.Address,
                    OwnerId = dto.OwnerId,
                    YearOfCommissionOrPurchase = dto.YearOfCommissionOrPurchase ?? DateTime.UtcNow.Year,
                    FairValue = dto.FairValue ?? 0,
                    InsurableValue = dto.InsurableValue ?? 0,
                    OwnershipStatus = dto.OwnershipStatus,
                    SalePrice = dto.SalePrice,
                    IsRentable = dto.IsRentable ?? false,
                    IsSaleable = dto.IsSaleable ?? false,
                    RentPrice = (decimal)dto.RentPrice,
                    NumberOfWings = dto.NumberOfWings.Value,
                    ApprovalStatus = dto.ApprovalStatus,
                    Wings = dto.Wings.Select(w => new WingDetails
                    {
                        WingName = w.WingName,
                        FloorArea = w.FloorArea,
                        CommonArea = w.CommonArea,
                        UsageType = w.UsageType,
                        RentalPrice = w.RentalPrice
                        }).ToList(),
                    TotalParkingAreaPerFloor = dto.TotalParkingAreaPerFloor ?? 0
                },
                "condominium" => new CondominiumProperty
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Address = dto.Address,
                    OwnerId = dto.OwnerId,
                    YearOfCommissionOrPurchase = dto.YearOfCommissionOrPurchase ?? DateTime.UtcNow.Year,
                    FairValue = dto.FairValue ?? 0,
                    InsurableValue = dto.InsurableValue ?? 0,
                    OwnershipStatus = dto.OwnershipStatus,
                    SalePrice = dto.SalePrice,
                    IsRentable = dto.IsRentable ?? false,
                    IsSaleable = dto.IsSaleable ?? false,
                    RentPrice = dto.RentPrice,
                    NumberOfUnitsPerFloor = dto.NumberOfUnitsPerFloor.Value,
                    ApprovalStatus = dto.ApprovalStatus,
                    Units = dto.Units.Select(u => new CondominiumUnit
                    {
                        UnitType = u.UnitType,
                        UnitNumber = u.UnitNumber,
                        FloorNumber = u.FloorNumber,
                        SizeSquareFeet = u.UnitSize,
                        MonthlyRent = u.MonthlyRent
                    }).ToList(),
                    HOAFees = dto.HOAFees ?? 0
                },
                "townhouse" => new TownhouseProperty
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Address = dto.Address,
                    OwnerId = dto.OwnerId,
                    YearOfCommissionOrPurchase = dto.YearOfCommissionOrPurchase ?? DateTime.UtcNow.Year,
                    FairValue = dto.FairValue ?? 0,
                    InsurableValue = dto.InsurableValue ?? 0,
                    OwnershipStatus = dto.OwnershipStatus,
                    SalePrice = dto.SalePrice,
                    IsRentable = dto.IsRentable ?? false,
                    IsSaleable = dto.IsSaleable ?? false,
                    RentPrice = dto.RentPrice,
                    NumberOfClusters = dto.NumberOfClusters.Value,
                    ApprovalStatus = dto.ApprovalStatus,
                    Clusters = dto.Clusters.Select(c => new TownhouseCluster
                    {
                        ClusterName = c.ClusterName,
                        // ClusterType = c.ClusterType,
                        // Title = c.Title,
                        // ClusterSizeSquareFeet = (int)c.ClusterSizeSquareFeet,
                        // MonthlyRent = c.MonthlyRent
                    }).ToList()
                },
                "bungalow" => new BungalowProperty
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Address = dto.Address,
                    OwnerId = dto.OwnerId,
                    YearOfCommissionOrPurchase = dto.YearOfCommissionOrPurchase ?? DateTime.UtcNow.Year,
                    FairValue = dto.FairValue ?? 0,
                    InsurableValue = dto.InsurableValue ?? 0,
                    OwnershipStatus = dto.OwnershipStatus,
                    SalePrice = dto.SalePrice,
                    IsRentable = dto.IsRentable ?? false,
                    IsSaleable = dto.IsSaleable ?? false,
                    RentPrice = dto.RentPrice,
                    ApprovalStatus = dto.ApprovalStatus,
                    NumberOfBedrooms = dto.NumberOfBedrooms.Value,
                    NumberOfBathrooms = dto.NumberOfBathrooms.Value,
                },
                "villa" => new VillaProperty
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Address = dto.Address,
                    OwnerId = dto.OwnerId,
                    YearOfCommissionOrPurchase = dto.YearOfCommissionOrPurchase ?? DateTime.UtcNow.Year,
                    FairValue = dto.FairValue ?? 0,
                    InsurableValue = dto.InsurableValue ?? 0,
                    OwnershipStatus = dto.OwnershipStatus,
                    SalePrice = dto.SalePrice,
                    IsRentable = dto.IsRentable ?? false,
                    IsSaleable = dto.IsSaleable ?? false,
                    RentPrice = dto.RentPrice,
                    ApprovalStatus = dto.ApprovalStatus,
                    NumberOfBedrooms = dto.NumberOfBedrooms.Value,
                    NumberOfBathrooms = dto.NumberOfBathrooms.Value
                },
                "vacantland" => new VacantLandProperty
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Address = dto.Address,
                    OwnerId = dto.OwnerId,
                    YearOfCommissionOrPurchase = dto.YearOfCommissionOrPurchase ?? DateTime.UtcNow.Year,
                    FairValue = dto.FairValue ?? 0,
                    InsurableValue = dto.InsurableValue ?? 0,
                    OwnershipStatus = dto.OwnershipStatus,
                    SalePrice = dto.SalePrice,
                    IsRentable = dto.IsRentable ?? false,
                    IsSaleable = dto.IsSaleable ?? false,
                    RentPrice = dto.RentPrice,
                    BlockNumber = dto.BlockNumber,
                    PlotNumber = dto.PlotNumber,
                    ApprovalStatus = dto.ApprovalStatus,
                    Acreage = dto.Acreage.Value
                },
                _ => throw new ArgumentException($"Unsupported property type: {dto.PropertyType}", nameof(dto.PropertyType))
            };
        }
    }

    internal static class PropertyMapper
    {

        public static void UpdatePropertyFromDto(Property property, CreatePropertyDto dto)
        {
            property.Name = dto.Name;
            property.Address = dto.Address;
            property.OwnerId = dto.OwnerId;
            property.YearOfCommissionOrPurchase = dto.YearOfCommissionOrPurchase ?? property.YearOfCommissionOrPurchase;
            property.FairValue = dto.FairValue ?? property.FairValue;
            property.InsurableValue = dto.InsurableValue ?? property.InsurableValue;
            property.OwnershipStatus = dto.OwnershipStatus;
            property.SalePrice = dto.SalePrice ?? property.SalePrice;
            property.IsRentable = dto.IsRentable ?? property.IsRentable;
            property.IsSaleable = dto.IsSaleable ?? property.IsSaleable;
            property.RentPrice = dto.RentPrice ?? property.RentPrice;
            property.ApprovalStatus = dto.ApprovalStatus;
            property.NumberOfStories = dto.NumberOfStories ?? property.NumberOfStories;

            switch (property)
            {
                case CommercialProperty commercial:
                    commercial.NumberOfWings = dto.NumberOfWings ?? commercial.NumberOfWings;
                    commercial.TotalParkingAreaPerFloor = dto.TotalParkingAreaPerFloor ?? commercial.TotalParkingAreaPerFloor;
                    break;

                case CondominiumProperty condo:
                    condo.NumberOfUnitsPerFloor = dto.NumberOfUnitsPerFloor ?? condo.NumberOfUnitsPerFloor;
                    condo.HOAFees = dto.HOAFees ?? condo.HOAFees;
                    break;

                case TownhouseProperty townhouse:
                    townhouse.NumberOfClusters = dto.NumberOfClusters ?? townhouse.NumberOfClusters;
                    break;

                case BungalowProperty bungalow:
                    bungalow.NumberOfBedrooms = dto.NumberOfBedrooms ?? bungalow.NumberOfBedrooms;
                    bungalow.NumberOfBathrooms = dto.NumberOfBathrooms ?? bungalow.NumberOfBathrooms;
                    break;

                case VillaProperty villa:
                    villa.NumberOfBedrooms = dto.NumberOfBedrooms ?? villa.NumberOfBedrooms;
                    villa.NumberOfBathrooms = dto.NumberOfBathrooms ?? villa.NumberOfBathrooms;
                    break;

                case VacantLandProperty vacantLand:
                    vacantLand.BlockNumber = dto.BlockNumber;
                    vacantLand.PlotNumber = dto.PlotNumber;
                    vacantLand.Acreage = dto.Acreage ?? vacantLand.Acreage;
                    break;
            }
        }
    }

    internal class PropertyDtoValidator
    {
        public void Validate(CreatePropertyDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Property DTO cannot be null");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required and cannot be empty", nameof(dto.Name));

            if (string.IsNullOrWhiteSpace(dto.Address))
                throw new ArgumentException("Address is required and cannot be empty", nameof(dto.Address));

            /// if (dto.OwnerId == Guid.Empty)
            //     throw new ArgumentException("OwnerId must be a valid GUID", nameof(dto.OwnerId));

            if (string.IsNullOrWhiteSpace(dto.PropertyType))
                throw new ArgumentException("Type is required and cannot be empty", nameof(dto.PropertyType));

            //if (string.IsNullOrWhiteSpace(dto.OwnershipStatus))
            //    throw new ArgumentException("OwnershipStatus is required and cannot be empty", nameof(dto.OwnershipStatus));


            if (string.IsNullOrWhiteSpace(dto.ApprovalStatus))
                throw new ArgumentException("ApprovalStatus is required and cannot be empty", nameof(dto.ApprovalStatus));

            if (dto.NumberOfStories.HasValue && dto.NumberOfStories <= 0 && dto.PropertyType.ToLower() != "vacantland")
                throw new ArgumentException("NumberOfStories must be a positive number for building properties", nameof(dto.NumberOfStories));

            ValidateTypeSpecificFields(dto);
        }
        private void ValidateTypeSpecificFields(CreatePropertyDto dto)
        {
            switch (dto.PropertyType.ToLower())
            {
                case "commercial":
                    if (!dto.NumberOfWings.HasValue || dto.NumberOfWings <= 0)
                        throw new ArgumentException("NumberOfWings must be a positive number for Commercial properties", nameof(dto.NumberOfWings));
                    if (dto.Wings == null || !dto.Wings.Any())
                        throw new ArgumentException("At least one Wing is required for Commercial properties", nameof(dto.Wings));
                    foreach (var wing in dto.Wings)
                    {
                        if (string.IsNullOrWhiteSpace(wing.UsageType))
                            throw new ArgumentException("Usage is required for each Wing", nameof(wing.UsageType));
                        if (dto.NumberOfStories.HasValue && wing.FloorNumber >= dto.NumberOfStories)
                            throw new ArgumentException("Wing FloorNumber cannot exceed property NumberOfStories", nameof(wing.FloorNumber));
                    }
                    break;

                case "condominium":
                    if (!dto.NumberOfUnitsPerFloor.HasValue || dto.NumberOfUnitsPerFloor <= 0)
                        throw new ArgumentException("NumberOfUnitsPerFloor must be a positive number for Condominium properties", nameof(dto.NumberOfUnitsPerFloor));
                    if (dto.Units == null || !dto.Units.Any())
                        throw new ArgumentException("At least one Unit is required for Condominium properties", nameof(dto.Units));
                    foreach (var unit in dto.Units)
                    {
                        if (dto.NumberOfStories.HasValue && unit.FloorNumber >= dto.NumberOfStories)
                            throw new ArgumentException("Unit Floor cannot exceed property NumberOfStories", nameof(unit.FloorNumber));
                    }
                    break;

                case "townhouse":
                    if (!dto.NumberOfClusters.HasValue || dto.NumberOfClusters <= 0)
                        throw new ArgumentException("NumberOfClusters must be a positive number for Townhouse properties", nameof(dto.NumberOfClusters));
                    if (dto.Clusters == null || !dto.Clusters.Any())
                        throw new ArgumentException("At least one Cluster is required for Townhouse properties", nameof(dto.Clusters));
                    break;

                case "bungalow":
                case "villa":
                    if (!dto.NumberOfBedrooms.HasValue || dto.NumberOfBedrooms <= 0)
                        throw new ArgumentException("NumberOfBedrooms must be a positive number for Bungalow/Villa properties", nameof(dto.NumberOfBedrooms));
                    if (!dto.NumberOfBathrooms.HasValue || dto.NumberOfBathrooms <= 0)
                        throw new ArgumentException("NumberOfBathrooms must be a positive number for Bungalow/Villa properties", nameof(dto.NumberOfBathrooms));
                    break;

                case "vacantland":
                    if (string.IsNullOrWhiteSpace(dto.BlockNumber))
                        throw new ArgumentException("BlockNumber is required for VacantLand properties", nameof(dto.BlockNumber));
                    if (string.IsNullOrWhiteSpace(dto.PlotNumber))
                        throw new ArgumentException("PlotNumber is required for VacantLand properties", nameof(dto.PlotNumber));
                    if (!dto.Acreage.HasValue || dto.Acreage <= 0)
                        throw new ArgumentException("Acreage must be a positive number for VacantLand properties", nameof(dto.Acreage));
                    if (dto.NumberOfStories.HasValue && dto.NumberOfStories != 0)
                        throw new ArgumentException("NumberOfStories must be 0 for VacantLand properties", nameof(dto.NumberOfStories));
                    break;

                default:
                    throw new ArgumentException($"Unsupported property type: {dto.PropertyType}", nameof(dto.PropertyType));
            }
        }
    }
}
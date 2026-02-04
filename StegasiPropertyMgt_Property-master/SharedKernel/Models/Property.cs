using System;
using System.Collections.Generic;

namespace SharedKernel.Models
{
    public class Property
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public Guid OwnerId { get; set; }
        public string? PropertyManagerId { get; set; }
        public int YearOfCommissionOrPurchase { get; set; }
        public decimal FairValue { get; set; }
        public decimal InsurableValue { get; set; }
        public string OwnershipStatus { get; set; }
        public decimal? SalePrice { get; set; }
        public bool IsRentable { get; set; }
        public bool IsSaleable { get; set; }
        public decimal? RentPrice { get; set; }
        public string PropertyType { get; set; }
        public string ApprovalStatus { get; set; }
        public int NumberOfStories { get; set; }

        // Navigation properties
        public ICollection<LeaseAgreement> LeaseAgreements { get; set; }
        public ICollection<Amenity> Amenities { get; set; }
        public PropertyCertification? Certifications { get; set; }
        public PropertyCompliance? Compliance { get; set; }
        public PropertyRegulation? Regulations { get; set; }
        public PropertyStandard? Standards { get; set; }
        public PropertyFeatures? Features { get; set; }
        public PropertyService? Services { get; set; }

        public Property()
        {
            LeaseAgreements = new List<LeaseAgreement>();
            Amenities = new List<Amenity>();
        }
    }

    public class CommercialProperty : Property
    {
        public int NumberOfWings { get; set; }
        public decimal TotalParkingAreaPerFloor { get; set; }
        public ICollection<WingDetails> Wings { get; set; }

        public CommercialProperty()
        {
            Wings = new List<WingDetails>();
        }
    }

    public class CondominiumProperty : Property
    {
        public int NumberOfUnitsPerFloor { get; set; }
        public decimal HOAFees { get; set; }
        public ICollection<CondominiumUnit> Units { get; set; }

        public CondominiumProperty()
        {
            Units = new List<CondominiumUnit>();
        }
    }

    public class TownhouseProperty : Property
    {
        public int NumberOfClusters { get; set; }
        public ICollection<TownhouseCluster> Clusters { get; set; }

        public TownhouseProperty()
        {
            Clusters = new List<TownhouseCluster>();
        }
    }

    public class BungalowProperty : Property
    {
        public int NumberOfBedrooms { get; set; }
        public int NumberOfBathrooms { get; set; }
    }

    public class VillaProperty : Property
    {
        public int NumberOfBedrooms { get; set; }
        public int NumberOfBathrooms { get; set; }
    }

    public class VacantLandProperty : Property
    {
        public string BlockNumber { get; set; }
        public string PlotNumber { get; set; }
        public decimal Acreage { get; set; }
    }

    public class WingDetails
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string WingName { get; set; }
        public int FloorArea { get; set; }
        public int CommonArea { get; set; }
        public string UsageType { get; set; }
        public decimal RentalPrice { get; set; }
        public int FloorNumber { get; set; }
        public CommercialProperty? Property { get; set; }
    }

    public class CondominiumUnit
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string UnitNumber { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int FloorNumber { get; set; }
        public int SizeSquareFeet { get; set; }
        public decimal MonthlyRent { get; set; }
        public string UnitType { get; set; }
        public CondominiumProperty? Property { get; set; }
    }

    public class TownhouseCluster
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string ClusterName { get; set; }
        public int NumberOfUnits { get; set; }
        public decimal CommonAreaSize { get; set; }
        public TownhouseProperty? Property { get; set; }
    }

    public class PropertyCertification
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string Name { get; set; }
        public string IssuingAuthority { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string DocumentUrl { get; set; }
        public Property? Property { get; set; }
    }

    public class PropertyCompliance
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string RegulationName { get; set; }
        public string ComplianceStatus { get; set; }
        public DateTime LastInspectionDate { get; set; }
        public DateTime NextInspectionDate { get; set; }
        public string InspectionNotes { get; set; }
        public string Violations { get; set; }
        public string CorrectiveActions { get; set; }
        public Property? Property { get; set; }
    }

    public class PropertyRegulation
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Jurisdiction { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string ComplianceRequirements { get; set; }
        public string Penalties { get; set; }
        public string DocumentationRequirements { get; set; }
        public Property? Property { get; set; }
    }

    public class PropertyStandard
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Requirements { get; set; }
        public string Specifications { get; set; }
        public string ComplianceLevel { get; set; }
        public Property? Property { get; set; }
    }

    public class PropertyFeatures
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public bool HasParking { get; set; }
        public bool HasGarden { get; set; }
        public bool HasPool { get; set; }
        public bool HasGym { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasAirConditioning { get; set; }
        public bool HasHeating { get; set; }
        public bool HasInternet { get; set; }
        public bool IsFurnished { get; set; }
        public bool PetsAllowed { get; set; }
        public Property? Property { get; set; }
    }

    public class PropertyService
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string ServiceType { get; set; }
        public string Provider { get; set; }
        public decimal Cost { get; set; }
        public string Frequency { get; set; }
        public DateTime LastServiceDate { get; set; }
        public DateTime NextServiceDate { get; set; }
        public string Status { get; set; }
        public Property? Property { get; set; }
    }
} 
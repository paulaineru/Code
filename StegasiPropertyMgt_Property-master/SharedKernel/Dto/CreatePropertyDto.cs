namespace SharedKernel.Dto
{

    public class CreatePropertyDto
    {
        public string Name { get; set; } // Required
        public string Address { get; set; } // Required
        public Guid OwnerId { get; set; } // Required
        public string PropertyType { get; set; } // Required: "Commercial", "Condominium", "Townhouse", "Bungalow", "Villa", "VacantLand"

        public int? YearOfCommissionOrPurchase { get; set; } // Optional
        public decimal? FairValue { get; set; } // Optional
        public decimal? InsurableValue { get; set; } // Optional
        public string OwnershipStatus { get; set; } // Required: "Owned", "Leased", etc.
        public decimal? SalePrice { get; set; } // Optional
        public bool? IsRentable { get; set; } // Optional
        public bool? IsSaleable { get; set; } // Optional
        public decimal? RentPrice { get; set; } // Optional

        // Commercial-specific fields
        public int? NumberOfWings { get; set; } // Required for Commercial
        public List<WingDetailsDto>? Wings { get; set; } // Required for Commercial
        public decimal? TotalParkingAreaPerFloor { get; set; } // Optional for Commercial

        // Condominium-specific fields
        public int? NumberOfUnitsPerFloor { get; set; } // Required for Condominium
        public List<CondominiumUnitDto>? Units { get; set; } // Required for Condominium
        public decimal? HOAFees { get; set; } // Optional for Condominium

        // Townhouse-specific fields
        public int? NumberOfClusters { get; set; } // Required for Townhouse
        public List<TownhouseClusterDto>? Clusters { get; set; } // Required for Townhouse
        public List<Guid>? AmenityIds { get; set; }

        // Bungalow/Villa-specific fields
        public int? NumberOfBedrooms { get; set; } // Required for Bungalow/Villa
        public int? NumberOfBathrooms { get; set; } // Required for Bungalow/Villa

        // Vacant Land-specific fields
        public string? BlockNumber { get; set; } // Required for Vacant Land
        public string PlotNumber { get; set; } // Required for Vacant Land
        public decimal? Acreage { get; set; } // Required for Vacant Land

        public string? ApprovalStatus { get; set; }
        public int? NumberOfStories { get; set; }

    }
}

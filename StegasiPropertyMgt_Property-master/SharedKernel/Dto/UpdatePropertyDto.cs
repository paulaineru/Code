// SharedKernel/UpdatePropertyDto.cs

using SharedKernel.Models;
namespace SharedKernel.Dto
{
    public class UpdatePropertyDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int? YearOfCommissionOrPurchase { get; set; }
        public decimal? FairValue { get; set; }
        public decimal? InsurableValue { get; set; }
        public string OwnershipStatus { get; set; }
        public decimal? SalePrice { get; set; }
        public bool? IsRentable { get; set; }
        public bool? IsSaleable { get; set; }

        // Commercial-specific fields
        public int? NumberOfWings { get; set; }
        public List<WingDetailsDto> Wings { get; set; }
        public decimal? TotalParkingAreaPerFloor { get; set; }

        // Condominium-specific fields
        public int? NumberOfUnitsPerFloor { get; set; }
        public List<CondominiumUnitDto> Units { get; set; }
        public decimal? HOAFees { get; set; }

        // Townhouse-specific fields
        public int? NumberOfClusters { get; set; }
        public List<TownhouseClusterDto> Clusters { get; set; }

        // Bungalow/Villa-specific fields
        public int? NumberOfBedrooms { get; set; }
        public int? NumberOfBathrooms { get; set; }

        // Vacant Land-specific fields
        public string BlockNumber { get; set; }
        public string PlotNumber { get; set; }
        public decimal? Acreage { get; set; }

        // Common field
        public decimal? RentPrice { get; set; }
    }
}
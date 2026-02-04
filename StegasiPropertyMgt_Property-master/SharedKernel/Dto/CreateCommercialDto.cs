using Nest;
using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Dto
{
    public class CreateCommercialPropertyDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public Guid OwnerId { get; set; }
        public int? NumberOfWings { get; set; }
        public List<WingDetailsDto>? Wings { get; set; }
        public decimal? TotalParkingAreaPerFloor { get; set; }
    }
    public class CreateAmenityDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; }
        [StringLength(200)]
        public string Description { get; set; }
    }

    public class WingDetailsDto
    {
        public string WingName { get; set; }
        [Required]
        public int FloorArea { get; set; }
        public int CommonArea { get; set; }
        [Required]
        public string UsageType { get; set; }
        public decimal RentalPrice { get; set; }
        [Required]
        public int FloorNumber { get; set; }
    }
    public class CreateCondominiumPropertyDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public Guid OwnerId { get; set; }
        public int? NumberOfUnitsPerFloor { get; set; }
        public List<CondominiumUnitDto> Units { get; set; }
        public decimal? HOAFees { get; set; }
        public string TitleDeed { get; set; }
        public string Bylaws { get; set; }
        public string LeaseAgreement { get; set; }
    }

    public class CondominiumUnitDto
    {
        public string UnitNumber { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int FloorNumber {get;set;}
        public int UnitSize { get; set; }
        public decimal MonthlyRent {get;set;}
        public string UnitType { get; set; } 
    }
    public class CreateTownhousePropertyDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public Guid OwnerId { get; set; }
        public int? NumberOfClusters { get; set; }
        public List<TownhouseClusterDto> Clusters { get; set; }
    }

    public class TownhouseClusterDto
    {
        public string ClusterName { get; set; }
        public string ClusterType { get; set; }
        public string? Title { get; set; }
        public decimal ClusterSizeSquareFeet { get; set; }
        public int? NumberOfStories { get; set; }
        
        public decimal MonthlyRent { get; set; }
    }
}
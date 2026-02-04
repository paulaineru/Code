namespace SharedKernel.Dto
{
    public class PropertyDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string PropertyManagerId { get; set; }

        public int YearOfCommissionOrPurchase { get; set; }
        public decimal FairValue { get; set; }
        public decimal SalePrice { get; set; }
        public decimal InsurableValue { get; set; }
        public string OwnershipStatus { get; set; }
        public Guid OwnerId { get; set; }
        public string Type { get; set; } // Discriminator property (e.g., "Commercial", "Condominium")
        public decimal? RentPrice { get; set; }
        public bool IsRentable { get; set; }
        public List<string> Images { get; set; } = new();
        public string? PrimaryImageUrl { get; set; }
    }
}
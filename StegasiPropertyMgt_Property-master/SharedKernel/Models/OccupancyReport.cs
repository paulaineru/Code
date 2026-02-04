// SharedKernel/OccupancyReport.cs
namespace SharedKernel.Models
{
    public class OccupancyReport
    {
        public Guid PropertyId { get; set; }
        public string PropertyName { get; set; }
        public int TotalUnits { get; set; }
        public int OccupiedUnits { get; set; }
        public decimal OccupancyRate => TotalUnits > 0 ? (OccupiedUnits / (decimal)TotalUnits) * 100 : 0;
    }
    public class FinancialReport
{
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal OutstandingAmounts { get; set; }
    public decimal MaintenanceCosts { get; set; }
    public decimal NetProfit => TotalRevenue - MaintenanceCosts - OutstandingAmounts;
}
}



using SharedKernel.Models;
using SharedKernel.Models.Billing;
namespace StegasiPropertyMgt.SharedKernel.Dto
{
    public class SetBillingScheduleDto
    {
        public DateTime StartDate { get; set; } // Start of the financial year
        public DateTime EndDate { get; set; } // End of the financial year
        public Frequency Frequency { get; set; } // Billing frequency (e.g., Quarterly)
        public decimal BaseRate { get; set; } // Base rate per unit area
        public List<CustomBillingFieldDto> CustomFields { get; set; } // Additional custom billing fields
    }

    public class CustomBillingFieldDto
    {
        public string FieldName { get; set; }
        public decimal Value { get; set; }
    }
}
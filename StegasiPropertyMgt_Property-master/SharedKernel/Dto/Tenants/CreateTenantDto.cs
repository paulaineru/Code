using System.Collections.Generic;
using SharedKernel.Models;
using SharedKernel.Dto.Tenants;

namespace SharedKernel.Dto.Tenants
{
    public class CreateTenantDto
    {
        public string Name { get; set; }
        public string PrimaryEmail { get; set; }
        public string PrimaryTelephone { get; set; }
        public string? BusinessRegistrationNumber { get; set; }
        public string TaxIdentificationNumber { get; set; }
        public TenantType TenantType { get; set; } = TenantType.CorporateOrganisation;
        public string BillingEntity { get; set; }
        public List<ContactDetailDto> Contacts { get; set; } = new();
    }
}
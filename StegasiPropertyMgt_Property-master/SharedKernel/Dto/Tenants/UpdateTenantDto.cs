// SharedKernel/Models/UpdateTenantDto.cs
using System.Collections.Generic;
using SharedKernel.Dto;
namespace SharedKernel.Dto.Tenants
{
    public class UpdateTenantDto
    {
        public string Name { get; set; } // Updated tenant name
        public string Email { get; set; } // Updated tenant email
        public string TaxIdentificationNumber { get; set; } // Updated tax ID
        public string BillingEntity { get; set; } // Updated billing entity
        public List<ContactDetailDto> Contacts { get; set; } = new(); // Updated contact details
    }
}
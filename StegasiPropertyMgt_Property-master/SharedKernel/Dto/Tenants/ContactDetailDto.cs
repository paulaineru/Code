namespace SharedKernel.Dto.Tenants
{
    public class ContactDetailDto
    {
        public string Type { get; set; } // e.g., "Phone", "Email"
        public string Value { get; set; } // e.g., "+1234567890", "tenant@example.com"
    }
}
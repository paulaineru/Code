namespace SharedKernel.Models
{
    public class ContactDetail
    {
        public Guid Id { get; set; } 
        public string Type { get; set; }
        public string Value { get; set; }
        public Guid TenantId { get; set; }
    }
}
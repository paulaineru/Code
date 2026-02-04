

using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Dto
{
    public class NotificationRequest
    {

        [Required]
        public Guid TenantId { get; set; }
        public string Recipient { get; set; }
        public string Subject { get; set; }
        [Required]
        public string Message { get; set; }
        public Guid? PropertyId { get; set; } 

        public string Type { get; set; } = "General";
    }
}
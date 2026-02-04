// SharedKernel/Email.cs
namespace SharedKernel.Models
{
    public class Email
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Recipient { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
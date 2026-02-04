namespace SharedKernel.Models
{
    public class NotificationLog
    {
        public int Id { get; set; }
        public string NotificationType { get; set; }
        public string NotificationMessage { get; set; }
        public string NotificationStatus { get; set; }
        public DateTime NotificationDate { get; set; }
    }
}
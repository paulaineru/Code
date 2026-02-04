// SharedKernel/Services/EmailService.cs
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace SharedKernel.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailService(
            string smtpHost = "smtp.example.com", 
            int smtpPort = 587, 
            string smtpUsername = "your-email@example.com", 
            string smtpPassword = "your-password")
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _smtpUsername = smtpUsername;
            _smtpPassword = smtpPassword;
        }
        public async Task SendEmailAsync(string recipient, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.example.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("your-email@example.com", "your-password"),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage("from@example.com", recipient, subject, body);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to send email", ex);
            }
        }
    }
}
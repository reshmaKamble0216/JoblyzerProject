using JobLyzerProject_Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace JoblyzerProject_Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendRegistrationEmailAsync(string toEmail, string fullName, string role)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["EmailSettings:Port"] ?? "587");
            var senderName = _config["EmailSettings:SenderName"] ?? "JobLyzer Portal";
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var password = _config["EmailSettings:Password"]?.Replace(" ", "");

            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(password))
                return;

            using var client = new SmtpClient(smtpServer, port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 10000
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = $"Welcome to JobLyzer, {fullName}!",
                Body = $@"
                    <h2>Welcome to JobLyzer Portal!</h2>
                    <p>Hi <strong>{fullName}</strong>,</p>
                    <p>Your registration as a <strong>{role}</strong> has been completed successfully.</p>
                    <p>You can now log in to your dashboard and explore opportunities.</p>
                    <br/>
                    <p>Best Regards,<br/><strong>JobLyzer Team</strong></p>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await client.SendMailAsync(mailMessage);
        }
    }
}

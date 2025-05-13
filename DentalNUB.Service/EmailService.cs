using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using DentalNUB.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DentalNUB.Interface;

namespace DentalNUB.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendVerificationEmail(string email, string code)
        {
            var mailSettings = _config.GetSection("MailSettings").Get<MailSettings>();

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(mailSettings.DisplayName, mailSettings.Mail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Your Verification Code";
            message.Body = new TextPart("plain")
            {
                Text = $"Your verification code is: {code}. It expires in 10 minutes."
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(mailSettings.Host, mailSettings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(mailSettings.Mail, mailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
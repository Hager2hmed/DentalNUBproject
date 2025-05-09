
//using Microsoft.Extensions.Options;
//using System.Net.Mail;

//namespace DentalNUB.Api.Services;

//public class EmailService : IEmailService
//{
//    private readonly EmailSettings _emailSettings;
//    public EmailService(IOptions<EmailSettings> emailSettings)
//    {
//        _emailSettings = emailSettings.Value;
//    }
//    public async Task SendVerificationEmailAsync(string toEmail, string verificationCode)
//    {
//        var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
//        {
//            Port = _emailSettings.SmtpPort,
//            Credentials = new System.Net.NetworkCredential(_emailSettings.SenderEmail, _emailSettings.AppPassword),
//            EnableSsl = true,
//        };

//        var mailMessage = new MailMessage
//        {
//            From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
//            Subject = "Verification Code for HealthCare App",
//            Body = $"Your verification code is: <strong>{verificationCode}</strong>. It will expire in 30 minutes.",
//            IsBodyHtml = true,
//        };

//        mailMessage.To.Add(toEmail); // هنا بيبعت لإيميل المستخدم

//        await smtpClient.SendMailAsync(mailMessage);
//    }

//}


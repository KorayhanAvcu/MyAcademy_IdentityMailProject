using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace IdentityMail.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string to,
            string subject,
            string body)
        {
            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    _configuration["EmailSettings:SenderName"],
                    _configuration["EmailSettings:SenderEmail"]));

            email.To.Add(
                MailboxAddress.Parse(to));

            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };

            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _configuration["EmailSettings:SmtpServer"],
                int.Parse(_configuration["EmailSettings:SmtpPort"]!),
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _configuration["EmailSettings:SenderEmail"],
                _configuration["EmailSettings:Password"]);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}
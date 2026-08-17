using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace IdentityMail.Web.Services
{
    // SMTP üzerinden email gönderme işlemini gerçekleştirir.
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        // Email ayarlarını appsettings.json üzerinden alır.
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Belirtilen adrese email gönderir.
        public async Task SendEmailAsync(
            string to,
            string subject,
            string body)
        {
            // Yeni email mesajı oluştur.
            var email = new MimeMessage();

            // Gönderen bilgilerini ayarlardan al.
            email.From.Add(
                new MailboxAddress(
                    _configuration["EmailSettings:SenderName"],
                    _configuration["EmailSettings:SenderEmail"]));

            // Alıcı email adresini ekle.
            email.To.Add(
                MailboxAddress.Parse(to));

            // Email konusunu belirle.
            email.Subject = subject;

            // Email içeriğini HTML olarak oluştur.
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };

            email.Body = bodyBuilder.ToMessageBody();

            // SMTP istemcisi oluştur.
            using var smtp = new SmtpClient();

            // SMTP sunucusuna güvenli bağlantı kur.
            await smtp.ConnectAsync(
                _configuration["EmailSettings:SmtpServer"],
                int.Parse(_configuration["EmailSettings:SmtpPort"]!),
                SecureSocketOptions.StartTls);

            // SMTP hesabıyla giriş yap.
            await smtp.AuthenticateAsync(
                _configuration["EmailSettings:SenderEmail"],
                _configuration["EmailSettings:Password"]);

            // Emaili gönder.
            await smtp.SendAsync(email);

            // SMTP bağlantısını güvenli şekilde kapat.
            await smtp.DisconnectAsync(true);
        }
    }
}
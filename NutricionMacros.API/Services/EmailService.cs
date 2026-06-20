using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace NutricionMacros.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCorreoAsync(string para, string asunto, string mensajeHtml)
        {
            var email = new MimeMessage();
            
            email.From.Add(MailboxAddress.Parse(_config["EmailSettings__Username"]));
            email.To.Add(MailboxAddress.Parse(para));
            email.Subject = asunto;

            var bodyBuilder = new BodyBuilder { HtmlBody = mensajeHtml };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            // CAMBIO CRÍTICO: Puerto 465 y SslOnConnect
            await smtp.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);

            await smtp.AuthenticateAsync(_config["EmailSettings__Username"], _config["EmailSettings__Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
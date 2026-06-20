using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NutricionMacros.API.Services
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

        public async Task EnviarCorreoAsync(string para, string asunto, string mensajeHtml)
        {
            try
            {
                var email = new MimeMessage();
                var fromEmail = _config["EmailSettings:Username"];
                var password = _config["EmailSettings:Password"];

                _logger.LogInformation($"📧 Intentando enviar email a: {para}");
                _logger.LogInformation($"📧 Desde: {fromEmail}");

                email.From.Add(MailboxAddress.Parse(fromEmail));
                email.To.Add(MailboxAddress.Parse(para));
                email.Subject = asunto;

                var bodyBuilder = new BodyBuilder { HtmlBody = mensajeHtml };
                email.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();

                _logger.LogInformation("🔐 Conectando a SMTP...");
                // CAMBIO: Puerto 587 con StartTlsAsync
                await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                _logger.LogInformation("🔐 Autenticando...");
                await smtp.AuthenticateAsync(fromEmail, password);

                _logger.LogInformation("📤 Enviando email...");
                await smtp.SendAsync(email);

                _logger.LogInformation("✅ Email enviado exitosamente");
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al enviar email: {ex.Message}");
                _logger.LogError($"❌ Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
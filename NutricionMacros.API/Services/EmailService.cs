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

                email.From.Add(MailboxAddress.Parse(fromEmail));
                email.To.Add(MailboxAddress.Parse(para));
                email.Subject = asunto;

                var bodyBuilder = new BodyBuilder { HtmlBody = mensajeHtml };
                email.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                smtp.Timeout = 30000; // 30 segundos (como Spring Boot)

                _logger.LogInformation("🔐 Conectando a SMTP en puerto 587...");
                await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                _logger.LogInformation("🔐 Autenticando con Gmail...");
                await smtp.AuthenticateAsync(fromEmail, password);

                _logger.LogInformation("📤 Enviando email...");
                await smtp.SendAsync(email);

                _logger.LogInformation("✅ Email enviado exitosamente a: {para}", para);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al enviar email a {para}: {ex.Message}");
                _logger.LogError($"❌ Detalles: {ex.StackTrace}");
                // ⚠️ NO relanzamos la excepción para que no bloquee la respuesta
                // El email fallará pero el usuario recibirá respuesta
            }
        }
    }
}
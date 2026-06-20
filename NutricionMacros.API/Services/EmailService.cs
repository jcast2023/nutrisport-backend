using SendGrid;
using SendGrid.Helpers.Mail;
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
                var apiKey = _config["SendGrid:ApiKey"];
                var fromEmail = _config["EmailSettings:Username"];

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(fromEmail, "NutricionMacros");
                var to = new EmailAddress(para);

                var msg = new SendGridMessage()
                {
                    From = from,
                    Subject = asunto,
                    HtmlContent = mensajeHtml
                };
                msg.AddTo(to);

                _logger.LogInformation($"📧 Enviando email a {para} vía SendGrid...");

                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    _logger.LogInformation($"✅ Email enviado exitosamente a {para}");
                }
                else
                {
                    _logger.LogError($"❌ SendGrid respondió con: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error enviando email: {ex.Message}");
                _logger.LogError($"❌ Detalles: {ex.StackTrace}");
            }
        }
    }
}
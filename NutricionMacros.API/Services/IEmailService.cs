namespace NutricionMacros.API.Services
{
    public interface IEmailService
    {
        Task EnviarCorreoAsync(string para, string asunto, string mensajeHtml);
    }
}
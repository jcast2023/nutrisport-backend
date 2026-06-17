using System;
namespace NutricionMacros.API.DTOs
{
    public class CitaResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NombrePaciente { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Notas { get; set; }
    }
}
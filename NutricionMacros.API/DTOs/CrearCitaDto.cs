using System;
using System.ComponentModel.DataAnnotations;

namespace NutricionMacros.API.DTOs
{
    public class CrearCitaDto
    {
        [Required(ErrorMessage = "La fecha y hora de la cita son obligatorias.")]
        public DateTime FechaHora { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden superar los 500 caracteres.")]
        public string? Notas { get; set; }
    }
}
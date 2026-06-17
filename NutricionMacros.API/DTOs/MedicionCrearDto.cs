using System.ComponentModel.DataAnnotations;

namespace NutricionMacros.API.DTOs
{
    public class MedicionCrearDto
    {
        [Required]
        public decimal Peso { get; set; }

        [Required]
        public decimal Talla { get; set; }

        public decimal? CinturasCm { get; set; }
        public decimal? CaderaCm { get; set; }
        public decimal? PorcentajeGrasa { get; set; }
        public decimal? MasaMuscular { get; set; }
        public string? Notas { get; set; }
    }
}
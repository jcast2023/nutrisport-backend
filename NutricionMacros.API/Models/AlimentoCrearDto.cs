using System.ComponentModel.DataAnnotations;

namespace NutricionMacros.API.Models
{
    public class AlimentoCrearDto
    {
        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Range(0, 2000, ErrorMessage = "Las calorías deben ser un valor positivo.")]
        public int Calorias { get; set; } // Por cada 100g

        [Required]
        public decimal Proteinas { get; set; }

        [Required]
        public decimal Carbohidratos { get; set; }

        [Required]
        public decimal Grasas { get; set; }
    }
}
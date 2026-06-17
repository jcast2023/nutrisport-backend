using System.ComponentModel.DataAnnotations;

namespace NutricionMacros.API.Models
{
    public class ConsumoCrearDto
    {
        [Required]
        public int AlimentoId { get; set; }

        [Required]
        [Range(1, 5000, ErrorMessage = "Los gramos consumidos deben ser mayores a 0.")]
        public int GramosConsumidos { get; set; }

        [Required]
        public string ComidaTipo { get; set; } = "Desayuno"; // Desayuno, Almuerzo, Cena, Snack
    }
}
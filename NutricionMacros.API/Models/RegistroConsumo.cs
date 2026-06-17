using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutricionMacros.API.Models
{
    public class RegistroConsumo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int AlimentoId { get; set; }

        [Required]
        public int GramosConsumidos { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [MaxLength(50)]
        public string ComidaTipo { get; set; } = "Desayuno"; // Desayuno, Almuerzo, etc.

        // Propiedades de navegación de Entity Framework
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [ForeignKey("AlimentoId")]
        public Alimento? Alimento { get; set; }
    }
}
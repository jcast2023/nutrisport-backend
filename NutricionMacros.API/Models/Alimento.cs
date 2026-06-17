using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutricionMacros.API.Models
{
    public class Alimento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public int Calorias { get; set; } // Por cada 100g

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Proteinas { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Carbohidratos { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Grasas { get; set; }
    }
}
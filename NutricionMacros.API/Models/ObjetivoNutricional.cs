using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutricionMacros.API.Models
{
    public class ObjetivoNutricional
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int CaloriasObjetivo { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ProteinasObjetivo { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CarbohidratosObjetivo { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrasasObjetivo { get; set; }

        [Required]
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

        // Relación con el Usuario
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
    }
}
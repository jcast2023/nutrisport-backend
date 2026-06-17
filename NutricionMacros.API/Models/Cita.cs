using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutricionMacros.API.Models
{
    public class Cita
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente";

        public string? Notas { get; set; }

        // Propiedad de navegación — necesaria para el Include y el Select
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
    }
}
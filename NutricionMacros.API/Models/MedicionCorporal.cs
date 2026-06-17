using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutricionMacros.API.Models
{
    public class MedicionCorporal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Peso { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Talla { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? CinturasCm { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? CaderaCm { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? PorcentajeGrasa { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? MasaMuscular { get; set; }

        public string? Notas { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        // IMC calculado automáticamente
        [NotMapped]
        public decimal IMC => Talla > 0 ? Math.Round(Peso / ((Talla / 100) * (Talla / 100)), 2) : 0;

        [NotMapped]
        public string CategoriaIMC => IMC switch
        {
            < 18.5m => "Bajo peso",
            < 25m => "Normal",
            < 30m => "Sobrepeso",
            _ => "Obesidad"
        };
    }
}

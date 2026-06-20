using System;
using System.ComponentModel.DataAnnotations;

namespace NutricionMacros.API.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(30)]
        public string Rol { get; set; } = "Atleta"; 

        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpiracion { get; set; }
    }
}
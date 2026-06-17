using Microsoft.EntityFrameworkCore;
using NutricionMacros.API.Models;

namespace NutricionMacros.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Alimento> Alimentos { get; set; }
        public DbSet<RegistroConsumo> RegistrosConsumos { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<ObjetivoNutricional> ObjetivosNutricionales { get; set; }
        public DbSet<MedicionCorporal> MedicionesCorporales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cita>()
                .Property(c => c.FechaHora)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<ObjetivoNutricional>()
                .Property(o => o.FechaAsignacion)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<RegistroConsumo>()
                .Property(r => r.Fecha)
                .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<MedicionCorporal>()
                .ToTable("MedicionesCorporales")
                .Property(m => m.Fecha)
                .HasColumnType("timestamp without time zone");

        }

    }
}
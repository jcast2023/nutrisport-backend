using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutricionMacros.API.Data;
using NutricionMacros.API.DTOs;
using NutricionMacros.API.Models;
using System.Security.Claims;

namespace NutricionMacros.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MedicionesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Mediciones — paciente registra medición
        [HttpPost]
        public async Task<IActionResult> RegistrarMedicion([FromBody] MedicionCrearDto dto)
        {
            var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdStr == null) return Unauthorized();

            var medicion = new MedicionCorporal
            {
                UsuarioId = int.Parse(usuarioIdStr),
                Fecha = DateTime.Now,
                Peso = dto.Peso,
                Talla = dto.Talla,
                CinturasCm = dto.CinturasCm,
                CaderaCm = dto.CaderaCm,
                PorcentajeGrasa = dto.PorcentajeGrasa,
                MasaMuscular = dto.MasaMuscular,
                Notas = dto.Notas
            };

            _context.MedicionesCorporales.Add(medicion);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Medición registrada correctamente.",
                imc = medicion.IMC,
                categoriaIMC = medicion.CategoriaIMC
            });
        }

        // GET: api/Mediciones/MiHistorial — paciente ve su historial
        [HttpGet("MiHistorial")]
        public async Task<IActionResult> GetMiHistorial()
        {
            var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdStr == null) return Unauthorized();

            var id = int.Parse(usuarioIdStr);

            var mediciones = await _context.MedicionesCorporales
                .Where(m => m.UsuarioId == id)
                .OrderByDescending(m => m.Fecha)
                .Select(m => new
                {
                    m.Id,
                    m.Fecha,
                    m.Peso,
                    m.Talla,
                    m.CinturasCm,
                    m.CaderaCm,
                    m.PorcentajeGrasa,
                    m.MasaMuscular,
                    m.Notas,
                    IMC = m.Talla > 0 ? Math.Round(m.Peso / ((m.Talla / 100) * (m.Talla / 100)), 2) : 0
                })
                .ToListAsync();

            return Ok(mediciones);
        }

        // GET: api/Mediciones/Paciente/{usuarioId} — admin ve historial de un paciente
        [HttpGet("Paciente/{usuarioId}")]
        [Authorize(Roles = "Nutricionista")]
        public async Task<IActionResult> GetHistorialPaciente(int usuarioId)
        {
            var mediciones = await _context.MedicionesCorporales
                .Where(m => m.UsuarioId == usuarioId)
                .OrderByDescending(m => m.Fecha)
                .Select(m => new
                {
                    m.Id,
                    m.Fecha,
                    m.Peso,
                    m.Talla,
                    m.CinturasCm,
                    m.CaderaCm,
                    m.PorcentajeGrasa,
                    m.MasaMuscular,
                    m.Notas,
                    IMC = m.Talla > 0 ? Math.Round(m.Peso / ((m.Talla / 100) * (m.Talla / 100)), 2) : 0
                })
                .ToListAsync();

            return Ok(mediciones);
        }
        // POST: api/Mediciones/RegistrarParaPaciente/{usuarioId} — solo Nutricionista
        [HttpPost("RegistrarParaPaciente/{usuarioId}")]
        [Authorize(Roles = "Nutricionista")]
        public async Task<IActionResult> RegistrarParaPaciente(int usuarioId, [FromBody] MedicionCrearDto dto)
        {
            var pacienteExiste = await _context.Usuarios.AnyAsync(u => u.Id == usuarioId);
            if (!pacienteExiste) return NotFound(new { mensaje = "Paciente no encontrado." });

            var medicion = new MedicionCorporal
            {
                UsuarioId = usuarioId,
                Fecha = DateTime.Now,
                Peso = dto.Peso,
                Talla = dto.Talla,
                CinturasCm = dto.CinturasCm,
                CaderaCm = dto.CaderaCm,
                PorcentajeGrasa = dto.PorcentajeGrasa,
                MasaMuscular = dto.MasaMuscular,
                Notas = dto.Notas
            };

            _context.MedicionesCorporales.Add(medicion);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Medición registrada correctamente.",
                imc = medicion.IMC,
                categoriaIMC = medicion.CategoriaIMC
            });
        }

        // DELETE: api/Mediciones/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarMedicion(int id)
        {
            var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdStr == null) return Unauthorized();

            var medicion = await _context.MedicionesCorporales
                .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == int.Parse(usuarioIdStr));

            if (medicion == null) return NotFound();

            _context.MedicionesCorporales.Remove(medicion);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Medición eliminada correctamente." });
        }
    }
}

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
    public class CitasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Citas
        [HttpPost]
        public async Task<IActionResult> AgendarCita([FromBody] CrearCitaDto dto)
        {
            try
            {
                var usuarioIdIdentificado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (usuarioIdIdentificado == null) return Unauthorized();

                var nuevaCita = new Cita
                {
                    UsuarioId = int.Parse(usuarioIdIdentificado),
                    FechaHora = dto.FechaHora,
                    Notas = dto.Notas,
                    Estado = "Pendiente"
                };

                _context.Citas.Add(nuevaCita);
                await _context.SaveChangesAsync();

                var usuario = await _context.Usuarios.FindAsync(nuevaCita.UsuarioId);

                var respuesta = new CitaResponseDto
                {
                    Id = nuevaCita.Id,
                    UsuarioId = nuevaCita.UsuarioId,
                    NombrePaciente = $"{usuario!.Nombre} {usuario.Apellido}",
                    FechaHora = nuevaCita.FechaHora,
                    Estado = nuevaCita.Estado,
                    Notas = nuevaCita.Notas
                };

                return Ok(new { mensaje = "Cita agendada correctamente", cita = respuesta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = ex.Message,
                    inner = ex.InnerException?.Message,
                    detalle = ex.StackTrace
                });
            }
        }

        // GET: api/Citas/MisCitas
        [HttpGet("MisCitas")]
        public async Task<IActionResult> GetMisCitas()
        {
            var usuarioIdIdentificado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdIdentificado == null) return Unauthorized();

            var id = int.Parse(usuarioIdIdentificado);

            var citas = await _context.Citas
                .Where(c => c.UsuarioId == id)
                .Include(c => c.Usuario)
                .OrderBy(c => c.FechaHora)
                .Select(c => new CitaResponseDto
                {
                    Id = c.Id,
                    UsuarioId = c.UsuarioId,
                    NombrePaciente = $"{c.Usuario!.Nombre} {c.Usuario.Apellido}",
                    FechaHora = c.FechaHora,
                    Estado = c.Estado,
                    Notas = c.Notas
                })
                .ToListAsync();

            return Ok(citas);
        }

        // GET: api/Citas/TodasLasCitas — solo Nutricionista
        [HttpGet("TodasLasCitas")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetTodasLasCitas()
        {
            var citas = await _context.Citas
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaHora)
                .Select(c => new CitaResponseDto
                {
                    Id = c.Id,
                    UsuarioId = c.UsuarioId,
                    NombrePaciente = $"{c.Usuario!.Nombre} {c.Usuario.Apellido}",
                    FechaHora = c.FechaHora,
                    Estado = c.Estado,
                    Notas = c.Notas
                })
                .ToListAsync();

            return Ok(citas);
        }

        // PUT: api/Citas/{id}/estado — solo Nutricionista
        [HttpPut("{id}/estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound(new { mensaje = "Cita no encontrada" });

            var estadosValidos = new[] { "Pendiente", "Confirmada", "Cancelada" };
            if (!estadosValidos.Contains(dto.Estado))
                return BadRequest(new { mensaje = "Estado no válido" });

            cita.Estado = dto.Estado;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Estado actualizado a {dto.Estado}" });
        }
    }
}
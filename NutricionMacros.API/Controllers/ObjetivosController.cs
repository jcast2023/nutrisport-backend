using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutricionMacros.API.Data;
using NutricionMacros.API.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NutricionMacros.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ObjetivosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ObjetivosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Objetivos (Asignar meta - El Nutricionista)
        [HttpPost]
        [Authorize(Roles = "Nutricionista")]
        public async Task<IActionResult> AsignarObjetivo([FromBody] ObjetivoNutricional objetivo)
        {
            objetivo.FechaAsignacion = DateTime.UtcNow;
            _context.ObjetivosNutricionales.Add(objetivo);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Objetivo nutricional asignado con éxito", objetivo });
        }

        // GET: api/Objetivos/Paciente/5 (El paciente o nutricionista ve la meta actual)
        [HttpGet("Paciente/{usuarioId}")]
        public async Task<IActionResult> GetObjetivoActual(int usuarioId)
        {
            var objetivo = await _context.ObjetivosNutricionales
                .Where(o => o.UsuarioId == usuarioId)
                .OrderByDescending(o => o.FechaAsignacion)
                .FirstOrDefaultAsync();

            if (objetivo == null) return NotFound(new { mensaje = "El paciente aún no tiene metas fijadas." });

            return Ok(objetivo);
        }

        // GET: api/Objetivos/MiObjetivo — el paciente ve su propio objetivo
        [HttpGet("MiObjetivo")]
        public async Task<IActionResult> GetMiObjetivo()
        {
            var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdStr == null) return Unauthorized();

            var usuarioId = int.Parse(usuarioIdStr);

            var objetivo = await _context.ObjetivosNutricionales
                .Where(o => o.UsuarioId == usuarioId)
                .OrderByDescending(o => o.FechaAsignacion)
                .FirstOrDefaultAsync();

            if (objetivo == null)
                return NotFound(new { mensaje = "Aún no tienes metas asignadas." });

            return Ok(objetivo);
        }
    }
}
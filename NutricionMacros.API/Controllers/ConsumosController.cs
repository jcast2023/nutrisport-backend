using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutricionMacros.API.Data;
using NutricionMacros.API.DTOs; // 👈 Importamos tus DTOs para el resumen
using NutricionMacros.API.Models;

namespace NutricionMacros.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protegido por JWT
    public class ConsumosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConsumosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. REGISTRAR UN CONSUMO: POST /api/Consumos
        [HttpPost]
        public async Task<IActionResult> RegistrarConsumo([FromBody] ConsumoCrearDto request)
        {
            // Extraer de forma segura el ID del usuario desde el Token JWT
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out int usuarioId))
            {
                return Unauthorized("Token inválido o usuario no identificado.");
            }

            // Validar si el alimento que intenta registrar existe en el catálogo
            var alimentoExiste = await _context.Alimentos.AnyAsync(a => a.Id == request.AlimentoId);
            if (!alimentoExiste)
            {
                return BadRequest("El alimento especificado no existe.");
            }

            var nuevoConsumo = new RegistroConsumo
            {
                UsuarioId = usuarioId,
                AlimentoId = request.AlimentoId,
                GramosConsumidos = request.GramosConsumidos,
                ComidaTipo = request.ComidaTipo,
                Fecha = DateTime.UtcNow // Se registra con la fecha y hora actual en UTC
            };

            _context.RegistrosConsumos.Add(nuevoConsumo);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Consumo diario registrado exitosamente en Supabase." });
        }

        // 2. OBTENER EL HISTORIAL DIARIO DEL USUARIO AUTENTICADO: GET /api/Consumos/historial
        [HttpGet("historial")]
        public async Task<IActionResult> ObtenerHistorialDiario()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out int usuarioId))
                return Unauthorized();

            var hoy = DateTime.UtcNow.Date; // 👈 filtro por hoy

            var historial = await _context.RegistrosConsumos
                .Where(c => c.UsuarioId == usuarioId && c.Fecha.Date == hoy) // 👈 solo hoy
                .Include(c => c.Alimento)
                .OrderByDescending(c => c.Fecha)
                .Select(c => new
                {
                    c.Id,
                    c.GramosConsumidos,
                    ComidaTipo = c.ComidaTipo,
                    c.Fecha,
                    Alimento = c.Alimento != null ? c.Alimento.Nombre : "Desconocido",
                    CaloriasTotales = c.Alimento != null ? (c.GramosConsumidos * c.Alimento.Calorias) / 100 : 0,
                    ProteinasTotales = c.Alimento != null ? (c.GramosConsumidos * c.Alimento.Proteinas) / 100 : 0,
                    CarbohidratosTotales = c.Alimento != null ? (c.GramosConsumidos * c.Alimento.Carbohidratos) / 100 : 0,
                    GrasasTotales = c.Alimento != null ? (c.GramosConsumidos * c.Alimento.Grasas) / 100 : 0
                })
                .ToListAsync();

            return Ok(historial);
        }

        // 🔥 3. NUEVO: OBTENER EL RESUMEN TOTAL DE METRICAS DE HOY: GET /api/Consumos/ResumenHoy
        [HttpGet("ResumenHoy")]
        public async Task<IActionResult> GetResumenHoy()
        {
            // Extraer el ID del usuario desde el Token JWT
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out int usuarioId))
            {
                return Unauthorized();
            }

            // Capturar el rango de fecha para el día de hoy en formato UTC corto
            var hoy = DateTime.UtcNow.Date;

            // Traer de Supabase los consumos que correspondan solo al día de hoy
            var consumosDeHoy = await _context.RegistrosConsumos
                .Include(c => c.Alimento)
                .Where(c => c.UsuarioId == usuarioId && c.Fecha.Date == hoy)
                .ToListAsync();

            // Realizar los cálculos acumulados aplicando la regla de tres simple por cada alimento
            var resumen = new ResumenConsumoDto
            {
                TotalCalorias = consumosDeHoy.Sum(c => c.Alimento != null ? (c.GramosConsumidos * c.Alimento.Calorias) / 100 : 0),
                TotalProteinas = consumosDeHoy.Sum(c => c.Alimento != null ? (c.GramosConsumidos * (decimal)c.Alimento.Proteinas) / 100 : 0),
                TotalCarbohidratos = consumosDeHoy.Sum(c => c.Alimento != null ? (c.GramosConsumidos * (decimal)c.Alimento.Carbohidratos) / 100 : 0),
                TotalGrasas = consumosDeHoy.Sum(c => c.Alimento != null ? (c.GramosConsumidos * (decimal)c.Alimento.Grasas) / 100 : 0)
            };

            return Ok(resumen);
        }


        // 4. ELIMINAR UN CONSUMO: DELETE /api/Consumos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarConsumo(int id)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out int usuarioId))
            {
                return Unauthorized();
            }

            // Buscamos el registro y verificamos que pertenezca al usuario logueado
            var consumo = await _context.RegistrosConsumos
                .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId);

            if (consumo == null)
            {
                return NotFound("Consumo no encontrado o no autorizado para eliminar.");
            }

            _context.RegistrosConsumos.Remove(consumo);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Consumo eliminado correctamente." });
        }
    }
}
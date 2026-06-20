using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutricionMacros.API.Data;
using NutricionMacros.API.Models;

namespace NutricionMacros.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ⚠️ ESTA ETIQUETA OBLIGA A QUE CADA PETICIÓN LLEVE UN TOKEN JWT VÁLIDO
    public class AlimentosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AlimentosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. OBTENER TODOS LOS ALIMENTOS: GET /api/Alimentos
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var alimentos = await _context.Alimentos.ToListAsync();
            return Ok(alimentos);
        }

        // 2. OBTENER ALIMENTO POR ID: GET /api/Alimentos/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var alimento = await _context.Alimentos.FindAsync(id);
            if (alimento == null)
            {
                return NotFound("El alimento solicitado no existe.");
            }
            return Ok(alimento);
        }

        // 3. CREAR UN NUEVO ALIMENTO: POST /api/Alimentos
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] AlimentoCrearDto request)
        {
            var nuevoAlimento = new Alimento
            {
                Nombre = request.Nombre,
                Calorias = request.Calorias,
                Proteinas = request.Proteinas,
                Carbohidratos = request.Carbohidratos,
                Grasas = request.Grasas
            };

            _context.Alimentos.Add(nuevoAlimento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoAlimento.Id }, nuevoAlimento);
        }

        // DELETE: api/Alimentos/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var alimento = await _context.Alimentos.FindAsync(id);
            if (alimento == null) return NotFound();
            _context.Alimentos.Remove(alimento);
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Alimento eliminado correctamente." });
        }

        // PUT: api/Alimentos/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] AlimentoCrearDto request)
        {
            var alimento = await _context.Alimentos.FindAsync(id);
            if (alimento == null) return NotFound();

            alimento.Nombre = request.Nombre;
            alimento.Calorias = request.Calorias;
            alimento.Proteinas = request.Proteinas;
            alimento.Carbohidratos = request.Carbohidratos;
            alimento.Grasas = request.Grasas;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Alimento actualizado correctamente.", alimento });
        }
    }
}

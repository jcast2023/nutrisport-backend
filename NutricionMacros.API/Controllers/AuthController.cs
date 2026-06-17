using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NutricionMacros.API.Data;
using NutricionMacros.API.Models;

namespace NutricionMacros.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        // Inyectamos el contexto de la BD y las configuraciones del appsettings
        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistroDto request)
        {
            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Email == request.Email.ToLower());
            if (usuarioExiste)
            {
                return BadRequest("El correo electrónico ya está registrado.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var nuevoUsuario = new Usuario
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email.ToLower(),
                PasswordHash = passwordHash,
                FechaRegistro = DateTime.UtcNow,
                Rol = "Atleta"
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario registrado exitosamente con seguridad activa." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // 1. Buscar al usuario por correo
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

            // 2. Validar credenciales con BCrypt
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            {
                return Unauthorized("Correo electrónico o contraseña incorrectos.");
            }

            // 3. Si todo está bien, generar el Token JWT
            string tokenString = GenerarJwtToken(usuario);

            // 4. Devolver el token y los datos básicos a Angular
            return Ok(new
            {
                token = tokenString,
                usuario = new
                {
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Email
                }
            });
        }

        // Método privado encargado de la creación física y firma del JWT
        private string GenerarJwtToken(Usuario usuario)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Información interna que viajará dentro del token (Claims)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.GivenName, $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1), // El token expira en 1 día
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // GET: api/Auth/Pacientes — solo Nutricionista
        [HttpGet("Pacientes")]
        [Authorize(Roles = "Nutricionista")]
        public async Task<IActionResult> GetPacientes()
        {
            var pacientes = await _context.Usuarios
                .Where(u => u.Rol == "Atleta")
                .Select(u => new {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.Email,
                    u.FechaRegistro
                })
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return Ok(pacientes);
        }
    }
}
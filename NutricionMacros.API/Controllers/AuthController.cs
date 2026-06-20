using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NutricionMacros.API.Data;
using NutricionMacros.API.Models;
using NutricionMacros.API.Services;

namespace NutricionMacros.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        // Inyectamos el contexto de la BD y las configuraciones del appsettings
        public AuthController(ApplicationDbContext context, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
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
                    usuario.Email,
                    Rol = usuario.Rol
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

        // GET: api/Auth/Pacientes
        [HttpGet("Pacientes")]
        [Authorize(Roles = "Administrador")]
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
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim());

            if (usuario == null)
            {
                return Ok(new { mensaje = "Si el correo electrónico coincide con una cuenta, recibirás un enlace para restablecer tu contraseña." });
            }

            string tokenHex = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            usuario.ResetToken = tokenHex;
            usuario.ResetTokenExpiracion = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync();

            string frontendUrl = _config["AppSettings:FrontendUrl"] ?? "http://localhost:4200";
            string urlFrontend = $"{frontendUrl}/reset-password?token={tokenHex}";

            string cuerpoHtml = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
            <h2 style='color: #333; text-align: center;'>Recuperación de Contraseña</h2>
            <p>Hola, <strong>{usuario.Nombre}</strong>.</p>
            <p>Has solicitado restablecer la contraseña de tu cuenta en <strong>NutricionMacrosApp</strong>. Haz clic en el botón de abajo para configurar una nueva clave:</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{urlFrontend}' style='background-color: #007bff; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Restablecer mi contraseña</a>
            </div>
            <p style='color: #666; font-size: 12px;'>Este enlace expirará automáticamente en 15 minutos por motivos de seguridad.</p>
            <p style='color: #666; font-size: 12px;'>Si no solicitaste este cambio, puedes ignorar este correo de forma segura.</p>
        </div>";

            // 🔑 IMPORTANTE: No esperes, dispara y olvida (Fire-and-forget)
            // El usuario recibe respuesta inmediatamente
#pragma warning disable CS4014
            _emailService.EnviarCorreoAsync(usuario.Email, "Restablecer Contraseña", cuerpoHtml);
#pragma warning restore CS4014

            return Ok(new { mensaje = "Si el correo electrónico coincide con una cuenta, recibirás un enlace para restablecer tu contraseña." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            // Buscamos el registro que coincida con el token guardado
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.ResetToken == request.Token);

            // Validamos si el token existe o si ya expiró comparándolo con la hora actual UTC
            if (usuario == null || usuario.ResetTokenExpiracion < DateTime.UtcNow)
            {
                return BadRequest(new { mensaje = "El enlace de recuperación es inválido o ya ha expirado." });
            }

            // Hasheamos la nueva contraseña de forma segura utilizando BCrypt
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NuevaPassword.Trim());

            // Limpiamos los campos del token para invalidarlo inmediatamente
            usuario.ResetToken = null;
            usuario.ResetTokenExpiracion = null;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña restablecida con éxito. Ya puedes iniciar sesión con tus nuevas credenciales." });
        }
    }
}
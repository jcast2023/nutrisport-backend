using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NutricionMacros.API.Data;
using System.Security.Claims;
using Npgsql;

AppContext.SetSwitch("System.Net.Sockets.Socket.OSSupportsIPv6", false);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE CORS (CORREGIDO PARA EVITAR FAILED TO FETCH)
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular", policy =>
    {
        // SetIsOriginAllowed(origin => true) permite que tanto Angular (puerto 4200) 
        // como la propia interfaz local de Swagger puedan enviar las cabeceras de autorización sin bloqueos.
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Útil para persistencia de sesiones en el desarrollo local
    });
});

// ==========================================
// 2. CONFIGURACIÓN DE POSTGRESQL (SUPABASE)
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("SupabaseConnection");
var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dataSource));

// ==========================================
// 3. CONFIGURACIÓN DE AUTENTICACIÓN JWT
// ==========================================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ==========================================
// CONFIGURACIÓN MEJORADA DE SWAGGER
// ==========================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NutricionMacros.API", Version = "v1" });

    // Definición de esquema Bearer optimizado
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        // Actualizamos la descripción para evitar confusiones con la inyección automática
        Description = "COPIA Y PEGA ÚNICAMENTE EL TOKEN PURO (El string largo que empieza con eyJ...). NO escribas 'Bearer ' manualmente.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ==========================================
// 4. ACTIVAR POLÍTICAS (EL ORDEN ES CRUCIAL)
// ==========================================



app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            await context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    mensaje = error.Error.Message,
                    detalle = error.Error.StackTrace
                })
            );
        }
    });
});
app.UseCors("PermitirAngular"); // Abre las puertas de enlace antes de validar accesos

app.UseAuthentication(); // Primero: ¿Quién eres? (Verifica el Token JWT)
app.UseAuthorization();  // Segundo: ¿Qué tienes permitido hacer?

app.MapControllers();

app.Run();
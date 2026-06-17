# NutriSport API 🥗

Backend REST API para el sistema de nutrición deportiva **NutriSport**, desarrollado con ASP.NET Core 8 y PostgreSQL en Supabase.

## 🚀 Tecnologías

- **ASP.NET Core 8** — Framework principal
- **Entity Framework Core** — ORM para PostgreSQL
- **Npgsql** — Driver PostgreSQL para .NET
- **JWT Bearer** — Autenticación y autorización por roles
- **BCrypt.Net** — Hash de contraseñas
- **Swagger/OpenAPI** — Documentación de endpoints

## 📁 Estructura del proyecto

NutricionMacros.API/
├── Controllers/
│   ├── AuthController.cs          # Registro y login
│   ├── AlimentosController.cs     # CRUD de alimentos
│   ├── CitasController.cs         # Gestión de citas
│   ├── ConsumosController.cs      # Registro de consumos diarios
│   ├── ObjetivosController.cs     # Objetivos nutricionales
│   └── MedicionesController.cs    # Ficha clínica corporal
├── Models/
│   ├── Usuario.cs
│   ├── Alimento.cs
│   ├── Cita.cs
│   ├── RegistroConsumo.cs
│   ├── ObjetivoNutricional.cs
│   └── MedicionCorporal.cs
├── DTOs/
│   ├── LoginDto.cs
│   ├── RegistroDto.cs
│   ├── CitaResponseDto.cs
│   ├── CrearCitaDto.cs
│   ├── CambiarEstadoDto.cs
│   ├── MedicionCrearDto.cs
│   └── ResumenConsumoDto.cs
└── Data/
    └── ApplicationDbContext.cs

## 🔐 Autenticación

El sistema usa JWT con dos roles:

| Rol | Acceso |
|-----|--------|
| `Atleta` | Dashboard personal, consumos, citas, mediciones propias |
| `Nutricionista` | Panel admin, todos los pacientes, asignación de objetivos |

## 📡 Endpoints principales

### Auth
- `POST /api/Auth/register` — Registro de nuevo usuario
- `POST /api/Auth/login` — Login y obtención de JWT
- `GET /api/Auth/Pacientes` — Lista de pacientes [Nutricionista]

### Alimentos
- `GET /api/Alimentos` — Listar catálogo
- `POST /api/Alimentos` — Crear alimento [Nutricionista]
- `PUT /api/Alimentos/{id}` — Editar alimento [Nutricionista]
- `DELETE /api/Alimentos/{id}` — Eliminar alimento [Nutricionista]

### Citas
- `POST /api/Citas` — Agendar cita [Atleta]
- `GET /api/Citas/MisCitas` — Mis citas [Atleta]
- `GET /api/Citas/TodasLasCitas` — Todas las citas [Nutricionista]
- `PUT /api/Citas/{id}/estado` — Cambiar estado [Nutricionista]

### Consumos
- `POST /api/Consumos` — Registrar consumo
- `GET /api/Consumos/historial` — Historial del día
- `GET /api/Consumos/ResumenHoy` — Totales del día
- `DELETE /api/Consumos/{id}` — Eliminar consumo

### Objetivos
- `POST /api/Objetivos` — Asignar objetivo [Nutricionista]
- `GET /api/Objetivos/MiObjetivo` — Mi objetivo [Atleta]
- `GET /api/Objetivos/Paciente/{usuarioId}` — Objetivo de paciente

### Mediciones
- `POST /api/Mediciones` — Registrar medición [Atleta]
- `GET /api/Mediciones/MiHistorial` — Mi historial
- `GET /api/Mediciones/Paciente/{usuarioId}` — Historial paciente [Nutricionista]
- `POST /api/Mediciones/RegistrarParaPaciente/{id}` — Registrar para paciente [Nutricionista]
- `DELETE /api/Mediciones/{id}` — Eliminar medición

## ⚙️ Configuración local

1. Clona el repositorio:
```bash
git clone https://github.com/jcast2023/nutrisport-backend.git
```

2. Crea el archivo `appsettings.json` en la raíz del proyecto:
```json
{
  "ConnectionStrings": {
    "SupabaseConnection": "TU_CONNECTION_STRING_DE_SUPABASE"
  },
  "Jwt": {
    "Key": "TU_CLAVE_SECRETA_JWT",
    "Issuer": "NutricionMacros.API",
    "Audience": "NutricionMacros.Angular"
  }
}
```

3. Restaura dependencias y ejecuta:
```bash
dotnet restore
dotnet run
```

4. Accede a Swagger en: `https://localhost:7234/swagger`

## 🗄️ Base de datos

PostgreSQL hosteado en **Supabase**. Tablas principales:

- `Usuarios` — Pacientes y nutricionistas
- `Alimentos` — Catálogo de alimentos con macros
- `RegistrosConsumos` — Consumos diarios de cada paciente
- `Citas` — Agenda de consultas
- `ObjetivosNutricionales` — Metas asignadas por el nutricionista
- `MedicionesCorporales` — Ficha clínica con historial de evolución

## 👨‍💻 Desarrollador

Desarrollado por **Julio Castillo** como proyecto de portafolio.

- GitHub: [@jcast2023](https://github.com/jcast2023)

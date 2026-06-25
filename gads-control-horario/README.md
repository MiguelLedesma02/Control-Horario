# Sistema de Gestión de Novedades Laborales y Control Horario

**Trabajo Práctico — Gestión Aplicada al Desarrollo de Software**
Universidad Nacional de La Matanza · Ingeniería en Informática

SaaS para pymes argentinas que centraliza fichadas, interpreta novedades laborales (tardanzas, ausencias, horas extra, licencias, etc.) y exporta una preliquidación lista para el contador.

---

## Arquitectura

```
┌─────────────────┐     HTTPS/JWT     ┌──────────────────────┐     EF Core    ┌──────────────┐
│  Frontend SPA   │ ────────────────► │  ASP.NET Core API    │ ─────────────► │  SQL Server  │
│  React + TS     │                   │  (Clean Architecture)│                │              │
└─────────────────┘                   └──────────────────────┘                └──────────────┘
                                              │
                                              ▼
                                      ┌──────────────────┐
                                      │  Motor de Reglas │
                                      │  (Domain Service)│
                                      └──────────────────┘
```

### Stack

| Capa | Tecnología |
|------|------------|
| Frontend | React 18 + TypeScript + Vite + TailwindCSS |
| Backend | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 |
| DB | SQL Server (compatible con SQLite para dev) |
| Auth | JWT Bearer + roles |
| Exportación | EPPlus (Excel), CsvHelper (CSV) |

### Capas del backend (Clean Architecture)

```
ControlHorario.Domain        → Entidades, enums, reglas de negocio puras
ControlHorario.Application   → Casos de uso, DTOs, motor de reglas
ControlHorario.Infrastructure→ EF Core, repositorios, exportadores
ControlHorario.Api           → Controllers, JWT, Swagger
```

---

## Cómo correr el proyecto

### Backend

```bash
cd backend/ControlHorario.Api
dotnet restore
dotnet ef database update          # crea la DB
dotnet run                         # http://localhost:5080  + Swagger en /swagger
```

### Frontend

```bash
cd frontend
npm install
npm run dev                        # http://localhost:5173
```

### Usuarios de prueba (seed)

| Rol           | Usuario              | Password    |
|---------------|----------------------|-------------|
| Administrador | admin@pyme.com       | Admin123!   |
| Empleado      | jperez@pyme.com      | Empleado1!  |
| Contador      | contador@estudio.com | Contador1!  |

---

## Módulos implementados

- [x] **Gestión de empleados** — alta manual, importación Excel, API REST
- [x] **Gestión de horarios y turnos** — fijos, rotativos, parciales, parametrización
- [x] **Registro de fichadas** — biométrico (API), QR, PIN, manual, separación cruda/interpretada
- [x] **Motor de reglas** — tardanza, ausencia, doble fichada, horas extra 50/100, salida anticipada, descanso excedido
- [x] **Gestión de novedades** — automáticas + manuales, workflow pendiente/aprobada/rechazada
- [x] **Cierre mensual** — snapshot inmutable, exportación XLSX/CSV/PDF
- [x] **Multi-rol** — Admin, Empleado, Contador (vista solo lectura)

---

## Estructura del repositorio

```
.
├── backend/
│   ├── ControlHorario.Domain/
│   ├── ControlHorario.Application/
│   ├── ControlHorario.Infrastructure/
│   └── ControlHorario.Api/
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── services/
│   │   ├── context/
│   │   └── types/
│   └── package.json
├── sql/
│   └── 001_schema.sql
└── docs/
    ├── modelo-datos.md
    ├── casos-de-uso.md
    └── motor-reglas.md
```

Ver `docs/` para la documentación de diseño completa.

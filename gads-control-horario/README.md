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

### Requisitos previos

| Herramienta | Versión | Descarga |
|-------------|---------|----------|
| .NET SDK    | 8.0     | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Node.js     | 18 o superior (trae `npm`) | https://nodejs.org/ |

Verificá que estén instalados:

```bash
dotnet --version   # debe imprimir 8.x
node --version     # v18+
```

> **Importante (puerto del backend):** el frontend está configurado para hablar con el backend en
> `http://localhost:5080`. El proyecto **no trae** un `launchSettings.json`, así que **siempre** hay que
> levantar el backend indicando ese puerto con `--urls "http://localhost:5080"`. Si lo arrancás sin eso,
> .NET usa el puerto 5000 por defecto y el frontend no se puede conectar (errores de login/red).

> **Base de datos:** se usa **SQLite** y se crea sola al arrancar el backend (`EnsureCreated()` + seed).
> **No** hace falta correr `dotnet ef database update`.

---

### Windows (paso a paso)

Abrí **dos** terminales de PowerShell.

**Terminal 1 — Backend:**

```powershell
cd backend\ControlHorario.Api
dotnet restore
dotnet run --urls "http://localhost:5080"
```

**Terminal 2 — Frontend:**

```powershell
cd frontend
npm install
npm run dev
```

Cuando el backend diga `Now listening on: http://localhost:5080`, abrí el navegador en
`http://localhost:5173`.

> Alternativa con doble click: están los scripts `instalar.bat` (instala dependencias) e `iniciar.bat`
> (levanta todo). Si `iniciar.bat` no logra conectar con el backend, usá el paso a paso manual de arriba,
> que fija el puerto 5080 explícitamente.

---

### Linux / macOS (paso a paso)

Abrí **dos** terminales.

**Terminal 1 — Backend:**

```bash
cd backend/ControlHorario.Api
dotnet restore
dotnet run --urls "http://localhost:5080"
```

**Terminal 2 — Frontend:**

```bash
cd frontend
npm install
npm run dev
```

Cuando el backend diga `Now listening on: http://localhost:5080`, abrí el navegador en
`http://localhost:5173`.

---

### Usuarios de demo (seed)

| Rol           | Usuario              | Password    |
|---------------|----------------------|-------------|
| Administrador | admin@pyme.com       | Admin123!   |
| Empleado      | jperez@pyme.com      | Empleado1!  |
| Contador      | contador@estudio.com | Contador1!  |

### Datos de demo (seed)

Como una app de control horario **nunca tiene datos reales en una demo**, al arrancar el backend con la
base vacía se genera automáticamente (`DbSeeder`) un set de datos de ejemplo para poder mostrar todas las
funcionalidades, en especial **del lado del empleado** (`jperez@pyme.com`):

- **~3 semanas de fichadas** del mes en curso para Juan Pérez, con jornadas normales y casos especiales
  (tardanza, salida anticipada, horas extra, doble fichada, etc.) y una **jornada en curso de hoy**.
- **Novedades generadas por el motor de reglas real** (no inventadas), en distintos estados:
  Tardanza (aprobada), Salida anticipada (rechazada), Horas extra 50% y 100% (aprobadas),
  Descanso excedido / Ausencia / Doble fichada (pendientes).
- **Novedades manuales** para mostrar el workflow: Licencia por enfermedad (aprobada),
  Justificativo médico (pendiente) y Permiso especial (rechazado con motivo).

> Los datos se generan **solo cuando la base está vacía**. Para regenerarlos desde cero, borrá el archivo
> de base de datos y volvé a iniciar el backend (ver *Solución de problemas*).

---

## Solución de problemas

### El backend no arranca: "Address already in use" (puerto 5080 ocupado)

Significa que ya hay un backend corriendo en ese puerto. Hay que matar el proceso colgado:

**Linux / macOS:**

```bash
lsof -ti tcp:5080 | xargs kill -9
```

**Windows (PowerShell):**

```powershell
Get-NetTCPConnection -LocalPort 5080 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
```

**Windows (CMD):**

```bat
netstat -ano | findstr :5080
taskkill /PID <PID_que_aparezca> /F
```

### Reiniciar la base de datos desde cero (volver a los datos de demo)

Pará el backend, borrá la base y volvé a iniciarlo (se recrea y re-seedea limpia):

**Linux / macOS** (desde `backend/ControlHorario.Api`):

```bash
rm -f controlhorario.db controlhorario.db-shm controlhorario.db-wal
dotnet run --urls "http://localhost:5080"
```

**Windows (PowerShell)** (desde `backend\ControlHorario.Api`):

```powershell
Remove-Item controlhorario.db* -ErrorAction SilentlyContinue
dotnet run --urls "http://localhost:5080"
```

### `dotnet` o `node` "no se reconoce"

Cerrá **todas** las terminales y abrí una nueva (las variables de entorno solo se actualizan en ventanas
nuevas). Verificá con `dotnet --version` y `node --version`.

### Restore/instalación bloqueada por un firewall corporativo (Socket Firewall)

Si tu organización tiene **Socket Firewall** instalado y `dotnet restore` o `npm install` fallan con
errores de certificado SSL (`PartialChain`) o `Unable to reach Socket API`, antepené `SFW_BYPASS=1`:

```bash
SFW_BYPASS=1 dotnet restore
SFW_BYPASS=1 npm install
```

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

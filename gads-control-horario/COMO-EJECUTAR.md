# Cómo ejecutar el sistema (Windows)

Estos scripts hacen TODO el trabajo automáticamente.

---

## La primera vez: instalar todo

**Hacé doble click sobre `instalar.bat`**

Va a:
1. Verificar si tenés .NET 8 instalado, si no lo instala con winget
2. Verificar si tenés Node.js instalado, si no lo instala con winget
3. Descargar todas las dependencias del backend (.NET)
4. Descargar todas las dependencias del frontend (npm)

**Tardá entre 5 y 15 minutos** la primera vez (depende de tu conexión).

> Si Windows te muestra una advertencia tipo "este script no está firmado"
> o "SmartScreen", hacé click en **"Más información"** → **"Ejecutar de todos modos"**.
> Es normal — los scripts de PowerShell descargados de internet siempre piden confirmación.

> Si te dice que **winget no está disponible**, instalá "App Installer" desde
> la Microsoft Store, o instalá .NET y Node manualmente desde:
> - https://dotnet.microsoft.com/download/dotnet/8.0
> - https://nodejs.org/

---

## Cada vez que quieras usar el sistema

**Hacé doble click sobre `iniciar.bat`**

Va a:
1. Abrir una ventana de PowerShell con el backend
2. Esperar a que el backend esté listo
3. Abrir otra ventana de PowerShell con el frontend
4. Abrir tu navegador en http://localhost:5173 automáticamente

Ya está. Vas a ver la pantalla de login con los usuarios de demo a la izquierda.

---

## Para apagar todo

Cerrá las dos ventanas de PowerShell que se abrieron (las que dicen BACKEND y FRONTEND).

O en cada ventana apretá **Ctrl + C**.

---

## Usuarios de demo

| Rol           | Email                | Contraseña    |
|---------------|----------------------|---------------|
| Administrador | admin@pyme.com       | Admin123!     |
| Empleado      | jperez@pyme.com      | Empleado1!    |
| Contador      | contador@estudio.com | Contador1!    |

---

## Si algo sale mal

**El backend no arranca:** abrí una PowerShell, andá a la carpeta `backend\ControlHorario.Api` y corré `dotnet run`. Vas a ver el error real.

**El frontend no arranca:** abrí una PowerShell, andá a la carpeta `frontend` y corré `npm run dev`. Vas a ver el error real.

**Aparece "no se reconoce dotnet" o "no se reconoce node":** cerrá TODAS las ventanas de PowerShell y abrí una nueva. Las variables de entorno solo se actualizan en ventanas nuevas.

**El navegador no abre solo:** entrá manualmente a http://localhost:5173

**Quiero borrar la base de datos para empezar de cero:** parar el backend, borrar el archivo `backend\ControlHorario.Api\controlhorario.db`, y volver a iniciar. Se vuelve a crear con los datos de demo.

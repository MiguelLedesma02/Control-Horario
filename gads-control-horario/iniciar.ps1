# =============================================================
# iniciar.ps1
# Levanta el backend (.NET) y el frontend (React) en dos
# ventanas de PowerShell separadas, despues abre el navegador.
# =============================================================
# Uso: click derecho sobre el archivo → "Ejecutar con PowerShell"
#      o en una terminal:  .\iniciar.ps1
# =============================================================

$ErrorActionPreference = "Stop"

$raiz = Split-Path -Parent $MyInvocation.MyCommand.Path
$backendDir = Join-Path $raiz "backend\ControlHorario.Api"
$frontendDir = Join-Path $raiz "frontend"

# Verificaciones rapidas
if (-not (Test-Path $backendDir)) {
    Write-Host "ERROR: no encuentro $backendDir" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"; exit 1
}
if (-not (Test-Path $frontendDir)) {
    Write-Host "ERROR: no encuentro $frontendDir" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"; exit 1
}
if (-not (Test-Path (Join-Path $frontendDir "node_modules"))) {
    Write-Host "ATENCION: no se encontro node_modules en frontend." -ForegroundColor Yellow
    Write-Host "Corre primero: .\instalar.ps1" -ForegroundColor Yellow
    Read-Host "Presiona Enter para salir"; exit 1
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " Iniciando Control Horario SaaS" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# ─── Lanzar backend en ventana propia ───
Write-Host "Lanzando BACKEND en nueva ventana..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "Set-Location '$backendDir'; Write-Host 'BACKEND - Control Horario API' -ForegroundColor Cyan; Write-Host 'http://localhost:5080/swagger' -ForegroundColor Yellow; Write-Host ''; dotnet run"
)

# ─── Esperar que el backend este listo ───
Write-Host "Esperando que el backend este disponible..." -ForegroundColor Yellow
$intentos = 0
$backendListo = $false
while ($intentos -lt 60 -and -not $backendListo) {
    Start-Sleep -Seconds 1
    try {
        $resp = Invoke-WebRequest -Uri "http://localhost:5080/swagger" -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
        if ($resp.StatusCode -eq 200) { $backendListo = $true }
    } catch {
        # sigue esperando
    }
    $intentos++
    if ($intentos % 5 -eq 0) { Write-Host "  ... esperando ($intentos s)" -ForegroundColor Gray }
}

if ($backendListo) {
    Write-Host "OK: Backend disponible en http://localhost:5080" -ForegroundColor Green
} else {
    Write-Host "ATENCION: el backend tarda mas de lo normal. Continuamos igual." -ForegroundColor Yellow
}

# ─── Lanzar frontend en ventana propia ───
Write-Host ""
Write-Host "Lanzando FRONTEND en nueva ventana..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "Set-Location '$frontendDir'; Write-Host 'FRONTEND - Control Horario Web' -ForegroundColor Cyan; Write-Host 'http://localhost:5173' -ForegroundColor Yellow; Write-Host ''; npm run dev"
)

# ─── Esperar y abrir navegador ───
Write-Host "Esperando que el frontend este disponible..." -ForegroundColor Yellow
$intentos = 0
$frontListo = $false
while ($intentos -lt 30 -and -not $frontListo) {
    Start-Sleep -Seconds 1
    try {
        $resp = Invoke-WebRequest -Uri "http://localhost:5173" -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
        if ($resp.StatusCode -eq 200) { $frontListo = $true }
    } catch {}
    $intentos++
}

if ($frontListo) {
    Write-Host "OK: Frontend disponible. Abriendo navegador..." -ForegroundColor Green
    Start-Process "http://localhost:5173"
} else {
    Write-Host "Abri manualmente http://localhost:5173 en tu navegador." -ForegroundColor Yellow
    Start-Process "http://localhost:5173"
}

# ─── Resumen ───
Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " SISTEMA EN EJECUCION" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Frontend:  http://localhost:5173" -ForegroundColor White
Write-Host "  Backend:   http://localhost:5080" -ForegroundColor White
Write-Host "  Swagger:   http://localhost:5080/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Usuarios de demo:" -ForegroundColor White
Write-Host "  Admin:     admin@pyme.com         / Admin123!" -ForegroundColor Gray
Write-Host "  Empleado:  jperez@pyme.com        / Empleado1!" -ForegroundColor Gray
Write-Host "  Contador:  contador@estudio.com   / Contador1!" -ForegroundColor Gray
Write-Host ""
Write-Host "Para apagar todo: cerra las dos ventanas de PowerShell que se abrieron." -ForegroundColor Yellow
Write-Host ""
Read-Host "Presiona Enter para cerrar esta ventana (las otras dos siguen corriendo)"

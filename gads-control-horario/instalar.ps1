# =============================================================
# instalar.ps1
# Instala .NET 8 SDK y Node.js LTS si no están presentes,
# después instala las dependencias del backend y del frontend.
# =============================================================
# Uso: click derecho sobre el archivo → "Ejecutar con PowerShell"
#      o en una terminal:  .\instalar.ps1
# =============================================================

$ErrorActionPreference = "Stop"

function Write-Paso($texto) {
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host " $texto" -ForegroundColor Cyan
    Write-Host "=========================================" -ForegroundColor Cyan
}

function Test-Comando($comando) {
    try { Get-Command $comando -ErrorAction Stop | Out-Null; return $true }
    catch { return $false }
}

function Refrescar-Path {
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" +
                [System.Environment]::GetEnvironmentVariable("Path", "User")
}

# ─── Verificar permisos para usar winget ───
Write-Paso "Verificando entorno"

if (-not (Test-Comando "winget")) {
    Write-Host "ERROR: winget no esta disponible." -ForegroundColor Red
    Write-Host "Necesitas Windows 10 1809+ o Windows 11 con App Installer."
    Write-Host "Descargalo desde la Microsoft Store: 'App Installer'"
    Write-Host ""
    Write-Host "Alternativa manual:"
    Write-Host "  - .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0"
    Write-Host "  - Node.js LTS: https://nodejs.org/"
    Read-Host "Presiona Enter para salir"
    exit 1
}
Write-Host "OK: winget disponible" -ForegroundColor Green

# ─── .NET 8 SDK ───
Write-Paso "Verificando .NET 8 SDK"

$dotnetOk = $false
if (Test-Comando "dotnet") {
    $sdks = & dotnet --list-sdks 2>$null
    if ($sdks -match "^8\.") {
        Write-Host "OK: .NET 8 SDK ya instalado" -ForegroundColor Green
        $dotnetOk = $true
    } else {
        Write-Host "Tenes dotnet pero no la version 8."
    }
}

if (-not $dotnetOk) {
    Write-Host "Instalando .NET 8 SDK con winget..." -ForegroundColor Yellow
    winget install Microsoft.DotNet.SDK.8 --accept-source-agreements --accept-package-agreements -h
    Refrescar-Path
    if (-not (Test-Comando "dotnet")) {
        Write-Host "ATENCION: dotnet no se detecta aun. Cerra y reabri PowerShell, despues volve a correr este script." -ForegroundColor Yellow
        Read-Host "Presiona Enter para salir"
        exit 1
    }
    Write-Host "OK: .NET 8 SDK instalado" -ForegroundColor Green
}

# ─── Node.js ───
Write-Paso "Verificando Node.js"

$nodeOk = $false
if (Test-Comando "node") {
    $version = (& node --version) -replace 'v',''
    $major = [int]($version.Split('.')[0])
    if ($major -ge 18) {
        Write-Host "OK: Node.js $version ya instalado" -ForegroundColor Green
        $nodeOk = $true
    } else {
        Write-Host "Tenes Node $version pero necesitamos 18 o superior."
    }
}

if (-not $nodeOk) {
    Write-Host "Instalando Node.js LTS con winget..." -ForegroundColor Yellow
    winget install OpenJS.NodeJS.LTS --accept-source-agreements --accept-package-agreements -h
    Refrescar-Path
    if (-not (Test-Comando "node")) {
        Write-Host "ATENCION: node no se detecta aun. Cerra y reabri PowerShell, despues volve a correr este script." -ForegroundColor Yellow
        Read-Host "Presiona Enter para salir"
        exit 1
    }
    Write-Host "OK: Node.js instalado" -ForegroundColor Green
}

# ─── Restaurar dependencias del backend ───
Write-Paso "Restaurando paquetes del backend (.NET)"

$raiz = Split-Path -Parent $MyInvocation.MyCommand.Path
$backendDir = Join-Path $raiz "backend\ControlHorario.Api"

if (-not (Test-Path $backendDir)) {
    Write-Host "ERROR: no se encuentra $backendDir" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"
    exit 1
}

Push-Location $backendDir
try {
    Write-Host "Esto puede tardar unos minutos la primera vez..." -ForegroundColor Yellow
    dotnet restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore fallo" }
    Write-Host "OK: backend listo" -ForegroundColor Green
} finally {
    Pop-Location
}

# ─── Instalar dependencias del frontend ───
Write-Paso "Instalando paquetes del frontend (npm)"

$frontendDir = Join-Path $raiz "frontend"
if (-not (Test-Path $frontendDir)) {
    Write-Host "ERROR: no se encuentra $frontendDir" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"
    exit 1
}

Push-Location $frontendDir
try {
    Write-Host "Esto puede tardar unos minutos la primera vez..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) { throw "npm install fallo" }
    Write-Host "OK: frontend listo" -ForegroundColor Green
} finally {
    Pop-Location
}

# ─── Listo ───
Write-Paso "TODO INSTALADO CORRECTAMENTE"
Write-Host ""
Write-Host "Para iniciar el sistema, ejecuta:" -ForegroundColor White
Write-Host "  .\iniciar.ps1" -ForegroundColor Yellow
Write-Host ""
Write-Host "(O hace doble click en iniciar.ps1)" -ForegroundColor Gray
Write-Host ""
Read-Host "Presiona Enter para cerrar"

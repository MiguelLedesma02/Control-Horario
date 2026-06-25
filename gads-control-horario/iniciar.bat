@echo off
REM Lanzador de doble-click para iniciar.ps1
REM Sortea la politica de ejecucion de PowerShell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0iniciar.ps1"
pause

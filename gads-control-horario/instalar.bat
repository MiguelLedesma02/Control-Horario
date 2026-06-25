@echo off
REM Lanzador de doble-click para instalar.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0instalar.ps1"
pause

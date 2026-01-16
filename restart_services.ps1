# restart_services.ps1

Write-Host "==================================================="
Write-Host "   ATLANTIC CITY - REINICIANDO SOLO SERVICIOS .NET"
Write-Host "==================================================="

# 1. Matar procesos locales (Dotnet y Node)
Write-Host "[1/2] Deteniendo procesos dotnet/node..."
Stop-Process -Name "dotnet" -ErrorAction SilentlyContinue -Force
Stop-Process -Name "node" -ErrorAction SilentlyContinue -Force

# 2. Ejecutar el script de inicio
# Esto asume que Docker (SQL, Rabbit, Seaweed) YA ESTÁ CORRIENDO
Write-Host "[2/2] Ejecutando start_env.bat..."
cmd /c start_env.bat

Write-Host "¡Servicios reiniciados!"

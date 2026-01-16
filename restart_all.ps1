# restart_all.ps1

Write-Host "==================================================="
Write-Host "   ATLANTIC CITY - REINICIANDO TODO EL SISTEMA"
Write-Host "==================================================="

# 1. Detener Contenedores (Docker)
Write-Host "[1/5] Deteniendo contenedores Docker..."
docker-compose down

# 2. Matar procesos locales (Dotnet y Node)
Write-Host "[2/5] Deteniendo procesos locales (dotnet, node)..."
Stop-Process -Name "dotnet" -ErrorAction SilentlyContinue -Force
Stop-Process -Name "node" -ErrorAction SilentlyContinue -Force
# También intentar cerrar las ventanas de consola si tienen títulos específicos (opcional, pero Stop-Process suele bastar para el backend)
# Nota: Esto cierra todos los dotnet/node del usuario.

# 3. Levantar Contenedores
Write-Host "[3/5] Levantando infraestructura (Docker)..."
docker-compose up -d
# Esperar unos segundos para que SQL Server y RabbitMQ esten listos
Write-Host "      Esperando 10 segundos para inicialización de infraestructura..."
Start-Sleep -Seconds 10

# 4. Ejecutar el script de inicio (start_env.bat)
Write-Host "[4/5] Ejecutando start_env.bat..."
# Usamos cmd /c para correr el bat desde powershell
cmd /c start_env.bat

Write-Host "[5/5] ¡Reinicio completado!"

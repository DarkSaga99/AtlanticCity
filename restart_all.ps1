# restart_all.ps1

Write-Host "==================================================="
Write-Host "   Atlantic City - System Restart"
Write-Host "==================================================="

Write-Host "Stopping Docker containers..."
docker-compose down

Write-Host "Stopping local processes (dotnet, node)..."
Stop-Process -Name "dotnet" -ErrorAction SilentlyContinue -Force
Stop-Process -Name "node" -ErrorAction SilentlyContinue -Force

Write-Host "Starting infrastructure..."
docker-compose up -d

Write-Host "Waiting for initialization..."
Start-Sleep -Seconds 10

Write-Host "Executing start_env.bat..."
cmd /c start_env.bat

Write-Host "Restart completed."

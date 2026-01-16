@echo off

echo Starting Gateway (Port 5000)...
start /B dotnet run --project src\Backend\AtlanticCity.Gateway --urls "http://localhost:5000"

echo Starting Identity Service (Port 5001)...
start /B dotnet run --project src\Backend\AtlanticCity.Services.Identity --urls "http://localhost:5001"

echo Starting Processing Service (Port 5002)...
start /B dotnet run --project src\Backend\AtlanticCity.Services.Processing --urls "http://localhost:5002"

echo Starting BulkLoad Worker...
start /B dotnet run --project src\Backend\AtlanticCity.Workers.BulkLoad

echo Starting Notifications Worker...
start /B dotnet run --project src\Backend\AtlanticCity.Workers.Notifications

echo Starting Frontend (React/Vite)...
start /B npm run dev --prefix src\Frontend\atlantic-city-client

echo ===================================================
echo   Environment Ready - Press any key to close
echo ===================================================
pause

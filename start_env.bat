@echo off


echo [1/6] Lanzando Gateway (Puerto 5000)...
start /B dotnet run --project src\Backend\AtlanticCity.Gateway --urls "http://localhost:5000"

echo [2/6] Lanzando Servicio Identity (Puerto 5001)...
start /B dotnet run --project src\Backend\AtlanticCity.Services.Identity --urls "http://localhost:5001"

echo [3/6] Lanzando Servicio Procesamiento (Puerto 5002)...
start /B dotnet run --project src\Backend\AtlanticCity.Services.Processing --urls "http://localhost:5002"

echo [4/6] Lanzando Worker de Carga Masiva...
start /B dotnet run --project src\Backend\AtlanticCity.Workers.BulkLoad

echo [5/6] Lanzando Worker de Notificaciones...
start /B dotnet run --project src\Backend\AtlanticCity.Workers.Notifications

echo [6/6] Lanzando Frontend (React/Vite)...
start /B npm run dev --prefix src\Frontend\atlantic-city-client

echo ===================================================
echo   AMBIENTE LISTO - PULSA UNA TECLA PARA CERRAR
echo ===================================================
pause

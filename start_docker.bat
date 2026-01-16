@echo off
echo =========================================
echo   Atlantic City - Docker Environment
echo =========================================

echo Stopping existing containers...
docker-compose down

echo Building and starting services...
docker-compose up --build -d

echo Waiting for system to be ready...
timeout /t 10

echo.
echo =========================================
echo   Deployment Completed
echo =========================================
echo   Frontend:    http://localhost:5174
echo   RabbitMQ:    http://localhost:15672
echo   SeaweedFS:   http://localhost:8888
echo =========================================
echo.
pause

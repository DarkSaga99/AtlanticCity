@echo off
echo =========================================
echo   ATLANTIC CITY - INICIANDO DOCKER
echo =========================================

echo [1/3] Deteniendo contenedores anteriores...
docker-compose down

echo [2/3] Construyendo e iniciando servicios...
echo       (Esto puede tardar unos minutos la primera vez)
docker-compose up --build -d

echo [3/3] Esperando a que el sistema este listo...
timeout /t 10

echo.
echo =========================================
echo   DESPLIEGUE COMPLETADO
echo =========================================
echo   Frontend:    http://localhost:5174
echo   RabbitMQ:    http://localhost:15672
echo   SeaweedFS:   http://localhost:8888
echo =========================================
echo.
pause

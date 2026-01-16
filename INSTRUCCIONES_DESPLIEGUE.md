# Guía de Despliegue con Docker - Atlantic City

Este proyecto utiliza una arquitectura de microservicios moderna y está completamente contenedorizado. Las dependencias y la base de datos se inicializan automáticamente al arrancar.

## 1. Requisitos Previos

- **Docker Desktop** (Asegúrate de que esté **abierto y corriendo** antes de empezar).
- **Recursos**: Se recomienda asignar al menos 4GB de RAM a Docker para un rendimiento óptimo de SQL Server y RabbitMQ.

## 2. Despliegue con un solo comando

Para levantar el ecosistema completo (Infraestructura + Microservicios + Frontend):

1. Abre una terminal en la raíz del proyecto.
2. Ejecuta:
   ```bash
   docker-compose up --build -d
   ```

### ¿Qué sucede durante el despliegue?
- Se levantan los contenedores de infraestructura: **SQL Server**, **RabbitMQ** y **SeaweedFS**.
- Se compilan y empaquetan los microservicios de .NET 8.
- Se construye la imagen de producción del Frontend (Nginx + React).
- **Auto-Sanación de la DB**: Los servicios de Identity y Processing aplican automáticamente las **EF Core Migrations** y los **SPs de carga masiva** apenas SQL Server está listo.

## 3. Acceso a los Servicios

| Servicio | URL | Notas |
| :--- | :--- | :--- |
| **Portal Web** | [http://localhost:5174](http://localhost:5174) | Interfaz principal. |
| **RabbitMQ** | [http://localhost:15672](http://localhost:15672) | Panel Administrativo (`guest/guest`). |
| **SeaweedFS** | [http://localhost:8888](http://localhost:8888) | Consola de Almacenamiento (S3/Filer). |
| **API Gateway** | [http://localhost:5000](http://localhost:5000) | Puerta de entrada para los servicios. |

## 4. Credenciales de Prueba

El sistema pre-configura un usuario administrador:
- **Usuario:** `admin`
- **Contraseña:** `password123`

## 5. Mantenimiento y Troubleshooting

### Reiniciar desde cero (Limpieza Total)
Si deseas borrar todos los datos y empezar con una base de datos limpia:
```bash
docker-compose down -v
docker-compose up --build -d
```

### Ver Logs de Procesamiento
Para monitorear la carga de archivos en tiempo real:
```bash
docker logs -f ac-worker-bulkload
```

### Problemas comunes
- **Puerto 1433 ocupado**: Asegúrate de no tener un SQL Server local corriendo.
- **SQL Server Connection Timeout**: La primera vez puede tardar unos segundos extra. Los microservicios tienen políticas de reintento para esperar a que la DB esté lista.

---
**Nota**: El sistema utiliza **Entity Framework Core Migrations** para el esquema de tablas y un **Inicializador de C#** para los Procedimientos Almacenados internos. Ya no se requieren archivos SQL manuales para el despliegue.

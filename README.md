# Atlantic City - Sistema de Procesamiento Masivo de Datos


## 1. Características Principales

*   **Arquitectura Híbrida de Persistencia**: Uso de **Entity Framework Core** para una lógica de negocio limpia en microservicios y **Dapper** para máxima eficiencia en procesos batch de alto volumen.
*   **Gestión Automática de Esquemas**: Implementación de **EF Core Migrations** con aislamiento de historial por servicio, garantizando despliegues libres de colisiones incluso en arranques simultáneos.
*   **Resiliencia Nativa**: Políticas de **Retry** y **Circuit Breaker** (Polly) integradas para manejar fallas temporales de red o base de datos de forma transparente, protegiendo la integridad del sistema ante sobrecargas.
*   **Procesamiento Asíncrono (Event-Driven)**: Desacoplamiento mediante **RabbitMQ**, permitiendo escalar los Workers de carga de forma independiente.
*   **Almacenamiento Distribuido**: Integración con **SeaweedFS (S3)** para el manejo de archivos adjuntos de gran tamaño.
*   **Validación de Unicidad**: Lógica optimizada mediante **Stored Procedures** para el control de integridad de productos por período.

## 2. Componentes del Sistema

*   **API Gateway (YARP)**: Orquestador de rutas, seguridad JWT y túnel para **SignalR** (Notificaciones en tiempo real).
*   **Identity Service**: Microservicio encargado de la seguridad (Login/Registro) con soporte para **Refresh Tokens**.
*   **Processing Service**: Gestiona el flujo de vida de los archivos y la trazabilidad de los lotes.
*   **BulkLoad Worker**: El "motor" de procesamiento masivo. Lee Excel/CSV, valida reglas de negocio e inserta masivamente en SQL Server.
*   **Notifications Worker**: Se activa al finalizar la carga para emitir correos (MailKit) y actualizar alertas en pantalla.

## 3. Stack Tecnológico

| Capa | Tecnología |
| :--- | :--- |
| **Backend** | .NET 8 (C#) |
| **Frontend** | React (Vite) / CSS Moderno |
| **Persistencia** | SQL Server 2022 (EF Core + Dapper) |
| **Mensajería** | RabbitMQ |
| **Storage** | SeaweedFS (S3 Compatible) |
| **Resiliencia** | Polly |
| **Tiempo Real** | SignalR |

## 4. Estructura de Datos (Tablas Principales)

*   **Users**: Seguridad e identificación.
*   **ProcessBatches**: Control maestro de lotes (Nombre, Usuario, Estadísticas, Estado).
*   **Products**: Registros de negocio procesados y validados.
*   **AuditLogs**: Auditoría completa de acciones críticas.

## 5. Instrucciones de Uso

Para desplegar el proyecto localmente, consulta la [Guía de Despliegue con Docker](./INSTRUCCIONES_DESPLIEGUE.md).

---


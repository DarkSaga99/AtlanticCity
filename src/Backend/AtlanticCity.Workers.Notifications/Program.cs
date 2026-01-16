using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AtlanticCity.Compartido.Mensajeria;
using AtlanticCity.Compartido.Mensajeria.Dtos;
using AtlanticCity.Workers.Notifications.Application.Interfaces;
using AtlanticCity.Workers.Notifications.Infrastructure.Correo;
using AtlanticCity.Workers.Notifications.Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using AtlanticCity.Workers.Notifications.Core.Interfaces;
using AtlanticCity.Workers.Notifications.Infrastructure.Persistence;

namespace AtlanticCity.Workers.Notifications
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 0. Cargar Configuración
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables() // Permite que Docker sobreescriba la configuración
                .Build();

            // 1. Configuración del contenedor de Inyección de Dependencias
            var connectionString = configuration.GetConnectionString("DefaultConnection")!;
            var proveedorServicios = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddLogging(constructor => constructor.AddConsole())
                .AddDbContext<NotificacionesDbContext>(opciones => 
                    opciones.UseSqlServer(connectionString))
                .AddSingleton<IServicioMensajeria, ServicioRabbitMQ>() 
                .AddSingleton<IServicioCorreo, ServicioCorreoMailKit>() 
                .AddScoped<IRepositorioNotificacion, RepositorioNotificacion>() // Nuevo Repositorio
                .BuildServiceProvider();

            var log = proveedorServicios.GetRequiredService<ILogger<Program>>();
            var servicioMensajeria = proveedorServicios.GetRequiredService<IServicioMensajeria>();
            var servicioCorreo = proveedorServicios.GetRequiredService<IServicioCorreo>();

            log.LogInformation("[NOTIFICADOR] Iniciado y esperando confirmaciones...");

            // Se suscribe a la cola de "notificaciones" enviada por el BulkLoad Worker
            await servicioMensajeria.SuscribirseAsync<MensajeProcesamientoArchivo>("cola_notificaciones", async (mensaje) =>
            {
                // Crear un scope para los servicios Scoped (Repositorio)
                using var scope = proveedorServicios.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IRepositorioNotificacion>();

                try
                {
                    log.LogInformation($"[CIERRE] Procesando lote: {mensaje.IdMensaje}");

                    // 1. Persistencia y Auditoría (Todo encapsulado en Infraestructura)
                    await repo.FinalizarNotificacionAsync(mensaje.IdMensaje, mensaje.CorreoUsuario, mensaje.NombreArchivo);

                    // 2. Notificación (Desacoplada)
                    string cuerpo = $"El archivo '{mensaje.NombreArchivo}' se procesó exitosamente.";
                    await servicioCorreo.EnviarCorreoAsync(mensaje.CorreoUsuario, "Carga masiva completada", cuerpo);

                    log.LogInformation($"[EXITO] Lote {mensaje.IdMensaje} finalizado y correo enviado.");
                    
                    // 3. Notificar al Gateway/SignalR
                    await servicioMensajeria.PublicarMensajeAsync("cola_signalr", mensaje);
                }
                catch (Exception ex)
                {
                    log.LogError($"[ERROR] Fallo en la fase de notificación: {ex.Message}");
                }
            });

            // Mantiene el proceso vivo escuchandoRabbitMQ
            while (true) { await Task.Delay(1000); }
        }
    }
}

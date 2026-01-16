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
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")!;
            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddLogging(constructor => constructor.AddConsole())
                .AddDbContext<NotificacionesDbContext>(opciones => 
                    opciones.UseSqlServer(connectionString))
                .AddSingleton<IServicioMensajeria, ServicioRabbitMQ>() 
                .AddSingleton<IServicioCorreo, ServicioCorreoMailKit>() 
                .AddScoped<IRepositorioNotificacion, RepositorioNotificacion>()
                .BuildServiceProvider();

            var log = services.GetRequiredService<ILogger<Program>>();
            var messaging = services.GetRequiredService<IServicioMensajeria>();
            var emailService = services.GetRequiredService<IServicioCorreo>();

            log.LogInformation("Notifications Service started.");

            await messaging.SuscribirseAsync<MensajeProcesamientoArchivo>("cola_notificaciones", async (mensaje) =>
            {
                using var scope = services.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IRepositorioNotificacion>();

                try
                {
                    log.LogInformation($"Processing completion for: {mensaje.IdMensaje}");

                    await repo.FinalizarNotificacionAsync(mensaje.IdMensaje, mensaje.CorreoUsuario, mensaje.NombreArchivo);

                    string body = $"The file '{mensaje.NombreArchivo}' has been processed successfully.";
                    await emailService.EnviarCorreoAsync(mensaje.CorreoUsuario, "Bulk Load Completed", body);

                    log.LogInformation($"Batch {mensaje.IdMensaje} finalized and email sent.");
                    
                    await messaging.PublicarMensajeAsync("cola_signalr", mensaje);
                }
                catch (Exception ex)
                {
                    log.LogError($"Notification phase failed: {ex.Message}");
                }
            });

            while (true) { await Task.Delay(1000); }
        }
    }
}

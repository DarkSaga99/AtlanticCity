using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using AtlanticCity.Compartido.Mensajeria;
using AtlanticCity.Compartido.Mensajeria.Dtos;
using AtlanticCity.Workers.BulkLoad.Core.Interfaces;
using AtlanticCity.Workers.BulkLoad.Application.Services;
using AtlanticCity.Workers.BulkLoad.Infraestructura.Persistencia;
using AtlanticCity.Workers.BulkLoad.Infrastructure.Services;

namespace AtlanticCity.Workers.BulkLoad
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

            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddLogging(configure => configure.AddConsole())
                .AddHttpClient() // Requerido para IHttpClientFactory
                .AddDbContext<BulkLoadDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")))
                .AddSingleton<IServicioMensajeria, ServicioRabbitMQ>()
                .AddScoped<IProductoRepositorio, RepositorioProductoEF>()
                .AddScoped<IRepositorioLote, RepositorioLote>()
                .AddScoped<IDescargadorArchivos, DescargadorArchivosHttp>()
                .AddScoped<IParseadorArchivo, ParseadorArchivoExcel>()
                .AddScoped<IProcesadorLote, ProcesadorLote>()
                .BuildServiceProvider();

            var log = services.GetRequiredService<ILogger<Program>>();
            var rabbit = services.GetRequiredService<IServicioMensajeria>();

            log.LogInformation("BulkLoad Worker Started");

            await rabbit.SuscribirseAsync<MensajeProcesamientoArchivo>("cola_procesamiento_archivos", async (mensaje) =>
            {
                using var scope = services.CreateScope();
                var procesador = scope.ServiceProvider.GetRequiredService<IProcesadorLote>();
                await procesador.ProcesarAsync(mensaje);
            });

            await Task.Delay(-1);
        }
    }
}

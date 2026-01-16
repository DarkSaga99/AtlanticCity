using AtlanticCity.Servicios.Procesamiento.Core.Interfaces;
using AtlanticCity.Servicios.Procesamiento.Infraestructura.Storage;
using AtlanticCity.Servicios.Procesamiento.Infraestructura.Persistencia;
using AtlanticCity.Compartido.Mensajeria;
using Microsoft.AspNetCore.RateLimiting;
using Polly;
using Polly.Extensions.Http;
using AtlanticCity.Shared.Middleware;
using Microsoft.EntityFrameworkCore;
using AtlanticCity.Services.Processing.Infrastructure.Persistence;
using AtlanticCity.Compartido.Persistencia;

var builder = WebApplication.CreateBuilder(args);

// 1. INYECCIÓN DE DEPENDENCIAS (CLEAN ARCHITECTURE)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<ProcesamientoDbContext>(options =>
    options.UseSqlServer(connectionString, x => x.MigrationsHistoryTable("__ProcessingMigrationsHistory")));

builder.Services.AddHealthChecks(); // Para monitoreo de disponibilidad

// Definimos políticas de Resiliencia (Circuit Breaker y Retry)
var politicaReintento = HttpPolicyExtensions
    .HandleTransientHttpError() 
    .WaitAndRetryAsync(3, reintento => TimeSpan.FromSeconds(Math.Pow(2, reintento)));

var politicaCircuitBreaker = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

builder.Services.AddHttpClient<IServicioAlmacenamiento, ServicioAlmacenamientoSeaweed>()
    .AddPolicyHandler(politicaReintento)
    .AddPolicyHandler(politicaCircuitBreaker);

builder.Services.AddSingleton<IServicioMensajeria, ServicioRabbitMQ>();
builder.Services.AddScoped<RepositorioProcesamiento>(); // Implementación concreta compartida
builder.Services.AddScoped<IRepositorioLote>(sp => sp.GetRequiredService<RepositorioProcesamiento>());
builder.Services.AddScoped<IAuditoriaRepositorio>(sp => sp.GetRequiredService<RepositorioProcesamiento>());
builder.Services.AddScoped<ILoteConsultas>(sp => sp.GetRequiredService<RepositorioProcesamiento>());

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// 2. Rate Limiting
builder.Services.AddRateLimiter(opciones =>
{
    opciones.AddFixedWindowLimiter(policyName: "limite_subida", opcionesVentaja =>
    {
        opcionesVentaja.PermitLimit = 100;
        opcionesVentaja.Window = TimeSpan.FromMinutes(1);
    });
});

// 3. CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5174" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. MIGRACIÓN AUTOMÁTICA
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var log = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ProcesamientoDbContext>();
        await context.Database.MigrateAsync();
        
        // Cargar SPs (Opcional, si este servicio los necesita instalar)
        await InicializadorBaseDatos.InicializarAsync(connectionString, log);
    }
    catch (Exception ex)
    {
        log.LogError($"Error inicializando DB en Processing: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();
app.UseCors("AllowReact");
app.UseMiddleware<ExceptionMiddleware>(); // Middleware Global de Errores
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

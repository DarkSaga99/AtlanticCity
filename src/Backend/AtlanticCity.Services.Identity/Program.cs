using AtlanticCity.Servicios.Identidad.Aplicacion.Interfaces;
using AtlanticCity.Servicios.Identidad.Infraestructura.Seguridad;
using AtlanticCity.Compartido.Persistencia;
using AtlanticCity.Servicios.Identidad.Core.Interfaces;
using AtlanticCity.Servicios.Identidad.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURACIÓN DE SERVICIOS (CLEAN ARCHITECTURE)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// Inyección de Dependencias
builder.Services.AddDbContext<ContextoBaseDatos>(options =>
    options.UseSqlServer(connectionString, x => x.MigrationsHistoryTable("__IdentityMigrationsHistory")));

builder.Services.AddSingleton<IServicioToken, ServicioToken>(); 
builder.Services.AddScoped<IUsuarioRepositorio, RepositorioUsuario>();

// Registro de MediatR para CQRS (Escanea el ensamblado actual para encontrar Handlers)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. MIGRACIÓN AUTOMÁTICA Y CARGA DE SPS
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var log = services.GetRequiredService<ILogger<Program>>();
    try
    {
        // Aplicar Migraciones de EF Core (Tablas)
        var context = services.GetRequiredService<ContextoBaseDatos>();
        await context.Database.MigrateAsync();
        
        // Cargar SPs y Auditoría (Lógica que EF Core no maneja nativamente)
        await InicializadorBaseDatos.InicializarAsync(connectionString, log);
    }
    catch (Exception ex)
    {
        log.LogError($"Error inicializando DB: {ex.Message}");
    }
}

// 3. MIDDLEWARES
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();

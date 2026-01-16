using AtlanticCity.Servicios.Identidad.Aplicacion.Interfaces;
using AtlanticCity.Servicios.Identidad.Infraestructura.Seguridad;
using AtlanticCity.Compartido.Persistencia;
using AtlanticCity.Servicios.Identidad.Core.Interfaces;
using AtlanticCity.Servicios.Identidad.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Service configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// Inyección de Dependencias
builder.Services.AddDbContext<ContextoBaseDatos>(options =>
    options.UseSqlServer(connectionString, x => x.MigrationsHistoryTable("__IdentityMigrationsHistory")));

builder.Services.AddSingleton<IServicioToken, ServicioToken>(); 
builder.Services.AddScoped<IUsuarioRepositorio, RepositorioUsuario>();

// Register MediatR handlers
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var log = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ContextoBaseDatos>();
        await context.Database.MigrateAsync();
        await InicializadorBaseDatos.InicializarAsync(connectionString, log);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "DB Initialization Error");
    }
}

// Middleware configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();

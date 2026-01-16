using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using AtlanticCity.Compartido.Mensajeria;
using AtlanticCity.Compartido.Mensajeria.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURACIÓN DE SEGURIDAD (REQUERIMIENTO PDF - VALIDACIÓN JWT)
// El Gateway centraliza la seguridad: valida el token antes de que toque cualquier microservicio.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(opciones =>
{
    // Política por defecto: Se requiere estar autenticado para cualquier ruta protegida
    opciones.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
    
    // Política Senior: Solo administradores pueden realizar cargas masivas
    opciones.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// 2. CONFIGURACIÓN DE YARP (Yet Another Reverse Proxy)
// Cargamos la configuración directamente desde appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 2. SIGNALR: COMUNICACIÓN EN TIEMPO REAL
// Permite que el servidor le "hable" al navegador (React) sin que el navegador pregunte.
builder.Services.AddSignalR();

// 3. INYECCIÓN DE DEPENDENCIAS: MENSAJERÍA (RABBITMQ)
// El Gateway necesita RabbitMQ para "escuchar" cuando un archivo termina de procesarse.
builder.Services.AddSingleton<IServicioMensajeria, ServicioRabbitMQ>();

// 4. CORS (Cross-Origin Resource Sharing)
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5174" };

builder.Services.AddCors(opciones => opciones.AddPolicy("PermitirReact", p => 
    p.WithOrigins(allowedOrigins) 
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials())); // Necesario para WebSockets / SignalR

var app = builder.Build();

app.UseCors("PermitirReact");

// IMPORTANTE: El orden importa. Primero autenticamos, luego autorizamos.
app.UseAuthentication();
app.UseAuthorization();

// 5. MAPEOS (HUBS Y PROXY)
// Mapea el canal de comunicación en vivo para notificaciones.
app.MapHub<NotificacionesHub>("/hub-notificaciones");

// Activa el motor de redirección (YARP). A partir de aquí, el Gateway redirige el tráfico.
app.MapReverseProxy();


var servicioMensajeria = app.Services.GetRequiredService<IServicioMensajeria>();
var hubContext = app.Services.GetRequiredService<IHubContext<NotificacionesHub>>();

_ = Task.Run(async () =>
{
    await servicioMensajeria.SuscribirseAsync<MensajeProcesamientoArchivo>("cola_signalr", async (mensaje) =>
    {
        // Esto envía el aviso al Frontend por WebSockets en TIEMPO REAL
        await hubContext.Clients.All.SendAsync("ArchivoProcesado", mensaje);
    });
});

app.Run();

// Clase obligatoria para que SignalR sepa que existe un "Hub" o canal de comunicación.
public class NotificacionesHub : Hub { }

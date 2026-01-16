using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using AtlanticCity.Compartido.Mensajeria;
using AtlanticCity.Compartido.Mensajeria.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Security configuration: JWT validation
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
    
    // Role-based authorization policy
    opciones.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// YARP Proxy Configuration
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSignalR();
builder.Services.AddSingleton<IServicioMensajeria, ServicioRabbitMQ>();

// CORS Configuration
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5174" };

builder.Services.AddCors(opciones => opciones.AddPolicy("PermitirReact", p => 
    p.WithOrigins(allowedOrigins) 
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

var app = builder.Build();

app.UseCors("PermitirReact");

app.UseAuthentication();
app.UseAuthorization();

// Routing: Hubs & Proxy
app.MapHub<NotificacionesHub>("/hub-notificaciones");
app.MapReverseProxy();

// SignalR Real-time Updates Subscriber
var messagingService = app.Services.GetRequiredService<IServicioMensajeria>();
var hubContext = app.Services.GetRequiredService<IHubContext<NotificacionesHub>>();

_ = Task.Run(async () =>
{
    await messagingService.SuscribirseAsync<MensajeProcesamientoArchivo>("cola_signalr", async (mensaje) =>
    {
        await hubContext.Clients.All.SendAsync("ArchivoProcesado", mensaje);
    });
});

app.Run();

public class NotificacionesHub : Hub { }

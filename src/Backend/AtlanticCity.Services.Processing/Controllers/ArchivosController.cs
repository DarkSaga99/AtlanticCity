using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MediatR;
using AtlanticCity.Services.Processing.Application.Commands;
using AtlanticCity.Services.Processing.Application.Queries;
using System.Threading.Tasks;
using System;

namespace AtlanticCity.Servicios.Procesamiento.Presentacion.Controladores
{
    // CONTROLADOR: Ahora solo actúa como punto de entrada (Capa de Presentación)
    [Route("api/[controller]")]
    [ApiController]
    public class ArchivosController : ControllerBase
    {
        private readonly IMediator _mediator; // Mediador inyectado para CQRS
        private readonly IConfiguration _config; // Inyectado para configuración dinámica

        public ArchivosController(IMediator mediator, IConfiguration config)
        {
            _mediator = mediator; // Delegamos la ejecución a los Handlers correspondientes
            _config = config;
        }

        // QUERY: Obtener seguimiento (Lectura de Productos)
        [HttpGet("seguimiento")]
        public async Task<IActionResult> GetSeguimiento()
        {
            var resultado = await _mediator.Send(new GetSeguimientoQuery());
            return Ok(resultado);
        }

        // QUERY: Obtener Historial de Lotes
        [HttpGet("historial")]
        public async Task<IActionResult> GetHistorial()
        {
            var resultado = await _mediator.Send(new GetHistorialQuery());
            return Ok(resultado);
        }

        // QUERY: Obtener Log de Alertas
        [HttpGet("alertas")]
        public async Task<IActionResult> GetAlertas()
        {
            var resultado = await _mediator.Send(new GetAlertasQuery());
            return Ok(resultado);
        }

        // QUERY: Obtener productos de un lote específico
        [HttpGet("{id}/productos")]
        public async Task<IActionResult> GetProductosLote(Guid id)
        {
            var resultado = await _mediator.Send(new GetProductosPorLoteQuery(id));
            return Ok(resultado);
        }

        // COMMAND: Subir archivo (Escritura/Procesamiento)
        [HttpPost("subir")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("limite_subida")]
        public async Task<IActionResult> Subir(IFormFile archivo, [FromQuery] string correoUsuario)
        {
            var comando = new SubirArchivoCommand 
            { 
                Archivo = archivo, 
                CorreoUsuario = correoUsuario 
            };

            try 
            {
                var loteId = await _mediator.Send(comando);
                return Ok(new { LoteId = loteId, Status = "PENDIENTE" }); // Estado inicial
            }
            catch (ArgumentException ex) // Capturar errores de validación del handler
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}

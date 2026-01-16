using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MediatR;
using AtlanticCity.Services.Processing.Application.Commands;
using AtlanticCity.Services.Processing.Application.Queries;
using System.Threading.Tasks;
using System;

namespace AtlanticCity.Servicios.Procesamiento.Presentacion.Controladores
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArchivosController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _config;

        public ArchivosController(IMediator mediator, IConfiguration config)
        {
            _mediator = mediator;
            _config = config;
        }

        [HttpGet("seguimiento")]
        public async Task<IActionResult> GetSeguimiento()
        {
            var resultado = await _mediator.Send(new GetSeguimientoQuery());
            return Ok(resultado);
        }

        [HttpGet("historial")]
        public async Task<IActionResult> GetHistorial()
        {
            var resultado = await _mediator.Send(new GetHistorialQuery());
            return Ok(resultado);
        }

        [HttpGet("alertas")]
        public async Task<IActionResult> GetAlertas()
        {
            var resultado = await _mediator.Send(new GetAlertasQuery());
            return Ok(resultado);
        }

        [HttpGet("{id}/productos")]
        public async Task<IActionResult> GetProductosLote(Guid id)
        {
            var resultado = await _mediator.Send(new GetProductosPorLoteQuery(id));
            return Ok(resultado);
        }

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
                return Ok(new { LoteId = loteId, Status = "PENDIENTE" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}

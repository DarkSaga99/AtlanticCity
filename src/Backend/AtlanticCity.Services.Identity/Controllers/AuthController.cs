using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AtlanticCity.Servicios.Identidad.Aplicacion.Comandos;
using AtlanticCity.Servicios.Identidad.Aplicacion.Dtos;

namespace AtlanticCity.Services.Identity.Controllers
{
    /* 
       =================================================================================
       ATLANTIC CITY - AUTH CONTROLLER (PRESENTATION LAYER)
       =================================================================================
       Este controlador ahora sigue el patrón CLEAN ARCHITECTURE.
       NO tiene lógica de negocio. NO tiene SQL. 
       Solo delega la intención a MediatR (CQRS).
       =================================================================================
    */
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            try 
            {
                var resultado = await _mediator.Send(new LoginCommand(request.Username, request.Password));
                
                if (resultado == null)
                    return Unauthorized("Credenciales Incorrectas");

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, Detail = ex.InnerException?.Message });
            }
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            try 
            {
                var resultado = await _mediator.Send(new RegistrarUsuarioCommand(request.Username, request.Email, request.Password));
                
                if (!resultado.Exito)
                    return BadRequest(new { Error = resultado.Error });

                return Ok("Usuario registrado exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, Detail = ex.InnerException?.Message });
            }
        }

        // POST api/auth/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            var resultado = await _mediator.Send(new RefreshTokenCommand(request.AccessToken, request.RefreshToken));
            
            if (resultado == null)
                return BadRequest("Token de refresco inválido o expirado");

            return Ok(resultado);
        }
    }
}

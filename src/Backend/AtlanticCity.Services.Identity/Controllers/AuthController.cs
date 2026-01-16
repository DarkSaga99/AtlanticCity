using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AtlanticCity.Servicios.Identidad.Aplicacion.Comandos;
using AtlanticCity.Servicios.Identidad.Aplicacion.Dtos;

namespace AtlanticCity.Services.Identity.Controllers
{

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
                    return Unauthorized("Invalid credentials");

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            try 
            {
                var resultado = await _mediator.Send(new RegistrarUsuarioCommand(request.Username, request.Email, request.Password));
                
                if (!resultado.Exito)
                    return BadRequest(new { Error = resultado.Error });

                return Ok("User registered successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            var resultado = await _mediator.Send(new RefreshTokenCommand(request.AccessToken, request.RefreshToken));
            
            if (resultado == null)
                return BadRequest("Invalid or expired refresh token");

            return Ok(resultado);
        }
    }
}

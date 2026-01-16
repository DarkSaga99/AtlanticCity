using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AtlanticCity.Servicios.Identidad.Aplicacion.Comandos;
using AtlanticCity.Servicios.Identidad.Aplicacion.Dtos;
using AtlanticCity.Servicios.Identidad.Aplicacion.Interfaces;
using AtlanticCity.Servicios.Identidad.Core.Interfaces;

namespace AtlanticCity.Servicios.Identidad.Aplicacion.Handlers
{
    // HANDLER: Orquestador del proceso de Login.
    // Aquí vive la lógica de orquestación, desacoplada del controlador.
    public class LoginHandler : IRequestHandler<LoginCommand, TokenDto?>
    {
        private readonly IUsuarioRepositorio _repositorio;
        private readonly IServicioToken _servicioToken;

        public LoginHandler(IUsuarioRepositorio repositorio, IServicioToken servicioToken)
        {
            _repositorio = repositorio;
            _servicioToken = servicioToken;
        }

        public async Task<TokenDto?> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscar usuario
            var usuario = await _repositorio.ObtenerPorNombreUsuarioAsync(request.Username);

            // 2. Validar (En producción usar BCrypt/Argon2 para comparar hashes)
            if (usuario == null || usuario.ClaveHash != request.Password)
                return null;

            // 3. Generar Tokens
            var claims = new[] 
            { 
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.Role) // <--- REQUERIMIENTO SENIOR: ROLES
            };
            var accessToken = _servicioToken.GenerarTokenAcceso(claims);
            var refreshToken = _servicioToken.GenerarTokenRefresco();

            // 4. Actualizar persistencia con el nuevo Refresh Token
            usuario.TokenRefresco = refreshToken;
            usuario.FechaExpiracionRefresco = DateTime.UtcNow.AddDays(7);
            await _repositorio.ActualizarAsync(usuario);

            return new TokenDto(accessToken, refreshToken, usuario.Email ?? "");
        }
    }
}

using System;
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
    // HANDLER: Lógica de renovación de tokens (Keep-Alive de seguridad).
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, TokenDto?>
    {
        private readonly IUsuarioRepositorio _repositorio;
        private readonly IServicioToken _servicioToken;

        public RefreshTokenHandler(IUsuarioRepositorio repositorio, IServicioToken servicioToken)
        {
            _repositorio = repositorio;
            _servicioToken = servicioToken;
        }

        public async Task<TokenDto?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // 1. Extraer principal del token muerto
            var principal = _servicioToken.ObtenerPrincipalDeTokenExpirado(request.AccessToken);
            if (principal?.Identity?.Name == null) return null;

            // 2. Buscar usuario
            var usuario = await _repositorio.ObtenerPorNombreUsuarioAsync(principal.Identity.Name);

            // 3. Validar consistencia del Refresh Token
            if (usuario == null || 
                usuario.TokenRefresco != request.RefreshToken || 
                usuario.FechaExpiracionRefresco <= DateTime.UtcNow)
                return null;

            // 4. Generar nuevos tokens (Rotación de tokens de refresco por seguridad)
            var newAccessToken = _servicioToken.GenerarTokenAcceso(principal.Claims);
            var newRefreshToken = _servicioToken.GenerarTokenRefresco();

            usuario.TokenRefresco = newRefreshToken;
            await _repositorio.ActualizarAsync(usuario);

            return new TokenDto(newAccessToken, newRefreshToken, usuario.Email ?? "");
        }
    }
}

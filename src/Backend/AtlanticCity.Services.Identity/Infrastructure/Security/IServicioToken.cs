using System.Collections.Generic;
using System.Security.Claims;

namespace AtlanticCity.Servicios.Identidad.Aplicacion.Interfaces
{
    // Contrato para manejar la seguridad mediante JWT y Refresh Tokens
    public interface IServicioToken
    {
        // Crea un token de acceso (JWT) corto (15 min) para el usuario
        string GenerarTokenAcceso(IEnumerable<Claim> claims);

        // Crea un código aleatorio y seguro para renovar sesiones
        string GenerarTokenRefresco();

        // Recupera al usuario de un token aunque ya haya caducado (necesario para el Refresh)
        ClaimsPrincipal ObtenerPrincipalDeTokenExpirado(string token);
    }
}

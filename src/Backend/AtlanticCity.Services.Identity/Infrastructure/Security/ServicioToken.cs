using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AtlanticCity.Servicios.Identidad.Aplicacion.Interfaces;

namespace AtlanticCity.Servicios.Identidad.Infraestructura.Seguridad
{
    // IMPLEMENTACIÓN: Lógica criptográfica para manejo de identidad.
    public class ServicioToken : IServicioToken
    {
        private readonly IConfiguration _configuracion;

        public ServicioToken(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        public string GenerarTokenAcceso(IEnumerable<Claim> claims)
        {
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuracion["Jwt:Key"] ?? "LlaveSecretaDePruebaAtlanticCity123!"));
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuracion["Jwt:Issuer"],
                audience: _configuracion["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public string GenerarTokenRefresco()
        {
            var numerosAleatorios = new byte[32];
            using var generador = RandomNumberGenerator.Create();
            generador.GetBytes(numerosAleatorios);
            return Convert.ToBase64String(numerosAleatorios);
        }

        public ClaimsPrincipal ObtenerPrincipalDeTokenExpirado(string token)
        {
            var parametrosValidacion = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuracion["Jwt:Key"] ?? "LlaveSecretaDePruebaAtlanticCity123!")),
                ValidateLifetime = false 
            };

            var manejadorTokens = new JwtSecurityTokenHandler();
            var principal = manejadorTokens.ValidateToken(token, parametrosValidacion, out SecurityToken tokenSeguridad);

            if (tokenSeguridad is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("El token no tiene un formato válido");

            return principal;
        }
    }
}

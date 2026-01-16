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
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public string GenerarTokenRefresco()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal ObtenerPrincipalDeTokenExpirado(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuracion["Jwt:Key"] ?? "LlaveSecretaDePruebaAtlanticCity123!")),
                ValidateLifetime = false 
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token format");

            return principal;
        }
    }
}

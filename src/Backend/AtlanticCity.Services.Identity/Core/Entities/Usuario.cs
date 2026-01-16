using System;

namespace AtlanticCity.Servicios.Identidad.Core.Entidades
{
    // ENTIDAD: Representa un usuario en el sistema.
    // Sigue el principio de Domain Driven Design (DDD) al estar en el corazón del microservicio.
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; }
        public string ClaveHash { get; set; }
        public string? Email { get; set; }
        public string? TokenRefresco { get; set; }
        public DateTime? FechaExpiracionRefresco { get; set; }
        public string Role { get; set; } = "User";
    }
}

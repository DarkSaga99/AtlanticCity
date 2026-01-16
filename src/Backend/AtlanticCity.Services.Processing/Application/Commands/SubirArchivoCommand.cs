using MediatR;
using Microsoft.AspNetCore.Http;
using System;

namespace AtlanticCity.Services.Processing.Application.Commands
{
    // COMMAND: Representa la intención de realizar una acción (Escribir/Modificar)
    // Según CQRS, los Comandos no deben devolver datos complejos, pero aquí devolvemos el ID del lote
    public class SubirArchivoCommand : IRequest<Guid>
    {
        // El archivo que se desea procesar
        public IFormFile Archivo { get; set; } = null!;
        // El correo del usuario que realiza la acción (para auditoría)
        public string CorreoUsuario { get; set; } = string.Empty;
    }
}

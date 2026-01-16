using MediatR;
using System.Collections.Generic;
using AtlanticCity.Services.Processing.Application.Dtos;

namespace AtlanticCity.Services.Processing.Application.Queries
{
    // QUERY: Representa la intención de Leer datos (Sin modificar el estado)
    // Devuelve una lista de DTOs con los registros de trazabilidad
    public class GetSeguimientoQuery : IRequest<IEnumerable<SeguimientoDto>>
    {
        // En consultas simples no solemos necesitar parámetros
    }
}

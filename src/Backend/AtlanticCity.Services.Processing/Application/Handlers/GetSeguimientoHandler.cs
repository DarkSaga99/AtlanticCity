using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AtlanticCity.Services.Processing.Application.Queries;
using AtlanticCity.Services.Processing.Application.Dtos;
using AtlanticCity.Servicios.Procesamiento.Core.Interfaces;

namespace AtlanticCity.Services.Processing.Application.Handlers
{
    // HANDLER DE CONSULTA: Lectura de datos para el panel de seguimiento
    // Ahora delega al repositorio en lugar de usar SqlConnection directamente
    public class GetSeguimientoHandler : IRequestHandler<GetSeguimientoQuery, IEnumerable<SeguimientoDto>>
    {
        private readonly ILoteConsultas _consultas;

        public GetSeguimientoHandler(ILoteConsultas consultas)
        {
            _consultas = consultas;
        }

        public async Task<IEnumerable<SeguimientoDto>> Handle(GetSeguimientoQuery request, CancellationToken cancellationToken)
        {
            // Delegamos la consulta a la interfaz específica de lectura (ISP)
            return await _consultas.ObtenerSeguimientoAsync();
        }
    }
}

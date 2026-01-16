using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AtlanticCity.Services.Processing.Application.Queries;
using AtlanticCity.Services.Processing.Application.Dtos;
using AtlanticCity.Servicios.Procesamiento.Core.Interfaces;

namespace AtlanticCity.Services.Processing.Application.Handlers
{
    // Handles batch history queries
    public class GetHistorialHandler : IRequestHandler<GetHistorialQuery, IEnumerable<HistorialDto>>
    {
        private readonly ILoteConsultas _consultas;

        public GetHistorialHandler(ILoteConsultas consultas)
        {
            _consultas = consultas;
        }

        public async Task<IEnumerable<HistorialDto>> Handle(GetHistorialQuery request, CancellationToken cancellationToken)
        {
            return await _consultas.ObtenerHistorialAsync();
        }
    }

    // HANDLER: Consulta de alertas del log de auditoría
    public class GetAlertasHandler : IRequestHandler<GetAlertasQuery, IEnumerable<AlertaDto>>
    {
        private readonly ILoteConsultas _consultas;

        public GetAlertasHandler(ILoteConsultas consultas)
        {
            _consultas = consultas;
        }

        public async Task<IEnumerable<AlertaDto>> Handle(GetAlertasQuery request, CancellationToken cancellationToken)
        {
            return await _consultas.ObtenerAlertasAsync();
        }
    }
}

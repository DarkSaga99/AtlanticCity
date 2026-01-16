using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AtlanticCity.Services.Processing.Application.Dtos;
using AtlanticCity.Services.Processing.Application.Queries;
using AtlanticCity.Servicios.Procesamiento.Core.Interfaces;

namespace AtlanticCity.Services.Processing.Application.Handlers
{
    public class GetProductosPorLoteHandler : IRequestHandler<GetProductosPorLoteQuery, IEnumerable<SeguimientoDto>>
    {
        private readonly ILoteConsultas _consultas;

        public GetProductosPorLoteHandler(ILoteConsultas consultas)
        {
            _consultas = consultas;
        }

        public async Task<IEnumerable<SeguimientoDto>> Handle(GetProductosPorLoteQuery request, CancellationToken cancellationToken)
        {
            return await _consultas.ObtenerProductosPorLoteAsync(request.LoteId);
        }
    }
}

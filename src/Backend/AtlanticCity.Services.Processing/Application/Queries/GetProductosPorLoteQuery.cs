using MediatR;
using System;
using System.Collections.Generic;
using AtlanticCity.Services.Processing.Application.Dtos;

namespace AtlanticCity.Services.Processing.Application.Queries
{
    public class GetProductosPorLoteQuery : IRequest<IEnumerable<SeguimientoDto>>
    {
        public Guid LoteId { get; }

        public GetProductosPorLoteQuery(Guid loteId)
        {
            LoteId = loteId;
        }
    }
}

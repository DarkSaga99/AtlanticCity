using MediatR;
using System.Collections.Generic;
using AtlanticCity.Services.Processing.Application.Dtos;

namespace AtlanticCity.Services.Processing.Application.Queries
{
    public class GetHistorialQuery : IRequest<IEnumerable<HistorialDto>> { }
    public class GetAlertasQuery : IRequest<IEnumerable<AlertaDto>> { }
}

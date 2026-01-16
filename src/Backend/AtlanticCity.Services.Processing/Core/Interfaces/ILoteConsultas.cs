using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtlanticCity.Services.Processing.Application.Dtos;

namespace AtlanticCity.Servicios.Procesamiento.Core.Interfaces
{
    // SOLID: ISP - Interfaz especializada en consultas (Lectura) para desacoplar la UI de la persistencia de comandos.
    public interface ILoteConsultas
    {
        Task<IEnumerable<SeguimientoDto>> ObtenerSeguimientoAsync();
        Task<IEnumerable<SeguimientoDto>> ObtenerProductosPorLoteAsync(Guid batchId);
        Task<IEnumerable<HistorialDto>> ObtenerHistorialAsync();
        Task<IEnumerable<AlertaDto>> ObtenerAlertasAsync();
    }
}

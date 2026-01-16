using System;
using System.Threading.Tasks;

namespace AtlanticCity.Servicios.Procesamiento.Core.Interfaces
{
    // SOLID: ISP/SRP - Ahora esta interfaz solo se enfoca en comandos de gestión de Lotes.
    public interface IRepositorioLote
    {
        Task CrearLoteAsync(Guid id, string nombreArchivo, string correoUsuario);
        Task ActualizarEstadoAsync(Guid id, string estado, int total = 0, int success = 0, int error = 0);
        Task<bool> IntentarBloquearPeriodoAsync(Guid batchId, string period);
    }
}

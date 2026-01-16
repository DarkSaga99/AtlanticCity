using System;
using System.Threading.Tasks;

namespace AtlanticCity.Workers.BulkLoad.Core.Interfaces
{
    public interface IRepositorioLote
    {
        Task ActualizarEstadoAsync(Guid id, string estado, int total = 0, int exitosos = 0, int errores = 0);
        Task<bool> IntentarBloquearPeriodoAsync(Guid id, string periodo);
        Task RegistrarAuditoriaAsync(string accion, string detalles, string usuario, Guid? loteId = null);
    }
}

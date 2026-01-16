using System;
using System.Threading.Tasks;

namespace AtlanticCity.Workers.Notifications.Core.Interfaces
{
    public interface IRepositorioNotificacion
    {
        Task FinalizarNotificacionAsync(Guid batchId, string usuario, string nombreArchivo);
    }
}

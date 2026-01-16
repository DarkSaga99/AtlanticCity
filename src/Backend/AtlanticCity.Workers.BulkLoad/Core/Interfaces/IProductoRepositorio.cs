using System;
using System.Threading.Tasks;

namespace AtlanticCity.Workers.BulkLoad.Core.Interfaces
{
    public interface IProductoRepositorio
    {
        Task ProcesarProductoAsync(Guid batchId, string codigo, string nombre, string periodo);
        Task AsegurarBaseDatosAsync();
    }
}

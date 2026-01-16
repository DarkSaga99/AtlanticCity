using System.IO;
using System.Threading.Tasks;

namespace AtlanticCity.Workers.BulkLoad.Core.Interfaces
{
    // SOLID: SRP - Esta interfaz solo se encarga de obtener el flujo de datos del archivo.
    public interface IDescargadorArchivos
    {
        Task<Stream> DescargarArchivoAsync(string url);
    }
}

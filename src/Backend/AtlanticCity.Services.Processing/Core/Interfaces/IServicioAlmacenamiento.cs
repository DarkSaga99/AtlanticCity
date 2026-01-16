using System.IO;
using System.Threading.Tasks;

namespace AtlanticCity.Servicios.Procesamiento.Core.Interfaces
{
    // INTERFAZ: Contrato para el servicio de almacenamiento de archivos.
    // Vive en Core porque es una abstracción del dominio (DIP).
    // La implementación concreta (SeaweedFS) vive en Infrastructure.
    public interface IServicioAlmacenamiento
    {
        // Sube un archivo y devuelve la URL donde quedó guardado
        Task<string> SubirArchivoAsync(Stream flujoArchivo, string nombreArchivo);
    }
}

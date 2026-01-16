using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AtlanticCity.Workers.BulkLoad.Core.Interfaces
{
    public class FilaArchivo
    {
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Periodo { get; set; }
        public int NumeroFila { get; set; }
    }

    // SOLID: SRP - Esta interfaz solo se encarga de interpretar el formato del archivo.
    public interface IParseadorArchivo
    {
        IEnumerable<FilaArchivo> LeerFilas(Stream stream, string nombreArchivo);
    }
}

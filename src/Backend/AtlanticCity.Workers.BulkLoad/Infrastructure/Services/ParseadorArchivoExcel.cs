using System.Collections.Generic;
using System.IO;
using ExcelDataReader;
using AtlanticCity.Workers.BulkLoad.Core.Interfaces;

namespace AtlanticCity.Workers.BulkLoad.Infrastructure.Services
{
    public class ParseadorArchivoExcel : IParseadorArchivo
    {
        public IEnumerable<FilaArchivo> LeerFilas(Stream stream, string nombreArchivo)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            
            using var reader = (nombreArchivo?.EndsWith(".csv") ?? false)
                ? ExcelReaderFactory.CreateCsvReader(stream)
                : ExcelReaderFactory.CreateReader(stream);

            int filaActual = 0;
            while (reader.Read())
            {
                filaActual++;
                if (filaActual == 1) continue; // Saltar cabecera

                yield return new FilaArchivo
                {
                    Codigo = reader.GetValue(0)?.ToString(),
                    Nombre = reader.GetValue(1)?.ToString(),
                    Periodo = reader.GetValue(2)?.ToString(),
                    NumeroFila = filaActual
                };
            }
        }
    }
}

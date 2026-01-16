using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using AtlanticCity.Workers.BulkLoad.Core.Interfaces;

namespace AtlanticCity.Workers.BulkLoad.Infraestructura.Persistencia
{
    // Implements product processing using specialized stored procedures for performance.
    public class RepositorioProductoEF : IProductoRepositorio
    {
        private readonly BulkLoadDbContext _context;

        public RepositorioProductoEF(BulkLoadDbContext context)
        {
            _context = context;
        }

        public async Task ProcesarProductoAsync(Guid batchId, string codigo, string nombre, string periodo)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_InsertProductUnique {0}, {1}, {2}, {3}",
                batchId, codigo, nombre, periodo);
        }

        public async Task AsegurarBaseDatosAsync()
        {
            await Task.CompletedTask;
        }
    }
}

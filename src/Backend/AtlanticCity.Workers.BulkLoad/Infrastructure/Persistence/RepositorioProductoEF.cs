using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using AtlanticCity.Workers.BulkLoad.Core.Interfaces;

namespace AtlanticCity.Workers.BulkLoad.Infraestructura.Persistencia
{
    /* 
       =================================================================================
       REPOSITORIO PRODUCTO - REFACTORIZADO A EF CORE (Llamada a SP)
       =================================================================================
       Mantenemos el alto rendimiento del Stored Procedure "SP_InsertProductUnique"
       pero lo invocamos a través del BulkLoadDbContext de EF Core para eliminar
       la dependencia de Dapper y unificar la persistencia.
       =================================================================================
    */
    public class RepositorioProductoEF : IProductoRepositorio
    {
        private readonly BulkLoadDbContext _context;

        public RepositorioProductoEF(BulkLoadDbContext context)
        {
            _context = context;
        }

        public async Task ProcesarProductoAsync(Guid batchId, string codigo, string nombre, string periodo)
        {
            // Invocamos el SP especializado en carga masiva con parámetros directos
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

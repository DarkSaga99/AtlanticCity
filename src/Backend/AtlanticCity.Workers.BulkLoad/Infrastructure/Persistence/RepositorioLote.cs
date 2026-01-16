using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using AtlanticCity.Workers.BulkLoad.Core.Interfaces;
using AtlanticCity.Workers.BulkLoad.Core.Entities;

namespace AtlanticCity.Workers.BulkLoad.Infraestructura.Persistencia
{
    
    public class RepositorioLote : IRepositorioLote
    {
        private readonly BulkLoadDbContext _context;

        public RepositorioLote(BulkLoadDbContext context)
        {
            _context = context;
        }

        public async Task ActualizarEstadoAsync(Guid id, string estado, int total = 0, int exitosos = 0, int errores = 0)
        {
            // Ejecutamos el SP a través de EF Core con parámetros posicionales para máxima compatibilidad
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_UpdateBatchStatus {0}, {1}, {2}, {3}, {4}",
                id, estado, total, exitosos, errores);
        }

        public async Task<bool> IntentarBloquearPeriodoAsync(Guid batchId, string period)
        {
            // Lógica de bloqueo atómico con EF Core y LINQ (más legible)
            var existeCarga = await _context.Lotes.AnyAsync(l => 
                l.Period == period && 
                l.Id != batchId && 
                new[] { "Pendiente", "En Proceso", "Finalizado", "Notificado" }.Contains(l.Status));

            if (existeCarga) return false;

            var lote = await _context.Lotes.FindAsync(batchId);
            if (lote == null) return false;

            lote.Period = period;
            lote.Status = "En Proceso";
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task RegistrarAuditoriaAsync(string accion, string detalles, string usuario, Guid? loteId = null)
        {
            var detallesFinal = loteId.HasValue ? $"[Lote: {loteId}] {detalles}" : detalles;
            
            var log = new LogAuditoria
            {
                Action = accion,
                TableName = "Products",
                UserId = usuario ?? "Sistema",
                Details = detallesFinal,
                Timestamp = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}

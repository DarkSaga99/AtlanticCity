using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AtlanticCity.Workers.Notifications.Core.Interfaces;
using AtlanticCity.Workers.Notifications.Core.Entities;

namespace AtlanticCity.Workers.Notifications.Infrastructure.Persistence
{
    // Repository for notification persistence and batch lifecycle finalization.
    public class RepositorioNotificacion : IRepositorioNotificacion
    {
        private readonly NotificacionesDbContext _context;

        public RepositorioNotificacion(NotificacionesDbContext context)
        {
            _context = context;
        }

        public async Task FinalizarNotificacionAsync(Guid batchId, string usuario, string nombreArchivo)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var productos = await _context.Productos
                    .Where(p => p.BatchId == batchId && (p.Status == "Cargado" || p.Status == "Procesado"))
                    .ToListAsync();

                foreach (var producto in productos)
                {
                    producto.Status = "Notificado";
                }

                var lote = await _context.Lotes.FindAsync(batchId);
                if (lote != null)
                {
                    lote.Status = "Notificado";
                }

                _context.Logs.Add(new LogAuditoria
                {
                    Action = "Notification Sent",
                    TableName = "ProcessBatches",
                    UserId = usuario,
                    Details = $"Confirmation email sent for '{nombreArchivo}'",
                    Timestamp = DateTime.Now
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

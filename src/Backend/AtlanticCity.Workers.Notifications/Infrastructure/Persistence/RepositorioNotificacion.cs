using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AtlanticCity.Workers.Notifications.Core.Interfaces;
using AtlanticCity.Workers.Notifications.Core.Entities;

namespace AtlanticCity.Workers.Notifications.Infrastructure.Persistence
{
    /* 
       =================================================================================
       REPOSITORIO NOTIFICACIÓN - REFACTORIZADO A EF CORE
       =================================================================================
       Eliminamos Dapper y SQL nativo para usar EF Core, manteniendo la consistencia
       con el resto del sistema y asegurando transaccionalidad limpia.
       =================================================================================
    */
    public class RepositorioNotificacion : IRepositorioNotificacion
    {
        private readonly NotificacionesDbContext _context;

        public RepositorioNotificacion(NotificacionesDbContext context)
        {
            _context = context;
        }

        public async Task FinalizarNotificacionAsync(Guid batchId, string usuario, string nombreArchivo)
        {
            // Iniciamos una transacción con EF Core
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Marcar productos como 'Notificado'
                var productos = await _context.Productos
                    .Where(p => p.BatchId == batchId && p.Status == "Cargado")
                    .ToListAsync();

                foreach (var producto in productos)
                {
                    producto.Status = "Notificado";
                }

                // 2. Marcar lote maestro como 'Notificado'
                var lote = await _context.Lotes.FindAsync(batchId);
                if (lote != null)
                {
                    lote.Status = "Notificado";
                }

                // 3. Registrar Auditoría
                var log = new LogAuditoria
                {
                    Action = "Notificación Enviada",
                    TableName = "ProcessBatches",
                    UserId = usuario,
                    Details = $"Se envió correo de confirmación para el archivo '{nombreArchivo}'.",
                    Timestamp = DateTime.Now
                };

                _context.Logs.Add(log);

                // Guardamos todos los cambios en un solo viaje a la base de datos
                await _context.SaveChangesAsync();
                
                // Confirmamos la transacción
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // En caso de error, revertimos todo automáticamente al salir del bloque using o explícitamente:
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

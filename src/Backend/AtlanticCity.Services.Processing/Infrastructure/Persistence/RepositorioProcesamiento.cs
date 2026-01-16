using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AtlanticCity.Servicios.Procesamiento.Core.Interfaces;
using AtlanticCity.Services.Processing.Application.Dtos;
using AtlanticCity.Services.Processing.Core.Entities;
using AtlanticCity.Services.Processing.Infrastructure.Persistence;

namespace AtlanticCity.Servicios.Procesamiento.Infraestructura.Persistencia
{
    // SOLID: ISP - Implementamos interfaces segregadas para comandos y consultas.
    // Aunque la implementación física sea la misma clase, los clientes solo ven lo que necesitan.
    public class RepositorioProcesamiento : IRepositorioLote, IAuditoriaRepositorio, ILoteConsultas
    {
        private readonly ProcesamientoDbContext _context;

        public RepositorioProcesamiento(ProcesamientoDbContext context)
        {
            _context = context;
        }

        // --- COMANDOS: IRepositorioLote ---
        public async Task CrearLoteAsync(Guid id, string nombreArchivo, string correoUsuario)
        {
            var lote = new LoteProceso
            {
                Id = id,
                FileName = nombreArchivo,
                UserEmail = correoUsuario,
                Status = "Pendiente",
                StartTime = DateTime.Now
            };
            _context.Lotes.Add(lote);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarEstadoAsync(Guid id, string estado, int total = 0, int success = 0, int error = 0)
        {
            var lote = await _context.Lotes.FindAsync(id);
            if (lote != null)
            {
                lote.Status = estado;
                lote.TotalRecords = total;
                lote.SuccessCount = success;
                lote.ErrorCount = error;
                if (new[] { "Finalizado", "Rechazado", "Error" }.Contains(estado))
                    lote.EndTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IntentarBloquearPeriodoAsync(Guid batchId, string period)
        {
            // Implementación lógica del bloqueo (mismo concepto que el Worker)
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

        // --- COMANDOS: IAuditoriaRepositorio ---
        public async Task RegistrarAsync(string accion, string tabla, string usuario, string detalles)
        {
            var log = new LogAuditoria
            {
                Action = accion,
                TableName = tabla,
                UserId = usuario,
                Details = detalles,
                Timestamp = DateTime.Now
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        // --- CONSULTAS: ILoteConsultas ---
        public async Task<IEnumerable<SeguimientoDto>> ObtenerSeguimientoAsync()
        {
            return await _context.Productos
                .OrderByDescending(p => p.Id)
                .Select(p => new SeguimientoDto { CodigoProducto = p.ProductCode, Nombre = p.ProductName, Period = p.Period, Estado = p.Status })
                .ToListAsync();
        }

        public async Task<IEnumerable<SeguimientoDto>> ObtenerProductosPorLoteAsync(Guid batchId)
        {
            return await _context.Productos
                .Where(p => p.BatchId == batchId)
                .OrderByDescending(p => p.Id)
                .Select(p => new SeguimientoDto { CodigoProducto = p.ProductCode, Nombre = p.ProductName, Period = p.Period, Estado = p.Status })
                .ToListAsync();
        }

        public async Task<IEnumerable<HistorialDto>> ObtenerHistorialAsync()
        {
            return await _context.Lotes
                .OrderByDescending(l => l.StartTime)
                .Select(l => new HistorialDto { Id = l.Id, FileName = l.FileName, UserEmail = l.UserEmail, Status = l.Status, CreatedAt = l.StartTime, TotalRecords = l.TotalRecords, SuccessCount = l.SuccessCount, ErrorCount = l.ErrorCount })
                .ToListAsync();
        }

        public async Task<IEnumerable<AlertaDto>> ObtenerAlertasAsync()
        {
            return await _context.Logs
                .OrderByDescending(l => l.Timestamp)
                .Select(l => new AlertaDto { Action = l.Action, Details = l.Details, User = l.UserId, CreatedAt = l.Timestamp })
                .ToListAsync();
        }
    }
}

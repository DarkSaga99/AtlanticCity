using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AtlanticCity.Compartido.Mensajeria;
using AtlanticCity.Compartido.Mensajeria.Dtos;
using AtlanticCity.Workers.BulkLoad.Core.Interfaces;

namespace AtlanticCity.Workers.BulkLoad.Application.Services
{
   
    public class ProcesadorLote : IProcesadorLote
    {
        private readonly ILogger<ProcesadorLote> _log;
        private readonly IServicioMensajeria _rabbit;
        private readonly IProductoRepositorio _repoProductos;
        private readonly IRepositorioLote _repoLotes;
        private readonly IDescargadorArchivos _descargador;
        private readonly IParseadorArchivo _parseador;

        public ProcesadorLote(
            ILogger<ProcesadorLote> log,
            IServicioMensajeria rabbit,
            IProductoRepositorio repoProductos,
            IRepositorioLote repoLotes,
            IDescargadorArchivos descargador,
            IParseadorArchivo parseador)
        {
            _log = log;
            _rabbit = rabbit;
            _repoProductos = repoProductos;
            _repoLotes = repoLotes;
            _descargador = descargador;
            _parseador = parseador;
        }

        public async Task ProcesarAsync(MensajeProcesamientoArchivo mensaje)
        {
            try
            {
                if (mensaje == null || string.IsNullOrEmpty(mensaje.UrlArchivo)) return;

                _log.LogInformation($"Processing Batch: {mensaje.IdMensaje} - File: {mensaje.NombreArchivo}");

                await _repoLotes.ActualizarEstadoAsync(mensaje.IdMensaje, "EN PROCESO");

                using var networkStream = await _descargador.DescargarArchivoAsync(mensaje.UrlArchivo);
                
                using var stream = new MemoryStream();
                await networkStream.CopyToAsync(stream);
                stream.Position = 0;
                
                var rows = new System.Collections.Generic.List<FilaArchivo>();
                foreach (var row in _parseador.LeerFilas(stream, mensaje.NombreArchivo!))
                {
                    rows.Add(row);
                }
                
                int totalRows = rows.Count;
                mensaje.TotalRegistros = totalRows;
                _log.LogInformation($"Batch {mensaje.IdMensaje}: {totalRows} rows detected");
                
                int success = 0, errors = 0, currentIndex = 0;
                var validatedPeriods = new System.Collections.Generic.HashSet<string>();

                foreach (var row in rows)
                {
                    currentIndex++;
                    try
                    {
                        if (!string.IsNullOrEmpty(row.Periodo) && !validatedPeriods.Contains(row.Periodo))
                        {
                            if (!await _repoLotes.IntentarBloquearPeriodoAsync(mensaje.IdMensaje, row.Periodo))
                            {
                                await RechazarLoteAsync(mensaje, row.Periodo);
                                return;
                            }
                            validatedPeriods.Add(row.Periodo);
                        }

                        if (string.IsNullOrEmpty(row.Codigo) || string.IsNullOrEmpty(row.Nombre))
                        {
                            errors++;
                            await _repoLotes.RegistrarAuditoriaAsync("Fila Inválida", $"La fila {row.NumeroFila} está incompleta.", mensaje.CorreoUsuario, mensaje.IdMensaje);
                        }
                        else
                        {
                            await _repoProductos.ProcesarProductoAsync(mensaje.IdMensaje, row.Codigo, row.Nombre, row.Periodo ?? "N/A");
                            success++;
                        }
                    }
                    catch (Exception) { errors++; }

                    if (currentIndex % 50 == 0) await NotificarProgresoAsync(mensaje, totalRows, currentIndex);
                }

                await FinalizarLoteAsync(mensaje, currentIndex, success, errors);
            }
            catch (Exception ex)
            {
                _log.LogError($"Batch processing error {mensaje.IdMensaje}: {ex.Message}");
                await _repoLotes.ActualizarEstadoAsync(mensaje.IdMensaje, "Error");
            }
        }

        private async Task RechazarLoteAsync(MensajeProcesamientoArchivo mensaje, string period)
        {
            await _repoLotes.ActualizarEstadoAsync(mensaje.IdMensaje, "Rechazado");
            await _repoLotes.RegistrarAuditoriaAsync("Carga Rechazada", $"Periodo {period} ya cargado.", mensaje.CorreoUsuario, mensaje.IdMensaje);
            
            mensaje.EsRechazado = true;
            mensaje.MotivoRechazo = $"Periodo {period} ya cuenta con una carga previa.";
            mensaje.EsCompleto = true;
            await _rabbit.PublicarMensajeAsync("cola_signalr", mensaje);
        }

        private async Task NotificarProgresoAsync(MensajeProcesamientoArchivo mensaje, int total, int procesados)
        {
            mensaje.TotalRegistros = total;
            mensaje.RegistrosProcesados = procesados;
            mensaje.EsCompleto = false;
            await _rabbit.PublicarMensajeAsync("cola_signalr", mensaje);
        }

        private async Task FinalizarLoteAsync(MensajeProcesamientoArchivo mensaje, int total, int success, int errors)
        {
            await _repoLotes.ActualizarEstadoAsync(mensaje.IdMensaje, "Finalizado", total, success, errors);
            await _repoLotes.RegistrarAuditoriaAsync("Carga Finalizada", $"Exitosos: {success}, Errores: {errors}.", mensaje.CorreoUsuario, mensaje.IdMensaje);

            mensaje.EsCompleto = true;
            mensaje.TotalRegistros = total;
            mensaje.RegistrosProcesados = total;
            
            await _rabbit.PublicarMensajeAsync("cola_notificaciones", mensaje);
            await _rabbit.PublicarMensajeAsync("cola_signalr", mensaje);
        }
    }
}

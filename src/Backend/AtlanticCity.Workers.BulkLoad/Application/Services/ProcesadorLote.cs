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

                _log.LogInformation($"[PROCESANDO] Lote: {mensaje.IdMensaje} - Archivo: {mensaje.NombreArchivo}");

                // 1. Notificar inicio
                await _repoLotes.ActualizarEstadoAsync(mensaje.IdMensaje, "EN PROCESO");

                // 2. Descargar archivo (Responsabilidad delegada)
                using var networkStream = await _descargador.DescargarArchivoAsync(mensaje.UrlArchivo);
                
                // 3. Crear stream buscable (MemoryStream) para el parseador
                using var stream = new MemoryStream();
                await networkStream.CopyToAsync(stream);
                stream.Position = 0;
                
                // 4. LEER TODAS LAS FILAS UNA SOLA VEZ (materializamos en lista)
                var todasLasFilas = new System.Collections.Generic.List<FilaArchivo>();
                foreach (var fila in _parseador.LeerFilas(stream, mensaje.NombreArchivo!))
                {
                    todasLasFilas.Add(fila);
                }
                
                int totalFilas = todasLasFilas.Count;
                mensaje.TotalRegistros = totalFilas;
                _log.LogInformation($"[CONTEO] Lote {mensaje.IdMensaje}: {totalFilas} filas detectadas");
                
                // 5. Procesar filas desde la lista (no desde el stream)
                int success = 0, errors = 0, filaActual = 0;
                var periodosValidados = new System.Collections.Generic.HashSet<string>();

                foreach (var fila in todasLasFilas)
                {
                    filaActual++;
                    try
                    {
                        // Validación de Periodos
                        if (!string.IsNullOrEmpty(fila.Periodo) && !periodosValidados.Contains(fila.Periodo))
                        {
                            if (!await _repoLotes.IntentarBloquearPeriodoAsync(mensaje.IdMensaje, fila.Periodo))
                            {
                                await RechazarLoteAsync(mensaje, fila.Periodo);
                                return;
                            }
                            periodosValidados.Add(fila.Periodo);
                        }

                        if (string.IsNullOrEmpty(fila.Codigo) || string.IsNullOrEmpty(fila.Nombre))
                        {
                            errors++;
                            await _repoLotes.RegistrarAuditoriaAsync("Error de Fila", $"Fila {fila.NumeroFila} inválida.", mensaje.CorreoUsuario, mensaje.IdMensaje);
                        }
                        else
                        {
                            await _repoProductos.ProcesarProductoAsync(mensaje.IdMensaje, fila.Codigo, fila.Nombre, fila.Periodo ?? "N/A");
                            success++;
                        }
                    }
                    catch (Exception) { errors++; }

                    // Notificar progreso cada 50 registros CON EL TOTAL
                    if (filaActual % 50 == 0) await NotificarProgresoAsync(mensaje, totalFilas, filaActual);
                }

                await FinalizarLoteAsync(mensaje, filaActual, success, errors);
            }
            catch (Exception ex)
            {
                _log.LogError($"[ERROR LOTE] {mensaje.IdMensaje}: {ex.Message}");
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

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AtlanticCity.Servicios.Procesamiento.Core.Interfaces;

namespace AtlanticCity.Servicios.Procesamiento.Infraestructura.Storage
{
    public class ServicioAlmacenamientoSeaweed : IServicioAlmacenamiento
    {
        private readonly HttpClient _clienteHttp;
        private readonly ILogger<ServicioAlmacenamientoSeaweed> _log;
        private readonly string _urlFiler; 

        public ServicioAlmacenamientoSeaweed(HttpClient clienteHttp, ILogger<ServicioAlmacenamientoSeaweed> log, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _clienteHttp = clienteHttp;
            _log = log;
            _urlFiler = config["SeaweedFS:FilerUrl"] 
                ?? throw new InvalidOperationException("La configuración 'SeaweedFS:FilerUrl' es requerida y no se encontró.");
        }

        public async Task<string> SubirArchivoAsync(Stream flujoArchivo, string nombreArchivo)
        {
            try
            {
                // Limpiamos el nombre del archivo para evitar problemas en la URL
                string nombreLimpio = $"{Guid.NewGuid()}_{nombreArchivo}";
                var urlFinal = $"{_urlFiler}{nombreLimpio}";

                // 2. Preparar y enviar archivo directamente al FILER
                using var contenido = new MultipartFormDataContent();
                var contenidoArchivo = new StreamContent(flujoArchivo);
                contenido.Add(contenidoArchivo, "file", nombreLimpio);

                var respuesta = await _clienteHttp.PostAsync(urlFinal, contenido);

                if (respuesta.IsSuccessStatusCode)
                {
                    _log.LogInformation($"[STORAGE] Archivo guardado en Filer: {urlFinal}");
                    return urlFinal;
                }

                throw new Exception($"Error SeaweedFS Filer: {respuesta.StatusCode} - {respuesta.ReasonPhrase}");
            }
            catch (Exception ex)
            {
                _log.LogError($"[ERROR STORAGE] Fallo crítico al subir archivo a SeaweedFS: {ex.Message}");
                throw;
            }
        }
    }
}

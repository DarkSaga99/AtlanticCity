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
                string fileName = $"{Guid.NewGuid()}_{nombreArchivo}";
                var url = $"{_urlFiler}{fileName}";

                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(flujoArchivo);
                content.Add(fileContent, "file", fileName);

                var response = await _clienteHttp.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    _log.LogInformation($"[Storage] File stored: {url}");
                    return url;
                }

                throw new Exception($"Error SeaweedFS Filer: {response.StatusCode} - {response.ReasonPhrase}");
            }
            catch (Exception ex)
            {
                _log.LogError($"[Storage Error] SeaweedFS upload fail: {ex.Message}");
                throw;
            }
        }
    }
}

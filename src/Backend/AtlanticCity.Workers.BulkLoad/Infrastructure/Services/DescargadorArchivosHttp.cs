using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AtlanticCity.Workers.BulkLoad.Core.Interfaces;

namespace AtlanticCity.Workers.BulkLoad.Infrastructure.Services
{
    public class DescargadorArchivosHttp : IDescargadorArchivos
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DescargadorArchivosHttp(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Stream> DescargarArchivoAsync(string url)
        {
            var client = _httpClientFactory.CreateClient();
            return await client.GetStreamAsync(url);
        }
    }
}

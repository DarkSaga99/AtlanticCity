using System.Threading.Tasks;
using AtlanticCity.Compartido.Mensajeria.Dtos;

namespace AtlanticCity.Workers.BulkLoad.Core.Interfaces
{
    public interface IProcesadorLote
    {
        Task ProcesarAsync(MensajeProcesamientoArchivo mensaje);
    }
}

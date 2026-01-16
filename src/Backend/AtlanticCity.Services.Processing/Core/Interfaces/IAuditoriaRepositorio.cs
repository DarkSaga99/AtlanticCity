using System.Threading.Tasks;

namespace AtlanticCity.Servicios.Procesamiento.Core.Interfaces
{
    // SOLID: SRP - Solo se encarga de persistir eventos de auditoría.
    public interface IAuditoriaRepositorio
    {
        Task RegistrarAsync(string accion, string tabla, string usuario, string detalles);
    }
}

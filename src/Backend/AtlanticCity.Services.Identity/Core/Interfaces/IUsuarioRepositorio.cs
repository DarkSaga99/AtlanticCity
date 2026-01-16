using System.Threading.Tasks;
using AtlanticCity.Servicios.Identidad.Core.Entidades;

namespace AtlanticCity.Servicios.Identidad.Core.Interfaces
{
    // REPOSITORIO: Puerto de salida para persistencia.
    // Sigue DIP (Dependency Inversion Principle) de Clean Architecture.
    public interface IUsuarioRepositorio
    {
        Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
        Task<Usuario?> ObtenerPorEmailAsync(string email);
        Task RegistrarAsync(Usuario usuario);
        Task ActualizarAsync(Usuario usuario);
    }
}

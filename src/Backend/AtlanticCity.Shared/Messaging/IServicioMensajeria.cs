using System; // Librería base de C#
using System.Threading.Tasks; // Para tareas asíncronas

namespace AtlanticCity.Compartido.Mensajeria
{
    // Interfaz que define qué puede hacer nuestro servicio de colas
    public interface IServicioMensajeria
    {
        // Envía un objeto a una cola (ej. "procesar_excel")
        Task PublicarMensajeAsync<T>(string nombreCola, T mensaje);

        // Se queda escuchando una cola para procesar lo que llegue
        Task SuscribirseAsync<T>(string nombreCola, Func<T, Task> accionAlRecibir);
    }
}

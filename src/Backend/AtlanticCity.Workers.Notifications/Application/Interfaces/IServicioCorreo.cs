using System.Threading.Tasks;

namespace AtlanticCity.Workers.Notifications.Application.Interfaces
{
    // Define la acción de enviar correo para que el sistema no dependa de una librería fija
    public interface IServicioCorreo
    {
        // Envía el correo de resumen al usuario final
        Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpo);
    }
}

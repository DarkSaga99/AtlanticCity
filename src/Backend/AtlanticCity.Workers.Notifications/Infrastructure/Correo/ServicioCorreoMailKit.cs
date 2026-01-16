using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using AtlanticCity.Workers.Notifications.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace AtlanticCity.Workers.Notifications.Infrastructure.Correo
{
    public class ServicioCorreoMailKit : IServicioCorreo
    {
        private readonly ILogger<ServicioCorreoMailKit> _log;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public ServicioCorreoMailKit(ILogger<ServicioCorreoMailKit> log, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpo)
        {
            // 1. Configuramos el mensaje (MimeKit)
            var senderEmail = _config["Email:SenderEmail"];
            var host = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var password = _config["Email:SenderPassword"];

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
            {
                throw new Exception("Configuración de correo incompleta (SenderEmail o SenderPassword faltantes).");
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Atlantic City - Sistema", senderEmail));
            email.To.Add(MailboxAddress.Parse(destinatario));
            email.Subject = asunto;
            email.Body = new TextPart("plain") { Text = cuerpo };

            using var cliente = new SmtpClient();
            try
            {
                _log.LogInformation($"[SMTP] Intentando enviar correo real a {destinatario}...");

                // 2. Conexión al servidor de Gmail (Puerto 587 con TLS)
                await cliente.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);

                // 3. Autenticación
                await cliente.AuthenticateAsync(senderEmail, password);

                // 4. Envío Real
                await cliente.SendAsync(email);
                
                _log.LogInformation($"[EXITO] Correo enviado físicamente vía Gmail a {destinatario}");
            }
            catch (Exception ex)
            {
                _log.LogError($"[ERROR SMTP] No se pudo enviar el correo: {ex.Message}");
                _log.LogWarning("[FALLBACK] Asegúrate de tener activa la 'Contraseña de Aplicación' en tu cuenta de Google.");
                
                // Si falla el envío real por falta de clave, imprimimos en consola para que no se pierda el rastro
                Console.WriteLine("\n--- COPIA DEL CORREO QUE DEBERIA HABERSE ENVIADO ---");
                Console.WriteLine($"PARA: {destinatario}");
                Console.WriteLine($"ASUNTO: {asunto}");
                Console.WriteLine($"CUERPO: {cuerpo}");
                Console.WriteLine("---------------------------------------------------\n");
            }
            finally
            {
                await cliente.DisconnectAsync(true);
            }
        }
    }
}

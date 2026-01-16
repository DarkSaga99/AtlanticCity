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
            var senderEmail = _config["Email:SenderEmail"];
            var host = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var password = _config["Email:SenderPassword"];

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
            {
                throw new Exception("Incomplete email configuration (SenderEmail/Password missing)");
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Atlantic City System", senderEmail));
            email.To.Add(MailboxAddress.Parse(destinatario));
            email.Subject = asunto;
            email.Body = new TextPart("plain") { Text = cuerpo };

            using var client = new SmtpClient();
            try
            {
                _log.LogInformation($"[SMTP] Sending email to {destinatario}...");

                await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, password);
                await client.SendAsync(email);
                
                _log.LogInformation($"[SMTP] Email sent successfully to {destinatario}");
            }
            catch (Exception ex)
            {
                _log.LogError($"[SMTP Error] Delivery failed: {ex.Message}");
                
                // Fallback: log to console
                Console.WriteLine("\n--- EMAIL FALLBACK LOG ---");
                Console.WriteLine($"TO: {destinatario}");
                Console.WriteLine($"SUBJECT: {asunto}");
                Console.WriteLine($"BODY: {cuerpo}");
                Console.WriteLine("--------------------------\n");
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}

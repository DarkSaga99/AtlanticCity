using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Text;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AtlanticCity.Compartido.Mensajeria
{
    public class ServicioRabbitMQ : IServicioMensajeria
    {
        private readonly string _nombreServidor;
        private readonly ILogger<ServicioRabbitMQ> _log;
        private const int MaxReintentos = 10;
        private const int EsperaBaseMs = 2000; // 2 segundos base

        public ServicioRabbitMQ(ILogger<ServicioRabbitMQ> log, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _log = log;
            _nombreServidor = config["RabbitMQ:HostName"] ?? "localhost";
        }

        private IConnection CrearConexionConReintentos(ConnectionFactory fabrica)
        {
            Exception? ultimaExcepcion = null;
            
            for (int intento = 1; intento <= MaxReintentos; intento++)
            {
                try
                {
                    _log.LogInformation($"[RABBITMQ] Intento {intento}/{MaxReintentos} - Conectando a {_nombreServidor}...");
                    var conexion = fabrica.CreateConnection();
                    _log.LogInformation($"[RABBITMQ] ¡Conexión exitosa a {_nombreServidor}!");
                    return conexion;
                }
                catch (Exception ex)
                {
                    ultimaExcepcion = ex;
                    int tiempoEspera = EsperaBaseMs * intento; // Espera lineal: 2s, 4s, 6s, etc.
                    _log.LogWarning($"[RABBITMQ] Intento {intento} fallido: {ex.Message}. Reintentando en {tiempoEspera/1000} segundos...");
                    Thread.Sleep(tiempoEspera);
                }
            }
            
            throw new Exception($"No se pudo conectar a RabbitMQ después de {MaxReintentos} intentos", ultimaExcepcion);
        }

        public Task PublicarMensajeAsync<T>(string nombreCola, T mensaje)
        {
            var fabrica = new ConnectionFactory()
            {
                HostName = _nombreServidor,
                UserName = "guest",
                Password = "guest"
            };

            using var conexion = CrearConexionConReintentos(fabrica);
            using var canal = conexion.CreateModel();

            canal.QueueDeclare(queue: nombreCola, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var json = JsonConvert.SerializeObject(mensaje);
            var cuerpo = Encoding.UTF8.GetBytes(json);

            var propiedades = canal.CreateBasicProperties();
            propiedades.Persistent = true;

            canal.BasicPublish(exchange: "", routingKey: nombreCola, basicProperties: propiedades, body: cuerpo);
            _log.LogInformation($"[MENSAJERÍA] Mensaje enviado a la cola: {nombreCola}");

            return Task.CompletedTask;
        }

        public Task SuscribirseAsync<T>(string nombreCola, Func<T, Task> accionAlRecibir)
        {
            var fabrica = new ConnectionFactory()
            {
                HostName = _nombreServidor,
                UserName = "guest",
                Password = "guest",
                DispatchConsumersAsync = true
            };

            var conexion = CrearConexionConReintentos(fabrica);
            var canal = conexion.CreateModel();

            canal.QueueDeclare(queue: nombreCola, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumidor = new AsyncEventingBasicConsumer(canal);
            consumidor.Received += async (model, ea) =>
            {
                try
                {
                    var cuerpo = ea.Body.ToArray();
                    var textoJson = Encoding.UTF8.GetString(cuerpo);
                    Console.WriteLine($"[RABBITMQ DEBUG] Recibido en {nombreCola}: {textoJson}");

                    var objetoResultante = JsonConvert.DeserializeObject<T>(textoJson);

                    if (objetoResultante != null)
                    {
                        await accionAlRecibir(objetoResultante);
                    }

                    canal.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RABBITMQ ERROR] {ex.Message}");
                    _log.LogError($"[RABBITMQ ERROR] {ex.Message}");
                    canal.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            canal.BasicConsume(queue: nombreCola, autoAck: false, consumer: consumidor);
            _log.LogInformation($"[RABBITMQ] Escuchando en cola: {nombreCola}");

            return Task.CompletedTask;
        }
    }
}

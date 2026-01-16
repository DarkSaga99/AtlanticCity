using Polly; // Librería para manejar reintentos y fallos
using Polly.Retry; // Para políticas de reintento
using Polly.CircuitBreaker; // Para el patrón de "Corta-fuegos"
using Microsoft.Extensions.Logging; // Para registrar lo que sucede en consola
using System;

namespace AtlanticCity.Compartido.Resiliencia
{
    public static class PoliticasResiliencia
    {
        // Define cómo reaccionar si falla una conexión (Base de Datos o RabbitMQ)
        public static AsyncRetryPolicy ObtenerPoliticaReintento(ILogger log)
        {
            return Policy
                .Handle<Exception>() // Captura cualquier tipo de error
                .WaitAndRetryAsync(3, intento => // Reintenta hasta 3 veces
                    TimeSpan.FromSeconds(Math.Pow(2, intento)), // Espera exponencial: 2, 4, 8 segundos
                    (excepcion, tiempo, contador, contexto) => // Qué hacer en cada fallo
                    {
                        log.LogWarning($"Intento {contador} fallido. Esperando {tiempo}. Error: {excepcion.Message}");
                    });
        }

        // Define cuándo dejar de intentar si un servicio está permanentemente caído
        public static AsyncCircuitBreakerPolicy ObtenerPoliticaCortaFuegos(ILogger log)
        {
            return Policy
                .Handle<Exception>() // Captura fallos
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30), // Tras 2 fallos, "abre el circuito" por 30 segundos
                    onBreak: (ex, tiempo) => // Acción cuando el circuito se abre
                    {
                        log.LogError($"CIRCUITO ABIERTO: El servicio no responde. Pausa de {tiempo}s.");
                    },
                    onReset: () => log.LogInformation("CIRCUITO CERRADO: El servicio se ha recuperado correctamente."),
                    onHalfOpen: () => log.LogInformation("CIRCUITO SEMI-ABIERTO: Probando si el servicio ya está listo..."));
        }
    }
}

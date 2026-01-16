using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using Microsoft.Extensions.Logging;
using System;

namespace AtlanticCity.Compartido.Resiliencia
{
    public static class PoliticasResiliencia
    {
        // Resiliency policies for DB and Messaging
        public static AsyncRetryPolicy ObtenerPoliticaReintento(ILogger log)
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, intento => 
                    TimeSpan.FromSeconds(Math.Pow(2, intento)),
                    (excepcion, tiempo, contador, contexto) => 
                    {
                        log.LogWarning($"Retry attempt {contador} failed. Waiting {tiempo}. Error: {excepcion.Message}");
                    });
        }

        public static AsyncCircuitBreakerPolicy ObtenerPoliticaCortaFuegos(ILogger log)
        {
            return Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30),
                    onBreak: (ex, tiempo) => 
                    {
                        log.LogError($"CIRCUIT OPEN: Paused for {tiempo}s due to consecutive failures.");
                    },
                    onReset: () => log.LogInformation("CIRCUIT CLOSED: Service recovered."),
                    onHalfOpen: () => log.LogInformation("CIRCUIT HALF-OPEN: Testing recovery..."));
        }
    }
}

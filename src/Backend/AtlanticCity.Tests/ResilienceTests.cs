using Xunit;
using AtlanticCity.Compartido.Resiliencia;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.CircuitBreaker;
using System;
using System.Threading.Tasks;

namespace AtlanticCity.Tests
{
    public class ResilienceTests
    {
        [Fact]
        public async Task CircuitBreaker_ShouldOpen_AfterTwoFailures()
        {
            // Arrange: Configuramos el logger nulo y obtenemos la política en español
            var logger = NullLogger.Instance;
            var policy = PoliticasResiliencia.ObtenerPoliticaCortaFuegos(logger);

            // Act & Assert
            // Primer fallo
            await Assert.ThrowsAsync<Exception>(() => policy.ExecuteAsync(() => throw new Exception("Error 1")));
            
            // Segundo fallo -> Debería abrir el circuito
            await Assert.ThrowsAsync<Exception>(() => policy.ExecuteAsync(() => throw new Exception("Error 2")));

            // Tercer intento -> Debería lanzar BrokenCircuitException porque el circuito está ABIERTO
            await Assert.ThrowsAnyAsync<BrokenCircuitException>(() => policy.ExecuteAsync(() => Task.CompletedTask));
        }
        [Fact]
        public async Task RetryPolicy_ShouldRetry_SpecifiedTimes()
        {
            // Arrange
            var logger = NullLogger.Instance;
            var policy = PoliticasResiliencia.ObtenerPoliticaReintento(logger);
            int intentosReales = 0;

            // Act
            try
            {
                await policy.ExecuteAsync(() =>
                {
                    intentosReales++;
                    throw new Exception("Fallo temporal");
                });
            }
            catch { /* Ignoramos el error final para validar los intentos */ }

            // Assert: 1 intento original + 3 reintentos = 4 ejecuciones en total
            Assert.Equal(4, intentosReales);
        }
    }
}

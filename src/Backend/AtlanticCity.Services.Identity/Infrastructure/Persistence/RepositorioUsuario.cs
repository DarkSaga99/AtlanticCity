using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AtlanticCity.Servicios.Identidad.Core.Entidades;
using AtlanticCity.Servicios.Identidad.Core.Interfaces;

namespace AtlanticCity.Servicios.Identidad.Infraestructura.Persistencia
{
    public class RepositorioUsuario : IUsuarioRepositorio
    {
        private readonly ContextoBaseDatos _context;
        private readonly ILogger<RepositorioUsuario> _log;
        private readonly Polly.Retry.AsyncRetryPolicy _retryPolicy;
        private readonly Polly.CircuitBreaker.AsyncCircuitBreakerPolicy _circuitBreaker;

        public RepositorioUsuario(ContextoBaseDatos context, ILogger<RepositorioUsuario> log)
        {
            _context = context;
            _log = log;
            
            _retryPolicy = AtlanticCity.Compartido.Resiliencia.PoliticasResiliencia.ObtenerPoliticaReintento(log);
            _circuitBreaker = AtlanticCity.Compartido.Resiliencia.PoliticasResiliencia.ObtenerPoliticaCortaFuegos(log);
        }

        public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
        {
            try 
            {
                return await _retryPolicy.ExecuteAsync(() => 
                    _circuitBreaker.ExecuteAsync(() => 
                        _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario)
                    )
                );
            }
            catch (Polly.CircuitBreaker.BrokenCircuitException)
            {
                _log.LogCritical("Circuit Breaker is open: database connection temporarily disabled.");
                throw;
            }
        }

        public async Task<Usuario?> ObtenerPorEmailAsync(string email)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task RegistrarAsync(Usuario usuario)
        {
            try 
            {
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _log.LogError($"EF Registry Error: {ex.Message}");
                throw;
            }
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AtlanticCity.Servicios.Identidad.Aplicacion.Comandos;
using AtlanticCity.Servicios.Identidad.Core.Entidades;
using AtlanticCity.Servicios.Identidad.Core.Interfaces;

namespace AtlanticCity.Servicios.Identidad.Aplicacion.Handlers
{
    // HANDLER: Orquestador del registro de usuarios.
    public class RegistrarUsuarioHandler : IRequestHandler<RegistrarUsuarioCommand, RegistroResultadoDto>
    {
        private readonly IUsuarioRepositorio _repositorio;

        public RegistrarUsuarioHandler(IUsuarioRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<RegistroResultadoDto> Handle(RegistrarUsuarioCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar si el usuario ya existe
            var existeUsuario = await _repositorio.ObtenerPorNombreUsuarioAsync(request.Username);
            if (existeUsuario != null)
            {
                return new RegistroResultadoDto(false, "El nombre de usuario ya está registrado");
            }

            // 2. Validar si el email ya existe
            var existeEmail = await _repositorio.ObtenerPorEmailAsync(request.Email);
            if (existeEmail != null)
            {
                return new RegistroResultadoDto(false, "El correo electrónico ya está registrado");
            }

            // 3. Crear entidad con email
            var usuario = new Usuario
            {
                NombreUsuario = request.Username,
                ClaveHash = request.Password, // Recordar: Hashear en producción
                Email = request.Email,
                Role = "Admin" // <--- ASIGNAMOS ADMIN POR DEFECTO PARA ESTA PRUEBA
            };

            // 4. Guardar
            await _repositorio.RegistrarAsync(usuario);
            return new RegistroResultadoDto(true, null);
        }
    }
}

using MediatR;
using AtlanticCity.Servicios.Identidad.Aplicacion.Dtos;

namespace AtlanticCity.Servicios.Identidad.Aplicacion.Comandos
{
    // COMMAND: Intención de iniciar sesión. CQRS Pattern.
    public record LoginCommand(string Username, string Password) : IRequest<TokenDto?>;

    // COMMAND: Intención de registrar nuevo usuario.
    public record RegistrarUsuarioCommand(string Username, string Email, string Password) : IRequest<RegistroResultadoDto>;

    // COMMAND: Intención de refrescar sesión.
    public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<TokenDto?>;

    // DTO: Resultado del registro con información detallada
    public record RegistroResultadoDto(bool Exito, string? Error);
}

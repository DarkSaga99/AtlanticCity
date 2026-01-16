namespace AtlanticCity.Servicios.Identidad.Aplicacion.Dtos
{
    // DTO: Estructuras de datos para comunicación externa.
    public record LoginDto(string Username, string Password);
    
    // DTO: Para registro incluye email
    public record RegisterDto(string Username, string Email, string Password);
    
    public record TokenDto(string AccessToken, string RefreshToken, string Email);
    
    public record RefreshTokenRequestDto(string AccessToken, string RefreshToken);
}


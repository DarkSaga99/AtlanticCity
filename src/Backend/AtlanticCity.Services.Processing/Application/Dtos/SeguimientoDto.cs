namespace AtlanticCity.Services.Processing.Application.Dtos
{
    // DTO (Data Transfer Object): Objeto simple para transportar datos entre capas
    // Ayuda a desacoplar el modelo de base de datos de la respuesta de la API
    public class SeguimientoDto
    {
        public string CodigoProducto { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}

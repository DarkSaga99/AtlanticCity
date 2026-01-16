namespace AtlanticCity.Workers.CargaMasiva.Nucleo.Entidades
{
    // Clase que representa un producto extraído del archivo Excel
    public class Producto
    {
        public int Id { get; set; } // Autoincremental en DB
        public string Codigo { get; set; } = string.Empty; // Código del producto
        public string Nombre { get; set; } = string.Empty; // Descripción
        public string Periodo { get; set; } = string.Empty; // Mes/Año ejemplo 2024-01
        public string Estado { get; set; } = "Cargado"; // Estado inicial del flujo

        // Regla del PDF: Ignorar si faltan datos críticos
        public bool EsValido() => !string.IsNullOrEmpty(Codigo) && !string.IsNullOrEmpty(Periodo);

        // Regla del PDF: Limpieza de datos (Celdas vacías toman valores por defecto)
        public void LimpiarDatos()
        {
            if (string.IsNullOrEmpty(Nombre)) Nombre = "Producto Sin Descripción";
        }
    }
}

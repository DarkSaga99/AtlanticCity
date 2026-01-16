using System;

namespace AtlanticCity.Compartido.Mensajeria.Dtos
{
    // Clase que viaja por la cola de RabbitMQ
    public class MensajeProcesamientoArchivo
    {
        public Guid IdMensaje { get; set; } = Guid.NewGuid(); // ID único de la petición
        public string NombreArchivo { get; set; } = string.Empty; // Ejemplo: datos_enero.xlsx
        public string UrlArchivo { get; set; } = string.Empty; // Dirección en SeaweedFS
        public string CorreoUsuario { get; set; } = string.Empty; // A quién notificar al final
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow; // Cuándo se pidió la carga
        public bool EsRechazado { get; set; } = false; // Indica si el proceso falló/fue rechazado
        public string MotivoRechazo { get; set; } = string.Empty; // Detalle técnico del porqué
        public int TotalRegistros { get; set; } // Total de filas en el archivo
        public int RegistrosProcesados { get; set; } // Cuántas filas se han procesado hasta ahora
        public bool EsCompleto { get; set; } = false; // Indica si es el mensaje final o solo progreso
    }
}

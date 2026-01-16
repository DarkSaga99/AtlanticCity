namespace AtlanticCity.Workers.Notifications.Core.Entities
{
    // Define los estados finales del procesamiento según el PDF
    public static class EstadosProceso
    {
        public const string Cargado = "Cargado";
        public const string Finalizado = "Finalizado";
        public const string Notificado = "Notificado";
        public const string Existente = "Existente"; // Para registros duplicados
    }
}

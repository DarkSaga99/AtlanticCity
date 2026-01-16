namespace AtlanticCity.Services.Processing.Application.Dtos
{
    public class AlertaDto
    {
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}

namespace AtlanticCity.Services.Processing.Application.Dtos
{
    public class HistorialDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public int TotalRecords { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace AtlanticCity.Services.Processing.Core.Entities
{
    public class LoteProceso
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Period { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public int TotalRecords { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }

        public virtual ICollection<ProductoDetalle> Productos { get; set; } = new List<ProductoDetalle>();
    }

    public class ProductoDetalle
    {
        public int Id { get; set; }
        public Guid BatchId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public virtual LoteProceso Lote { get; set; } = null!;
    }

    public class LogAuditoria
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

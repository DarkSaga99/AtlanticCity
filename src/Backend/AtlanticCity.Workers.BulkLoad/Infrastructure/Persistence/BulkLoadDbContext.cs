using Microsoft.EntityFrameworkCore;
using AtlanticCity.Workers.BulkLoad.Core.Entities;

namespace AtlanticCity.Workers.BulkLoad.Infraestructura.Persistencia
{
    public class BulkLoadDbContext : DbContext
    {
        public BulkLoadDbContext(DbContextOptions<BulkLoadDbContext> options) : base(options) { }

        public DbSet<LoteProceso> Lotes { get; set; } = null!;
        public DbSet<LogAuditoria> Logs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LoteProceso>(entity =>
            {
                entity.ToTable("ProcessBatches");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<LogAuditoria>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.HasKey(e => e.Id);
            });
        }
    }
}

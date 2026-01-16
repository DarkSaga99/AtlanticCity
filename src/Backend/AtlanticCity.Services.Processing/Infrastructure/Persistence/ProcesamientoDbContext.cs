using Microsoft.EntityFrameworkCore;
using AtlanticCity.Services.Processing.Core.Entities;

namespace AtlanticCity.Services.Processing.Infrastructure.Persistence
{
    public class ProcesamientoDbContext : DbContext
    {
        public ProcesamientoDbContext(DbContextOptions<ProcesamientoDbContext> options) : base(options) { }

        public DbSet<LoteProceso> Lotes { get; set; } = null!;
        public DbSet<ProductoDetalle> Productos { get; set; } = null!;
        public DbSet<LogAuditoria> Logs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoteProceso>(entity =>
            {
                entity.ToTable("ProcessBatches");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StartTime).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<ProductoDetalle>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.Id);
                entity.HasOne(d => d.Lote)
                    .WithMany(p => p.Productos)
                    .HasForeignKey(d => d.BatchId);
            });

            modelBuilder.Entity<LogAuditoria>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETDATE()");
            });
        }
    }
}

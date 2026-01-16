using Microsoft.EntityFrameworkCore;
using AtlanticCity.Workers.Notifications.Core.Entities;

namespace AtlanticCity.Workers.Notifications.Infrastructure.Persistence
{
    public class NotificacionesDbContext : DbContext
    {
        public NotificacionesDbContext(DbContextOptions<NotificacionesDbContext> options) : base(options) { }

        public DbSet<LoteProceso> Lotes { get; set; } = null!;
        public DbSet<ProductoDetalle> Productos { get; set; } = null!;
        public DbSet<LogAuditoria> Logs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LoteProceso>(entity =>
            {
                entity.ToTable("ProcessBatches");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<ProductoDetalle>(entity =>
            {
                entity.ToTable("Products");
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

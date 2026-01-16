using Microsoft.EntityFrameworkCore;
using AtlanticCity.Servicios.Identidad.Core.Entidades;

namespace AtlanticCity.Servicios.Identidad.Infraestructura.Persistencia
{
    public class ContextoBaseDatos : DbContext
    {
        public ContextoBaseDatos(DbContextOptions<ContextoBaseDatos> opciones) : base(opciones) { }

        public DbSet<Usuario> Usuarios { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder constructorModelos)
        {
            base.OnModelCreating(constructorModelos);

            // Fluent API Mapping
            constructorModelos.Entity<Usuario>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.NombreUsuario).HasColumnName("Username").IsRequired();
                entity.Property(u => u.ClaveHash).HasColumnName("PasswordHash").IsRequired();
                entity.Property(u => u.Email).HasColumnName("Email");
                entity.Property(u => u.Role).HasColumnName("Role");
                entity.Property(u => u.TokenRefresco).HasColumnName("RefreshToken");
                entity.Property(u => u.FechaExpiracionRefresco).HasColumnName("RefreshTokenExpiry");

                entity.HasIndex(u => u.NombreUsuario).IsUnique();
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using MitHra.API.Models;

namespace MitHra.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Agregamos el " = null!; " al final para silenciar el error CS8618
        public DbSet<Producto> Productos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Producto>().HasKey(p => p.Id);
            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioNeto)
                .HasColumnType("decimal(18,2)");
        }
    }
}
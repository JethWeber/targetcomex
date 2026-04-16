using Microsoft.EntityFrameworkCore;
using Target.Api.Models;

namespace Target.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nome das tabelas exatamente como no SQL Server
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Veiculo>().ToTable("Veiculos");
            modelBuilder.Entity<Avaliacao>().ToTable("Avaliacoes");

            // Email único
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
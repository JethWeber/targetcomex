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
        public DbSet<Endereco> Enderecos { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }
        
        // CORREÇÃO 1: O DbSet fica aqui em cima, junto com as outras propriedades da classe!
        public DbSet<VeiculoImagem> VeiculoImagens { get; set; }
        
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<HistoricoNavegacao> HistoricoNavegacao { get; set; }
        public DbSet<HistoricoCompra> HistoricoCompras { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nome das tabelas exatamente como no SQL Server
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Endereco>().ToTable("Enderecos");
            modelBuilder.Entity<Veiculo>().ToTable("Veiculos");
            
            // CORREÇÃO 2: Aqui dentro do método usamos o modelBuilder para mapear a tabela!
            modelBuilder.Entity<VeiculoImagem>().ToTable("VeiculoImagens");
            
            modelBuilder.Entity<Avaliacao>().ToTable("Avaliacoes");
            modelBuilder.Entity<HistoricoNavegacao>().ToTable("HistoricoNavegacao");
            modelBuilder.Entity<HistoricoCompra>().ToTable("HistoricoCompras");
            modelBuilder.Entity<Reserva>().ToTable("Reservas");

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Endereco>()
                .HasOne(e => e.Usuario)
                .WithOne(u => u.Endereco)
                .HasForeignKey<Endereco>(e => e.UsuarioId)
                .IsRequired(false);
        }
    }
}
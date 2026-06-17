using System;

namespace Target.Api.Models
{
    public class HistoricoCompra
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int VeiculoId { get; set; }
        public DateTime DataCompra { get; set; } = DateTime.UtcNow;
        public decimal ValorPago { get; set; }

        public Veiculo? Veiculo { get; set; }
    }
}

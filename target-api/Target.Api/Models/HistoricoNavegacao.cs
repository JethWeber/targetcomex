using System;

namespace Target.Api.Models
{
    public class HistoricoNavegacao
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int VeiculoId { get; set; }
        public DateTime DataVisualizacao { get; set; } = DateTime.UtcNow;
    }
}

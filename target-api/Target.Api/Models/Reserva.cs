using System;

namespace Target.Api.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int VeiculoId { get; set; }
        public string? TipoPedido { get; set; }
        public string? Showroom { get; set; }
        public string Estado { get; set; } = "Pendente";
        public DateTime DataReserva { get; set; } = DateTime.UtcNow;
    }
}

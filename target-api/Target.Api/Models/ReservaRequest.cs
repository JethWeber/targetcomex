namespace Target.Api.Models
{
    public class ReservaRequest
    {
        public int UsuarioId { get; set; }
        public int VeiculoId { get; set; }
        public string? TipoPedido { get; set; }
        public string? Showroom { get; set; }
    }
}

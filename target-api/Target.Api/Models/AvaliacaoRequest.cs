namespace Target.Api.Models
{
    public class AvaliacaoRequest
    {
        public int VeiculoId { get; set; }
        public int UsuarioId { get; set; }
        public int Nota { get; set; }
        public string? Comentario { get; set; }
    }
}

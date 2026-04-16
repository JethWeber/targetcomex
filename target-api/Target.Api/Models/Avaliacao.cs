namespace Target.Api.Models
{
    public class Avaliacao
    {
        public int Id { get; set; }

        public int VeiculoId { get; set; }

        public int UsuarioId { get; set; }

        public int Nota { get; set; }

        public string? Comentario { get; set; }

        public DateTime DataAvaliacao { get; set; }
    }
}
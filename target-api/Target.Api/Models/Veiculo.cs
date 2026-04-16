using System.ComponentModel.DataAnnotations;

namespace Target.Api.Models
{
    public class Veiculo
    {
        public int Id { get; set; }

        [Required]
        public string Marca { get; set; }

        [Required]
        public string Modelo { get; set; }

        public int Ano { get; set; }

        public string? Descricao { get; set; }

        public string? ImagemUrl { get; set; }

        public string? Cor { get; set; }

        public string? Estilo { get; set; }

        public string? Combustivel { get; set; }

        public int? Quilometragem { get; set; }

        public decimal Preco { get; set; }

        public bool Disponivel { get; set; }
    }
}
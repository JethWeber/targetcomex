using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Target.Api.Models
{
    public class VeiculoImagem
    {
        public int Id { get; set; }

        [Required]
        public int VeiculoId { get; set; }

        [Required]
        public string ImagemUrl { get; set; }

        // Relacionamento inversa (Muitos para 1)
        // O JsonIgnore evita loops infinitos na hora de serializar para a API
        [JsonIgnore]
        [ForeignKey("VeiculoId")]
        public Veiculo? Veiculo { get; set; }
    }
}
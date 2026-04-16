using System.ComponentModel.DataAnnotations;

namespace Target.Api.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string SenhaHash { get; set; }

        [Required]
        public string Role { get; set; }

        public DateTime? DataNascimento { get; set; }

        public string? Genero { get; set; }

        public string? EstadoCivil { get; set; }

        public int? NumeroFilhos { get; set; }

        public string? Profissao { get; set; }

        public string? FaixaRendaMensal { get; set; }

        public string? InteressesPrincipais { get; set; }

        public string? TipoDeUsoPretendido { get; set; }

        public DateTime DataCadastro { get; set; }
    }
}
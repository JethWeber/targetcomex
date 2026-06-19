namespace Target.Api.Models
{
    public class UpdateUsuarioRequest
    {
        public string? Nome { get; set; }
        public string? Telefone { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Genero { get; set; }
        public string? EstadoCivil { get; set; }
        public int? NumeroFilhos { get; set; }
        public string? Profissao { get; set; }
        public string? FaixaRendaMensal { get; set; }
        public string? InteressesPrincipais { get; set; }
        public string? TipoDeUsoPretendido { get; set; }
        public string? Role { get; set; }
    }
}

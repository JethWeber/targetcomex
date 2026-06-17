using System;
using System.Collections.Generic;

namespace Target.Api.Models
{
    public class RegisterRequest
    {
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public string? Telefone { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Genero { get; set; }
        public string? EstadoCivil { get; set; }
        public int? NumeroFilhos { get; set; }
        public string? Profissao { get; set; }
        public string? FaixaRendaMensal { get; set; }
        public List<string>? TiposUso { get; set; }
        public List<string>? InteressesPrincipais { get; set; }
        public string? Provincia { get; set; }
        public string? Municipio { get; set; }
        public string? Bairro { get; set; }
        public string? RuaComplemento { get; set; }
    }
}

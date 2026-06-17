using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Target.Api.Models
{
    public class Endereco
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string? Provincia { get; set; }
        public string? Municipio { get; set; }
        public string? Distrito { get; set; }
        public string? Bairro { get; set; }
        public string? RuaComplemento { get; set; }
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        public Usuario? Usuario { get; set; }
    }
}

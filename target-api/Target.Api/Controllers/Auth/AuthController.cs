using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Target.Api.Data;
using Target.Api.Models;
using BCrypt.Net;

namespace Target.Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // ================= REGISTER =================
        [HttpPost("register")]
        [HttpPost("registrar")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
                return BadRequest("Email e senha são obrigatórios.");

            if (_context.Usuarios.Any(u => u.Email == request.Email))
                return BadRequest("Email já existe");

            var generoNormalizado = NormalizeGenero(request.Genero);
            if (generoNormalizado == null && !string.IsNullOrWhiteSpace(request.Genero))
                return BadRequest("Gênero inválido. Utilize 'M' ou 'F'.");

            var user = new Usuario
            {
                Nome = request.Nome?.Trim() ?? string.Empty,
                Email = request.Email.Trim(),
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
                Telefone = request.Telefone?.Trim(),
                Role = "Cliente",
                DataCadastro = DateTime.UtcNow,
                DataNascimento = request.DataNascimento,
                Genero = generoNormalizado,
                EstadoCivil = request.EstadoCivil,
                NumeroFilhos = request.NumeroFilhos,
                Profissao = request.Profissao,
                FaixaRendaMensal = request.FaixaRendaMensal,
                TipoDeUsoPretendido = request.TiposUso != null ? string.Join(",", request.TiposUso) : null,
                InteressesPrincipais = request.InteressesPrincipais != null ? string.Join(",", request.InteressesPrincipais) : null,
            };

            _context.Usuarios.Add(user);
            _context.SaveChanges();

            if (!string.IsNullOrWhiteSpace(request.Provincia) ||
                !string.IsNullOrWhiteSpace(request.Municipio) ||
                !string.IsNullOrWhiteSpace(request.Bairro) ||
                !string.IsNullOrWhiteSpace(request.RuaComplemento))
            {
                var endereco = new Endereco
                {
                    UsuarioId = user.Id,
                    Provincia = request.Provincia,
                    Municipio = request.Municipio,
                    Bairro = request.Bairro,
                    RuaComplemento = request.RuaComplemento
                };

                _context.Enderecos.Add(endereco);
                _context.SaveChanges();
            }

            return Ok(new { message = "Usuário criado com sucesso" });
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.Email == login.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Senha, user.SenhaHash))
                return Unauthorized("Credenciais inválidas");

            var token = GenerateJwtToken(user);

            return Ok(new { token });
        }

        // ================= JWT =================
        private string GenerateJwtToken(Usuario user)
        {
            // O OIDC/IdentityModel exige chave HMAC >= 256 bits para HMAC-SHA256.
            // Mantemos o mesmo segredo do Program.cs.
            var key = Encoding.ASCII.GetBytes("TARGETCOMEX_SUPER_SECRET_KEY_1234567890");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(5),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private static string? NormalizeGenero(string? genero)
        {
            if (string.IsNullOrWhiteSpace(genero))
                return null;

            genero = genero.Trim();
            return genero.ToUpperInvariant() switch
            {
                "M" or "MASCULINO" => "M",
                "F" or "FEMININO"  => "F",
                _ => null
            };
        }
    }
}
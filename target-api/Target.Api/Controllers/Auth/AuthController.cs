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
        public IActionResult Register([FromBody] Usuario user)
        {
            if (_context.Usuarios.Any(u => u.Email == user.Email))
                return BadRequest("Email já existe");

            user.SenhaHash = BCrypt.Net.BCrypt.HashPassword(user.SenhaHash);
            user.DataCadastro = DateTime.Now;

            _context.Usuarios.Add(user);
            _context.SaveChanges();

            return Ok("Usuário criado com sucesso");
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        public IActionResult Login([FromBody] Usuario login)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.Email == login.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(login.SenhaHash, user.SenhaHash))
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
    }
}
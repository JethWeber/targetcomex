using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Target.Api.Data;
using Target.Api.Models;

namespace Target.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔐 protege tudo
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _context.Usuarios.ToList();
            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _context.Usuarios.Find(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateUsuarioRequest updated)
        {
            var user = _context.Usuarios.Find(id);

            if (user == null)
                return NotFound();

            var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("nameid")
                ?? User.FindFirstValue("sub");

            if (!int.TryParse(currentUserIdClaim, out var currentUserId))
                return Forbid();

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role)
                ?? User.FindFirstValue("role");

            var isAdmin = string.Equals(currentUserRole, "Admin", StringComparison.OrdinalIgnoreCase);
            var isSelf = currentUserId == id;

            if (!isSelf && !isAdmin)
                return Forbid();

            if (!isSelf && isAdmin)
            {
                if (string.IsNullOrWhiteSpace(updated.Role))
                    return BadRequest("O tipo de perfil é obrigatório para actualizar outro utilizador.");

                user.Role = updated.Role.Trim();
                _context.SaveChanges();
                return Ok(user);
            }

            if (!string.IsNullOrWhiteSpace(updated.Nome))
                user.Nome = updated.Nome.Trim();

            user.Telefone = updated.Telefone;
            user.DataNascimento = updated.DataNascimento;
            user.Genero = updated.Genero;
            user.EstadoCivil = updated.EstadoCivil;
            user.NumeroFilhos = updated.NumeroFilhos;
            user.Profissao = updated.Profissao;
            user.FaixaRendaMensal = updated.FaixaRendaMensal;
            user.InteressesPrincipais = updated.InteressesPrincipais;
            user.TipoDeUsoPretendido = updated.TipoDeUsoPretendido;

            if (isAdmin && !string.IsNullOrWhiteSpace(updated.Role))
                user.Role = updated.Role.Trim();

            _context.SaveChanges();
            return Ok(user);
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = _context.Usuarios.Find(id);

            if (user == null)
                return NotFound();

            _context.Usuarios.Remove(user);
            _context.SaveChanges();

            return Ok("Usuário removido com sucesso");
        }
    }
}
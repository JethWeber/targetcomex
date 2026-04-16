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
        public IActionResult Update(int id, Usuario updated)
        {
            var user = _context.Usuarios.Find(id);

            if (user == null)
                return NotFound();

            user.Nome = updated.Nome;
            user.Email = updated.Email;
            user.Role = updated.Role;
            user.DataNascimento = updated.DataNascimento;
            user.Genero = updated.Genero;
            user.EstadoCivil = updated.EstadoCivil;
            user.NumeroFilhos = updated.NumeroFilhos;
            user.Profissao = updated.Profissao;
            user.FaixaRendaMensal = updated.FaixaRendaMensal;
            user.InteressesPrincipais = updated.InteressesPrincipais;
            user.TipoDeUsoPretendido = updated.TipoDeUsoPretendido;

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
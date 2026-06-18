using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Target.Api.Data;
using Target.Api.Models;

namespace Target.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Veiculos")]
public class VeiculosController : ControllerBase
{
    private readonly AppDbContext _context;

    public VeiculosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/veiculos
    // Público — utilizado na página /produtos antes do login
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Lista todos os veículos")]
    public IActionResult GetAll()
    {
        // O .Include(v => v.Galeria) garante que as fotos adicionais sejam carregadas junto
        var list = _context.Veiculos
                        .Include(v => v.Galeria)
                        .AsNoTracking()
                        .OrderBy(v => v.Id)
                        .ToList();
        return Ok(list);
    }

    // GET: api/veiculos/{id}
    // Público — utilizado na página de detalhe antes do login
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Obtém um veículo por id")]
    public IActionResult GetById(int id)
    {
        var v = _context.Veiculos
                        .Include(v => v.Galeria)
                        .AsNoTracking()
                        .FirstOrDefault(x => x.Id == id);
        if (v == null)
            return NotFound();
        return Ok(v);
    }

    // POST: api/veiculos — requer autenticação (herdado da classe)
    [HttpPost]
    [SwaggerOperation(Summary = "Cria um veículo")]
    public IActionResult Create([FromBody] Veiculo body)
    {
        if (body == null) return BadRequest();

        // 1. Salvamos o veículo principal primeiro
        _context.Veiculos.Add(body);
        _context.SaveChanges(); // Aqui o SQL Server gera o body.Id automaticamente

        // 2. Se o objeto enviado já contiver itens mapeados na lista Galeria,
        // associamos o Id gerado e salvamos na tabela VeiculoImagens
        if (body.Galeria != null && body.Galeria.Any())
        {
            foreach (var img in body.Galeria)
            {
                img.VeiculoId = body.Id; // Vincula a Chave Estrangeira (FK)
                _context.VeiculoImagens.Add(img);
            }
            _context.SaveChanges();
        }

        return CreatedAtAction(nameof(GetById), new { id = body.Id }, body);
    }

    // PUT: api/veiculos/{id} — requer autenticação
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Atualiza um veículo")]
    public IActionResult Update(int id, [FromBody] Veiculo updated)
    {
        var v = _context.Veiculos.Find(id);
        if (v == null) return NotFound();

        v.Marca         = updated.Marca;
        v.Modelo        = updated.Modelo;
        v.Ano           = updated.Ano;
        v.Descricao     = updated.Descricao;
        v.ImagemUrl     = updated.ImagemUrl;
        v.Cor           = updated.Cor;
        v.Estilo        = updated.Estilo;
        v.Combustivel   = updated.Combustivel;
        v.Quilometragem = updated.Quilometragem;
        v.Preco         = updated.Preco;
        v.Disponivel    = updated.Disponivel;

        _context.SaveChanges();
        return Ok(v);
    }

    // DELETE: api/veiculos/{id} — requer autenticação
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Remove um veículo")]
    public IActionResult Delete(int id)
    {
        var v = _context.Veiculos.Find(id);
        if (v == null) return NotFound();

        try
        {
            _context.Veiculos.Remove(v);
            _context.SaveChanges();
        }
        catch (DbUpdateException)
        {
            return Conflict("Não é possível remover o veículo: existem registos relacionados.");
        }

        return Ok("Veículo removido com sucesso");
    }
}

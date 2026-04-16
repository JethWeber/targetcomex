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
    [HttpGet]
    [SwaggerOperation(Summary = "Lista todos os veículos")]
    public IActionResult GetAll()
    {
        var list = _context.Veiculos.AsNoTracking().OrderBy(v => v.Id).ToList();
        return Ok(list);
    }

    // GET: api/veiculos/{id}
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Obtém um veículo por id")]
    public IActionResult GetById(int id)
    {
        var v = _context.Veiculos.AsNoTracking().FirstOrDefault(x => x.Id == id);
        if (v == null)
            return NotFound();
        return Ok(v);
    }

    // POST: api/veiculos
    [HttpPost]
    [SwaggerOperation(Summary = "Cria um veículo")]
    public IActionResult Create([FromBody] Veiculo body)
    {
        body.Id = 0;
        _context.Veiculos.Add(body);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = body.Id }, body);
    }

    // PUT: api/veiculos/{id}
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Atualiza um veículo")]
    public IActionResult Update(int id, [FromBody] Veiculo updated)
    {
        var v = _context.Veiculos.Find(id);
        if (v == null)
            return NotFound();

        v.Marca = updated.Marca;
        v.Modelo = updated.Modelo;
        v.Ano = updated.Ano;
        v.Descricao = updated.Descricao;
        v.ImagemUrl = updated.ImagemUrl;
        v.Cor = updated.Cor;
        v.Estilo = updated.Estilo;
        v.Combustivel = updated.Combustivel;
        v.Quilometragem = updated.Quilometragem;
        v.Preco = updated.Preco;
        v.Disponivel = updated.Disponivel;

        _context.SaveChanges();
        return Ok(v);
    }

    // DELETE: api/veiculos/{id}
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Remove um veículo")]
    public IActionResult Delete(int id)
    {
        var v = _context.Veiculos.Find(id);
        if (v == null)
            return NotFound();

        try
        {
            _context.Veiculos.Remove(v);
            _context.SaveChanges();
        }
        catch (DbUpdateException)
        {
            return Conflict("Não é possível remover o veículo: existem registos relacionados (avaliações, histórico, features, etc.).");
        }

        return Ok("Veículo removido com sucesso");
    }
}

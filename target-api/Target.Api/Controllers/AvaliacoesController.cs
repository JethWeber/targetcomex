using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Target.Api.Data;
using Target.Api.Models;

namespace Target.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AvaliacoesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AvaliacoesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public IActionResult CriarAvaliacao([FromBody] AvaliacaoRequest request)
    {
        if (request == null || request.VeiculoId <= 0 || request.UsuarioId <= 0 || request.Nota < 1 || request.Nota > 5)
            return BadRequest("Dados de avaliação inválidos.");

        var avaliacao = new Avaliacao
        {
            VeiculoId = request.VeiculoId,
            UsuarioId = request.UsuarioId,
            Nota = request.Nota,
            Comentario = request.Comentario,
            DataAvaliacao = DateTime.UtcNow
        };

        _context.Avaliacoes.Add(avaliacao);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetAvaliacoesPorVeiculo), new { veiculoId = request.VeiculoId }, avaliacao);
    }

    [HttpGet("veiculo/{veiculoId:int}")]
    public IActionResult GetAvaliacoesPorVeiculo(int veiculoId)
    {
        var avaliacoes = _context.Avaliacoes
            .Where(a => a.VeiculoId == veiculoId)
            .OrderByDescending(a => a.DataAvaliacao)
            .ToList();

        return Ok(avaliacoes);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Target.Api.Data;
using Target.Api.Models;

namespace Target.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HistoricoNavegacaoController : ControllerBase
{
    private readonly AppDbContext _context;

    public HistoricoNavegacaoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public IActionResult Registrar([FromBody] HistoricoNavegacaoRequest request)
    {
        if (request == null || request.UsuarioId <= 0 || request.VeiculoId <= 0)
            return BadRequest("Dados inválidos para histórico de navegação.");

        var historico = new HistoricoNavegacao
        {
            UsuarioId = request.UsuarioId,
            VeiculoId = request.VeiculoId,
            DataVisualizacao = DateTime.UtcNow
        };

        _context.HistoricoNavegacao.Add(historico);
        _context.SaveChanges();

        return CreatedAtAction(nameof(Registrar), new { id = historico.Id }, historico);
    }
}

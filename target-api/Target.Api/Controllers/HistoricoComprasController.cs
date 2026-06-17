using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Target.Api.Data;

namespace Target.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HistoricoComprasController : ControllerBase
{
    private readonly AppDbContext _context;

    public HistoricoComprasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/HistoricoCompras/usuario/{usuarioId}
    // Requer autenticação
    [HttpGet("usuario/{usuarioId:int}")]
    public IActionResult GetPorUsuario(int usuarioId)
    {
        var historico = _context.HistoricoCompras
            .Where(h => h.UsuarioId == usuarioId)
            .OrderByDescending(h => h.DataCompra)
            .ToList();

        return Ok(historico);
    }

    // GET: api/HistoricoCompras/mais-comprados?top=6
    // Público — utilizado na página /produtos antes do login
    [HttpGet("mais-comprados")]
    [AllowAnonymous]
    public IActionResult GetMaisComprados([FromQuery] int top = 6)
    {
        // Agrupa por veículo, conta compras, junta com a tabela de veículos
        var resultado = _context.HistoricoCompras
            .GroupBy(h => h.VeiculoId)
            .Select(g => new { VeiculoId = g.Key, TotalCompras = g.Count() })
            .OrderByDescending(x => x.TotalCompras)
            .Take(top)
            .Join(
                _context.Veiculos.AsNoTracking(),
                hc => hc.VeiculoId,
                v  => v.Id,
                (hc, v) => new
                {
                    v.Id,
                    v.Marca,
                    v.Modelo,
                    v.Ano,
                    v.ImagemUrl,
                    v.Preco,
                    v.Cor,
                    v.Combustivel,
                    v.Disponivel,
                    hc.TotalCompras
                })
            .ToList();

        // Fallback: se não houver histórico, retorna os últimos veículos adicionados
        if (!resultado.Any())
        {
            var fallback = _context.Veiculos
                .AsNoTracking()
                .Where(v => v.Disponivel)
                .OrderByDescending(v => v.Id)
                .Take(top)
                .Select(v => new
                {
                    v.Id,
                    v.Marca,
                    v.Modelo,
                    v.Ano,
                    v.ImagemUrl,
                    v.Preco,
                    v.Cor,
                    v.Combustivel,
                    v.Disponivel,
                    TotalCompras = 0
                })
                .ToList();

            return Ok(fallback);
        }

        return Ok(resultado);
    }
}

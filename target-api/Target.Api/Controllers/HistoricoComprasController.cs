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
        // Veículos já vendidos (reserva Concluida) — excluídos de qualquer listagem
        var veiculosVendidos = _context.Reservas
            .Where(r => r.Estado == "Concluido")
            .Select(r => r.VeiculoId)
            .ToHashSet();

        // Agrupa compras por veículo e junta com os dados do veículo numa única query
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
            .AsEnumerable()                                          // materializa aqui
            .Where(x => !veiculosVendidos.Contains(x.Id))           // filtra vendidos
            .ToList();

        // Fallback: sem histórico → últimos veículos disponíveis e não vendidos
        if (!resultado.Any())
        {
            var fallback = _context.Veiculos
                .AsNoTracking()
                .Where(v => v.Disponivel && !veiculosVendidos.Contains(v.Id))
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

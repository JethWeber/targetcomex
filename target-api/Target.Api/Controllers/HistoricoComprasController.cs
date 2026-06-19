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
    // Público — utilizado na página /produtos antes do login.
    //
    // Regra: conta RESERVAS com Estado == "Concluido", agrupadas por
    // Marca+Modelo (não por VeiculoId individual), já que duas unidades
    // diferentes do mesmo Marca/Modelo devem somar no mesmo grupo.
    // O veículo "representante" do grupo (imagem/preço/id do link) é o de
    // maior Id (unidade mais recentemente cadastrada) dentro do grupo.
    [HttpGet("mais-comprados")]
    [AllowAnonymous]
    public IActionResult GetMaisComprados([FromQuery] int top = 6)
    {
        // Reservas concluídas, já trazendo os dados do veículo (join).
        var reservasConcluidas = _context.Reservas
            .AsNoTracking()
            .Where(r => r.Estado == "Concluido")
            .Join(
                _context.Veiculos.AsNoTracking(),
                r => r.VeiculoId,
                v => v.Id,
                (r, v) => new
                {
                    v.Id,
                    v.Marca,
                    v.Modelo,
                    v.Ano,
                    v.ImagemUrl,
                    v.Preco,
                    v.Cor,
                    v.Combustivel,
                    v.Disponivel
                })
            .ToList(); // materializa: o agrupamento por (Marca, Modelo) com
                       // seleção do maior Id é mais simples/segura em LINQ-to-Objects

        var resultado = reservasConcluidas
            .GroupBy(x => new { x.Marca, x.Modelo })
            .Select(g =>
            {
                // Representante do grupo = unidade de maior Id (mais recente)
                var representante = g.OrderByDescending(x => x.Id).First();
                return new
                {
                    representante.Id,
                    representante.Marca,
                    representante.Modelo,
                    representante.Ano,
                    representante.ImagemUrl,
                    representante.Preco,
                    representante.Cor,
                    representante.Combustivel,
                    representante.Disponivel,
                    TotalCompras = g.Count()
                };
            })
            .OrderByDescending(x => x.TotalCompras)
            .ThenByDescending(x => x.Id) // desempate estável e determinístico
            .Take(top)
            .ToList();

        // Fallback: sem nenhuma reserva concluída → últimos veículos disponíveis
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
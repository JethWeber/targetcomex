using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Target.Api.Data;
using Target.Api.Services;

namespace Target.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecomendacaoController : ControllerBase
{
    private readonly RecommendationService _aiService;
    private readonly AppDbContext          _context;

    public RecomendacaoController(RecommendationService aiService, AppDbContext context)
    {
        _aiService = aiService;
        _context   = context;
    }

    [HttpGet("usuario/{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        // Valida que o utilizador existe antes de chamar a IA
        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == id);
        if (!usuarioExiste)
            return NotFound($"Utilizador {id} não encontrado.");

        try
        {
            var raw = await _aiService.GetHybridRecommendations(id);
            if (raw == null)
                return Ok(Array.Empty<object>());

            // Converte o object? para uma lista de JsonElement
            var json   = JsonSerializer.Serialize(raw);
            var result = JsonSerializer.Deserialize<List<JsonElement>>(json);

            if (result == null || !result.Any())
                return Ok(Array.Empty<object>());

            // Exclui da resposta quaisquer veículos já vendidos (reserva Concluida)
            var veiculosVendidos = (await _context.Reservas
                .Where(r => r.Estado.ToLower() == "concluido")
                .Select(r => r.VeiculoId)
                .ToListAsync())
                .ToHashSet();

            var filtrado = result
                .Where(r =>
                {
                    // Suporta tanto "veiculoId" (camelCase) quanto "veiculo_id" (snake_case)
                    if (r.TryGetProperty("veiculoId", out var prop) ||
                        r.TryGetProperty("veiculo_id", out prop))
                        return !veiculosVendidos.Contains(prop.GetInt32());

                    return true; // se não encontrar o campo, mantém na lista
                })
                .ToList();

            return Ok(filtrado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao conectar com a IA: {ex.Message}");
        }
    }
}
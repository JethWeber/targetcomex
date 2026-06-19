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

    private record RecomendacaoResponse(int VeiculoId, double Score);

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
                return Ok(Array.Empty<RecomendacaoResponse>());

            var json   = JsonSerializer.Serialize(raw);
            var result = JsonSerializer.Deserialize<List<JsonElement>>(json);

            if (result == null || !result.Any())
                return Ok(Array.Empty<RecomendacaoResponse>());

            var veiculosVendidos = (await _context.Reservas
                .Where(r => r.Estado.ToLower() == "concluido")
                .Select(r => r.VeiculoId)
                .ToListAsync())
                .ToHashSet();

            var recomendaciones = new List<RecomendacaoResponse>();
            foreach (var element in result)
            {
                if (!TryParseVeiculoId(element, out var veiculoId))
                    continue;

                if (veiculosVendidos.Contains(veiculoId))
                    continue;

                if (!TryParseScore(element, out var score))
                    continue;

                recomendaciones.Add(new RecomendacaoResponse(veiculoId, score));
            }

            return Ok(recomendaciones);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao conectar com a IA: {ex.Message}");
        }
    }

    private static bool TryParseVeiculoId(JsonElement element, out int veiculoId)
    {
        veiculoId = 0;

        if (element.TryGetProperty("veiculoId", out var prop) ||
            element.TryGetProperty("veiculo_id", out prop) ||
            element.TryGetProperty("vehicle_id", out prop))
        {
            if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out veiculoId))
                return true;

            if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out veiculoId))
                return true;
        }

        return false;
    }

    private static bool TryParseScore(JsonElement element, out double score)
    {
        score = 0.0;

        if (element.TryGetProperty("score", out var prop))
        {
            if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDouble(out score))
                return true;

            if (prop.ValueKind == JsonValueKind.String && TryParseScoreString(prop.GetString(), out score))
                return true;
        }

        if (element.TryGetProperty("match_score", out var matchScoreProp) ||
            element.TryGetProperty("matchScore", out matchScoreProp))
        {
            if (matchScoreProp.ValueKind == JsonValueKind.Number && matchScoreProp.TryGetDouble(out score))
                return true;

            if (matchScoreProp.ValueKind == JsonValueKind.String && TryParseScoreString(matchScoreProp.GetString(), out score))
                return true;
        }

        return false;
    }

    private static bool TryParseScoreString(string? raw, out double score)
    {
        score = 0.0;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        raw = raw.Trim();
        if (raw.EndsWith("%"))
            raw = raw.Substring(0, raw.Length - 1).Trim();

        return double.TryParse(raw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out score);
    }
}
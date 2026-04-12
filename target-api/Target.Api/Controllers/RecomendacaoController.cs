using Microsoft.AspNetCore.Mvc;
using Target.Api.Services;


namespace Target.Api.Controllers;

// Controllers/RecomendacaoController.cs
[ApiController]
[Route("api/[controller]")]
public class RecomendacaoController : ControllerBase
{
    private readonly RecommendationService _aiService;

    public RecomendacaoController(RecommendationService aiService)
    {
        _aiService = aiService;
    }

    [HttpGet("usuario/{id}")]
    public async Task<IActionResult> Get(int id)
    {
        try 
        {
            var result = await _aiService.GetHybridRecommendations(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao conectar com a IA: {ex.Message}");
        }
    }
}
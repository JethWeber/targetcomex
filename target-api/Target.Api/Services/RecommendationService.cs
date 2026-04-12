namespace Target.Api.Services;

public class RecommendationService
{
    private readonly HttpClient _httpClient;

    public RecommendationService(IHttpClientFactory httpClientFactory)
    {
        // Puxa as configurações que definimos no Program.cs para o "AIService"
        _httpClient = httpClientFactory.CreateClient("AIService");
    }

    public async Task<object?> GetHybridRecommendations(int userId)
    {
        try
        {
            // Como o BaseAddress já está definido no Program.cs, basta passar o caminho relativo
            return await _httpClient.GetFromJsonAsync<object>($"recommend-hybrid/{userId}");
        }
        catch (Exception ex)
        {
            // Log para debug interno do container
            Console.WriteLine($"Erro ao chamar IA: {ex.Message}");
            throw;
        }
    }
}
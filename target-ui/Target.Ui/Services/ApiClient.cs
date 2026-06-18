using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Target.Ui.Services;

// ─── DTOs ────────────────────────────────────────────────────────────────────

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Senha { get; set; }
    public string? Telefone { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Genero { get; set; }
    public string? EstadoCivil { get; set; }
    public int? NumeroFilhos { get; set; }
    public string? Profissao { get; set; }
    public string? FaixaRendaMensal { get; set; }
    public List<string>? TiposUso { get; set; }
    public List<string>? InteressesPrincipais { get; set; }
    public string? Provincia { get; set; }
    public string? Municipio { get; set; }
    public string? Bairro { get; set; }
    public string? RuaComplemento { get; set; }
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Perfil => Role;
    public string Iniciais => string.Join("", Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => p[0])).ToUpper();
    public string? Provincia => Endereco?.Provincia;
    public DateTime? DataNascimento { get; set; }
    public string? Genero { get; set; }
    public string? EstadoCivil { get; set; }
    public int? NumeroFilhos { get; set; }
    public string? Profissao { get; set; }
    public string? FaixaRendaMensal { get; set; }
    public string? InteressesPrincipais { get; set; }
    public string? TipoDeUsoPretendido { get; set; }
    public DateTime DataCadastro { get; set; }
    public EnderecoDto? Endereco { get; set; }
}

public class EnderecoDto
{
    public int Id { get; set; }
    public string? Provincia { get; set; }
    public string? Municipio { get; set; }
    public string? Distrito { get; set; }
    public string? Bairro { get; set; }
    public string? RuaComplemento { get; set; }
}

public class UpdateUsuarioRequest
{
    public string? Nome { get; set; }
    public string? Telefone { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Genero { get; set; }
    public string? EstadoCivil { get; set; }
    public int? NumeroFilhos { get; set; }
    public string? Profissao { get; set; }
    public string? FaixaRendaMensal { get; set; }
    public string? InteressesPrincipais { get; set; }
    public string? TipoDeUsoPretendido { get; set; }
}

public class VeiculoDto
{
    public int Id { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
    public string? Descricao { get; set; }
    public string? ImagemUrl { get; set; }
    public string? Cor { get; set; }
    public string? Estilo { get; set; }
    public string? Combustivel { get; set; }
    public string? Motor { get; set; }
    public int? Potencia { get; set; }
    public string? Transmissao { get; set; }
    public string? Localizacao { get; set; }
    public int? Quilometragem { get; set; }
    public decimal Preco { get; set; }
    public bool Disponivel { get; set; }
}

public class VeiculoImagemDto
{
    public int Id { get; set; }
    public int VeiculoId { get; set; }
    public string ImagemUrl { get; set; } = string.Empty;
}

public class AvaliacaoDto
{
    public int Id { get; set; }
    public int VeiculoId { get; set; }
    public int UsuarioId { get; set; }
    public int Nota { get; set; }
    public string? Comentario { get; set; }
    public DateTime DataAvaliacao { get; set; }
}

public class AvaliacaoCreateRequest
{
    public int VeiculoId { get; set; }
    public int UsuarioId { get; set; }
    public int Nota { get; set; }
    public string? Comentario { get; set; }
}

public class HistoricoCompraDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int VeiculoId { get; set; }
    public DateTime DataCompra { get; set; }
    public decimal ValorPago { get; set; }
    public VeiculoDto? Veiculo { get; set; }
}

public class CreateVeiculoRequest
{
    public string  Marca         { get; set; } = string.Empty;
    public string  Modelo        { get; set; } = string.Empty;
    public int     Ano           { get; set; }
    public string? Descricao     { get; set; }
    public string? ImagemUrl     { get; set; }
    public string? Cor           { get; set; }
    public string? Estilo        { get; set; }
    public string? Combustivel   { get; set; }
    public string? Motor         { get; set; }
    public int?    Potencia      { get; set; }
    public string? Transmissao   { get; set; }
    public string? Localizacao   { get; set; }
    public int?    Quilometragem { get; set; }
    public decimal Preco         { get; set; }
    public bool    Disponivel    { get; set; } = true;
    // Galeria de imagens (opcional)
    public List<VeiculoImagemDto>? Galeria { get; set; } = new();
}

public class UpdateVeiculoRequest
{
    public string  Marca         { get; set; } = string.Empty;
    public string  Modelo        { get; set; } = string.Empty;
    public int     Ano           { get; set; }
    public string? Descricao     { get; set; }
    public string? ImagemUrl     { get; set; }
    public string? Cor           { get; set; }
    public string? Estilo        { get; set; }
    public string? Combustivel   { get; set; }
    public string? Motor         { get; set; }
    public int?    Potencia      { get; set; }
    public string? Transmissao   { get; set; }
    public string? Localizacao   { get; set; }
    public int?    Quilometragem { get; set; }
    public decimal Preco         { get; set; }
    public bool    Disponivel    { get; set; } = true;
    public List<VeiculoImagemDto>? Galeria { get; set; } = new();
}

public class RecomendacaoDto
{
    public int VeiculoId { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? ImagemUrl { get; set; }
    public decimal Preco { get; set; }
    public double Score { get; set; }
}

public class SmsRequest
{
    public string? To { get; set; }
    public string? Message { get; set; }
}

public class MaisCompradosDto
{
    public int Id { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public int Ano { get; set; }
    public string? ImagemUrl { get; set; }
    public decimal Preco { get; set; }
    public string? Cor { get; set; }
    public string? Combustivel { get; set; }
    public bool Disponivel { get; set; }
    public int TotalCompras { get; set; }
}

public class ReservaDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int VeiculoId { get; set; }
    public string? TipoPedido { get; set; }
    public string? Showroom { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime DataReserva { get; set; }
}

// Reservas
public class ReservaRequest
{
    public int     UsuarioId  { get; set; }
    public int     VeiculoId  { get; set; }
    public string? TipoPedido { get; set; }
    public string? Showroom   { get; set; }
}

// ─── Resultado genérico ───────────────────────────────────────────────────────

public class ApiResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }

    public static ApiResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResult<T> Fail(string error) => new() { Success = false, Error = error };
}

// ─── ApiClient ────────────────────────────────────────────────────────────────

public class ApiClient
{
    private readonly HttpClient  _http;
    private readonly TokenStore  _tokenStore;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // NOTA: o TokenStore é recebido directamente aqui (em vez de via
    // ApiAuthorizationMessageHandler) para garantir que partilha sempre o
    // MESMO scope/instância do AuthService. Handlers registados com
    // .AddHttpMessageHandler<T>() são construídos pelo IHttpClientFactory
    // num scope interno derivado do contentor raiz — não do scope do
    // circuito Blazor — pelo que um TokenStore Scoped injectado nesse
    // handler nunca é o mesmo objecto que o AuthService preenche. Resolver
    // o TokenStore aqui, no construtor do próprio ApiClient, evita esse
    // problema porque o ApiClient é resolvido a partir do scope correcto.
    public ApiClient(HttpClient http, TokenStore tokenStore)
    {
        _http       = http;
        _tokenStore = tokenStore;
    }

    /// <summary>
    /// Aplica (ou remove) o cabeçalho Authorization com base no valor actual
    /// do TokenStore. É chamado no início de cada pedido para garantir que
    /// lê sempre o token mais recente (definido por AuthService.InitAsync).
    /// </summary>
    private void AplicarToken()
    {
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(_tokenStore.Token)
            ? null
            : new AuthenticationHeaderValue("Bearer", _tokenStore.Token);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // AUTH
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Login — devolve o JWT em caso de sucesso.</summary>
    public async Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            AplicarToken();
            var response = await _http.PostAsJsonAsync("api/Auth/login", request, _json);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<LoginResponse>(_json);
                return ApiResult<LoginResponse>.Ok(data!);
            }
            var error = await response.Content.ReadAsStringAsync();
            return ApiResult<LoginResponse>.Fail(error);
        }
        catch (Exception ex)
        {
            return ApiResult<LoginResponse>.Fail(ex.Message);
        }
    }

    /// <summary>Registo de novo utilizador.</summary>
    public async Task<ApiResult<string>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            AplicarToken();
            var response = await _http.PostAsJsonAsync("api/Auth/register", request, _json);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode
                ? ApiResult<string>.Ok(body)
                : ApiResult<string>.Fail(body);
        }
        catch (Exception ex)
        {
            return ApiResult<string>.Fail(ex.Message);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // USERS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Lista todos os utilizadores (requer admin).</summary>
    public async Task<ApiResult<List<UsuarioDto>>> GetUsuariosAsync()
    {
        try
        {
            AplicarToken();
            var data = await _http.GetFromJsonAsync<List<UsuarioDto>>("api/Users", _json);
            return ApiResult<List<UsuarioDto>>.Ok(data ?? []);
        }
        catch (Exception ex)
        {
            return ApiResult<List<UsuarioDto>>.Fail(ex.Message);
        }
    }

    /// <summary>Obtém um utilizador pelo id.</summary>
    public async Task<ApiResult<UsuarioDto>> GetUsuarioAsync(int id)
    {
        try
        {
            AplicarToken();
            var data = await _http.GetFromJsonAsync<UsuarioDto>($"api/Users/{id}", _json);
            return ApiResult<UsuarioDto>.Ok(data!);
        }
        catch (Exception ex)
        {
            return ApiResult<UsuarioDto>.Fail(ex.Message);
        }
    }

    /// <summary>Actualiza dados do utilizador.</summary>
    public async Task<ApiResult<string>> UpdateUsuarioAsync(int id, UpdateUsuarioRequest request)
    {
        try
        {
            AplicarToken();
            var response = await _http.PutAsJsonAsync($"api/Users/{id}", request, _json);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode
                ? ApiResult<string>.Ok(body)
                : ApiResult<string>.Fail(body);
        }
        catch (Exception ex)
        {
            return ApiResult<string>.Fail(ex.Message);
        }
    }

    /// <summary>Remove um utilizador.</summary>
    public async Task<ApiResult<string>> DeleteUsuarioAsync(int id)
    {
        try
        {
            AplicarToken();
            var response = await _http.DeleteAsync($"api/Users/{id}");
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode
                ? ApiResult<string>.Ok(body)
                : ApiResult<string>.Fail(body);
        }
        catch (Exception ex)
        {
            return ApiResult<string>.Fail(ex.Message);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // VEÍCULOS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Lista todos os veículos.</summary>
    public async Task<ApiResult<List<VeiculoDto>>> GetVeiculosAsync()
    {
        try
        {
            AplicarToken();
            var data = await _http.GetFromJsonAsync<List<VeiculoDto>>("api/Veiculos", _json);
            return ApiResult<List<VeiculoDto>>.Ok(data ?? []);
        }
        catch (Exception ex)
        {
            return ApiResult<List<VeiculoDto>>.Fail(ex.Message);
        }
    }

    /// <summary>Obtém um veículo pelo id.</summary>
    public async Task<ApiResult<VeiculoDto>> GetVeiculoAsync(int id)
    {
        try
        {
            AplicarToken();
            var data = await _http.GetFromJsonAsync<VeiculoDto>($"api/Veiculos/{id}", _json);
            return ApiResult<VeiculoDto>.Ok(data!);
        }
        catch (Exception ex)
        {
            return ApiResult<VeiculoDto>.Fail(ex.Message);
        }
    }

    /// <summary>Cria um veículo (requer admin/vendedor).</summary>
    public async Task<ApiResult<VeiculoDto>> CreateVeiculoAsync(CreateVeiculoRequest request)
    {
        try
        {
            AplicarToken();
            var response = await _http.PostAsJsonAsync("api/Veiculos", request, _json);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<VeiculoDto>(_json);
                return ApiResult<VeiculoDto>.Ok(data!);
            }
            var error = await response.Content.ReadAsStringAsync();
            return ApiResult<VeiculoDto>.Fail(error);
        }
        catch (Exception ex)
        {
            return ApiResult<VeiculoDto>.Fail(ex.Message);
        }
    }

    /// <summary>Actualiza um veículo (requer admin/vendedor).</summary>
    public async Task<ApiResult<string>> UpdateVeiculoAsync(int id, UpdateVeiculoRequest request)
    {
        try
        {
            AplicarToken();
            var response = await _http.PutAsJsonAsync($"api/Veiculos/{id}", request, _json);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode
                ? ApiResult<string>.Ok(body)
                : ApiResult<string>.Fail(body);
        }
        catch (Exception ex)
        {
            return ApiResult<string>.Fail(ex.Message);
        }
    }

    /// <summary>Remove um veículo (requer admin).</summary>
    public async Task<ApiResult<string>> DeleteVeiculoAsync(int id)
    {
        try
        {
            AplicarToken();
            var response = await _http.DeleteAsync($"api/Veiculos/{id}");
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode
                ? ApiResult<string>.Ok(body)
                : ApiResult<string>.Fail(body);
        }
        catch (Exception ex)
        {
            return ApiResult<string>.Fail(ex.Message);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // UPLOAD DA imagem do carro
    public async Task<ApiResult<string>> UploadCapaAsync(byte[] bytes, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        var response = await _http.PostAsync("api/upload/capa", content);
        if (!response.IsSuccessStatusCode)
            return new ApiResult<string> { Success = false, Error = await response.Content.ReadAsStringAsync() };

        var url = await response.Content.ReadAsStringAsync();
        return new ApiResult<string> { Success = true, Data = url.Trim('"') };
    }
    
    /// <summary>Lista as avaliações de um veículo.</summary>
    public async Task<ApiResult<List<AvaliacaoDto>>> GetAvaliacoesPorVeiculoAsync(int veiculoId)
    {
        try
        {
            AplicarToken();
            var data = await _http.GetFromJsonAsync<List<AvaliacaoDto>>($"api/Avaliacoes/veiculo/{veiculoId}", _json);
            return ApiResult<List<AvaliacaoDto>>.Ok(data ?? []);
        }
        catch (Exception ex)
        {
            return ApiResult<List<AvaliacaoDto>>.Fail(ex.Message);
        }
    }

    /// <summary>Cria uma nova avaliação.</summary>
    public async Task<ApiResult<AvaliacaoDto>> CriarAvaliacaoAsync(AvaliacaoCreateRequest request)
    {
        try
        {
            AplicarToken();
            var response = await _http.PostAsJsonAsync("api/Avaliacoes", request, _json);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<AvaliacaoDto>(_json);
                return ApiResult<AvaliacaoDto>.Ok(data!);
            }
            var error = await response.Content.ReadAsStringAsync();
            return ApiResult<AvaliacaoDto>.Fail(error);
        }
        catch (Exception ex)
        {
            return ApiResult<AvaliacaoDto>.Fail(ex.Message);
        }
    }

    /// <summary>Lista o histórico de compras de um utilizador.</summary>
    public async Task<ApiResult<List<HistoricoCompraDto>>> GetHistoricoComprasPorUsuarioAsync(int usuarioId)
    {
        try
        {
            AplicarToken();
            var data = await _http.GetFromJsonAsync<List<HistoricoCompraDto>>($"api/HistoricoCompras/usuario/{usuarioId}", _json);
            return ApiResult<List<HistoricoCompraDto>>.Ok(data ?? []);
        }
        catch (Exception ex)
        {
            return ApiResult<List<HistoricoCompraDto>>.Fail(ex.Message);
        }
    }

    /// <summary>Lista todas as reservas.</summary>
    public async Task<ApiResult<List<ReservaDto>>> GetReservasAsync()
    {
        try
        {
            AplicarToken();
            var data = await _http.GetFromJsonAsync<List<ReservaDto>>("api/Reservas", _json);
            return ApiResult<List<ReservaDto>>.Ok(data ?? []);
        }
        catch (Exception ex)
        {
            return ApiResult<List<ReservaDto>>.Fail(ex.Message);
        }
    }

    /// <summary>Altera o estado de uma reserva.</summary>
    public async Task<ApiResult<string>> UpdateReservaEstadoAsync(int id, string novoEstado)
    {
        try
        {
            AplicarToken();
            var response = await _http.PutAsJsonAsync($"api/Reservas/{id}/estado", novoEstado, _json);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode
                ? ApiResult<string>.Ok(body)
                : ApiResult<string>.Fail(body);
        }
        catch (Exception ex)
        {
            return ApiResult<string>.Fail(ex.Message);
        }
    }

    /// <summary>Cria uma nova reserva.</summary>
    public async Task<ApiResult<ReservaDto>> CriarReservaAsync(ReservaRequest request)
    {
        try
        {
            AplicarToken();
            var response = await _http.PostAsJsonAsync("api/Reservas", request, _json);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<ReservaDto>(_json);
                return ApiResult<ReservaDto>.Ok(data!);
            }
            var error = await response.Content.ReadAsStringAsync();
            return ApiResult<ReservaDto>.Fail(error);
        }
        catch (Exception ex)
        {
            return ApiResult<ReservaDto>.Fail(ex.Message);
        }
    }

    /// <summary>Registra o histórico de navegação de um utilizador.</summary>
    public async Task<ApiResult<string>> RegistrarHistoricoNavegacaoAsync(int usuarioId, int veiculoId)
    {
        try
        {
            AplicarToken();
            var response = await _http.PostAsJsonAsync("api/HistoricoNavegacao", new { UsuarioId = usuarioId, VeiculoId = veiculoId }, _json);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode
                ? ApiResult<string>.Ok(body)
                : ApiResult<string>.Fail(body);
        }
        catch (Exception ex)
        {
            return ApiResult<string>.Fail(ex.Message);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // RECOMENDAÇÃO
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Obtém recomendações personalizadas para um utilizador.</summary>
    public async Task<ApiResult<List<RecomendacaoDto>>> GetRecomendacoesAsync(int usuarioId)
    {
        try
        {
            AplicarToken();
            var data = await _http.GetFromJsonAsync<List<RecomendacaoDto>>(
                $"api/Recomendacao/usuario/{usuarioId}", _json);
            return ApiResult<List<RecomendacaoDto>>.Ok(data ?? []);
        }
        catch (Exception ex)
        {
            return ApiResult<List<RecomendacaoDto>>.Fail(ex.Message);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // MAIS COMPRADOS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Devolve os veículos mais comprados (endpoint público, não requer token).
    /// Se não houver histórico retorna os últimos veículos adicionados (fallback da API).
    /// </summary>
    public async Task<ApiResult<List<MaisCompradosDto>>> GetMaisCompradosAsync(int top = 6)
    {
        try
        {
            AplicarToken();
            var data = await _http.GetFromJsonAsync<List<MaisCompradosDto>>(
                $"api/HistoricoCompras/mais-comprados?top={top}", _json);
            return ApiResult<List<MaisCompradosDto>>.Ok(data ?? []);
        }
        catch (Exception ex)
        {
            return ApiResult<List<MaisCompradosDto>>.Fail(ex.Message);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // SMS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Envia um SMS.</summary>
    public async Task<ApiResult<string>> EnviarSmsAsync(SmsRequest request)
    {
        try
        {
            AplicarToken();
            var response = await _http.PostAsJsonAsync("api/sms/enviar", request, _json);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode
                ? ApiResult<string>.Ok(body)
                : ApiResult<string>.Fail(body);
        }
        catch (Exception ex)
        {
            return ApiResult<string>.Fail(ex.Message);
        }
    }
}

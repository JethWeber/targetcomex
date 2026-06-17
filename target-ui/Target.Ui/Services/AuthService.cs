using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Target.Ui.Services;

public class AuthService
{
    private const string LocalTokenKey   = "targetcomex_jwt";
    private const string SessionTokenKey = "targetcomex_jwt_session";

    private readonly IJSRuntime        _jsRuntime;
    private readonly NavigationManager _navigation;
    private readonly TokenStore        _tokenStore;

    public AuthService(IJSRuntime jsRuntime, NavigationManager navigation, TokenStore tokenStore)
    {
        _jsRuntime  = jsRuntime;
        _navigation = navigation;
        _tokenStore = tokenStore;
    }

    // ── Inicializar — chamar no OnAfterRenderAsync de páginas autenticadas ────
    // Copia o token do localStorage/sessionStorage para o TokenStore em memória.
    // Deve ser chamado ANTES de qualquer pedido HTTP autenticado.
    public async Task InitAsync()
    {
        _tokenStore.Token = await GetTokenAsync();
    }

    // ── Token bruto (lê do JS — apenas usar em contexto de renderização) ──────

    public async Task<string?> GetTokenAsync()
    {
        var sessionToken = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", SessionTokenKey);
        if (!string.IsNullOrWhiteSpace(sessionToken))
            return sessionToken;

        var localToken = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", LocalTokenKey);
        return string.IsNullOrWhiteSpace(localToken) ? null : localToken;
    }

    public async Task SetTokenAsync(string token, bool remember)
    {
        _tokenStore.Token = token; // sincroniza imediatamente após login

        if (remember)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem",     LocalTokenKey,   token);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", SessionTokenKey);
        }
        else
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem",  SessionTokenKey, token);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem",  LocalTokenKey);
        }
    }

    public async Task RemoveTokenAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem",   LocalTokenKey);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", SessionTokenKey);
    }

    // ── Estado ────────────────────────────────────────────────────────────────

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task<string?> GetBearerTokenAsync()
    {
        var token = await GetTokenAsync();
        return string.IsNullOrWhiteSpace(token) ? null : token;
    }

    // ── Claims do JWT ─────────────────────────────────────────────────────────

    public async Task<int?> GetUserIdAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token)) return null;

        try
        {
            var jwt    = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var userId = jwt.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "nameid" ||
                c.Type == "sub")?.Value;

            return int.TryParse(userId, out var id) ? id : null;
        }
        catch { return null; }
    }

    public async Task<string?> GetRoleAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token)) return null;

        try
        {
            var jwt  = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var role = jwt.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Role ||
                c.Type == "role" ||
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            return role;
        }
        catch { return null; }
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    public async Task LogoutAsync()
    {
        _tokenStore.Token = null; // limpa o store imediatamente
        await RemoveTokenAsync();
        _navigation.NavigateTo("/login", forceLoad: true);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Target.Ui.Services;

public class AuthService
{
    private const string LocalTokenKey    = "targetcomex_jwt";
    private const string SessionTokenKey  = "targetcomex_jwt_session";
    private readonly IJSRuntime        _jsRuntime;
    private readonly NavigationManager _navigation;

    public AuthService(IJSRuntime jsRuntime, NavigationManager navigation)
    {
        _jsRuntime  = jsRuntime;
        _navigation = navigation;
    }

    // ── Token bruto ───────────────────────────────────────────────────────────

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

    /// <summary>
    /// Devolve o Role do utilizador autenticado (ex: "Admin", "Cliente").
    /// Retorna null se não houver sessão ou se o token for inválido.
    /// </summary>
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
        await RemoveTokenAsync();
        _navigation.NavigateTo("/login", forceLoad: true);
    }
}

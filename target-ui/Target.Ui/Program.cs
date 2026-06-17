using Target.Ui.Components;
using Target.Ui.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// ── Razor / Server ──────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── Data Protection ─────────────────────────────────────────────
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")));

// ── Token Store + Auth Service ──────────────────────────────────
// TokenStore DEVE ser Scoped (mesmo ciclo de vida que AuthService e ApiClient)
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<AuthService>();

// ── HttpClient apontando para a API ─────────────────────────────
// NOTA: já não usamos ApiAuthorizationMessageHandler aqui. Handlers
// registados via .AddHttpMessageHandler<T>() são construídos pelo
// IHttpClientFactory num scope interno (derivado do contentor raiz),
// que NÃO é o mesmo scope do circuito Blazor — por isso o TokenStore
// Scoped injectado nesse handler nunca correspondia ao TokenStore
// preenchido pelo AuthService, e os pedidos autenticados (ex: api/Users)
// saíam sempre sem o cabeçalho Authorization. O ApiClient agora recebe o
// TokenStore directamente no seu próprio construtor, partilhando assim o
// mesmo scope do AuthService.
var apiUrl = (builder.Configuration["ApiServiceUrl"] ?? "http://localhost:5000").TrimEnd('/');
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

// ────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

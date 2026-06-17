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

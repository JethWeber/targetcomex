using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System.Text;
using Target.Api.Data;
using Target.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ===================== CORS =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// ===================== DB CONTEXT =====================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===================== JWT =====================
// O OIDC/IdentityModel exige chave HMAC >= 256 bits para HMAC-SHA256.
// Use um segredo com >= 32 bytes para evitar IDX10720.
var key = Encoding.ASCII.GetBytes("TARGETCOMEX_SUPER_SECRET_KEY_1234567890");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// ===================== SERVICES =====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Target Comex API",
        Version = "v1",
        Description = "Inclui Auth, Users, Veiculos (CRUD) e Recomendacao."
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Cole apenas o token JWT. O Swagger adiciona 'Bearer ' automaticamente."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Serviço de IA
builder.Services.AddScoped<RecommendationService>();

// HttpClient para IA
builder.Services.AddHttpClient("AIService", client =>
{
    var aiUrl = builder.Configuration["AIServiceUrl"] ?? "http://targetcomexcr:8000";
    client.BaseAddress = new Uri(aiUrl);
});

// ===================== APP =====================
var app = builder.Build();

// Evita saltos grandes em colunas IDENTITY (ex.: +1000) após restart do SQL Server 2017+
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.ExecuteSqlRaw(
            "ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = OFF;");
    }
    catch
    {
        // Ignora se não for SQL Server 2017+ ou sem permissão ALTER na base.
    }
}

if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Target Comex API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");

// Autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
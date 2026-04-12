var builder = WebApplication.CreateBuilder(args);

// 1. Configuração de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// 2. Registro de Serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- ADICIONE ESTA LINHA ABAIXO ---
builder.Services.AddScoped<Target.Api.Services.RecommendationService>();
// ---------------------------------

// 3. Configura o HttpClient para a IA
builder.Services.AddHttpClient("AIService", client =>
{
    var aiUrl = builder.Configuration["AIServiceUrl"] ?? "http://targetcomexcr:8000";
    client.BaseAddress = new Uri(aiUrl);
});

var app = builder.Build();

// 4. Pipeline (Middleware)
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
app.UseAuthorization();
app.MapControllers();

app.Run();
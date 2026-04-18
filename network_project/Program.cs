using Microsoft.EntityFrameworkCore;
using QuantumCyberAnalyzer.Data;
using QuantumCyberAnalyzer.Interfaces;
using QuantumCyberAnalyzer.Repository;
using QuantumCyberAnalyzer.Helper;
using QuantumCyberAnalyzer.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<INetworkLogRepository, NetworkLogRepository>();
builder.Services.AddScoped<INodeRepository, NodeRepository>();
builder.Services.AddScoped<IEdgeRepository, EdgeRepository>();
builder.Services.AddScoped<IQuantumWalkResultRepository, QuantumWalkResultRepository>();
builder.Services.AddScoped<IQftResultRepository, QftResultRepository>();
builder.Services.AddScoped<IDetectionResultRepository, DetectionResultRepository>();

// ── Helpers / Services ────────────────────────────────────────────────────────
builder.Services.AddScoped<CsvParserHelper>();
builder.Services.AddScoped<GraphBuilderHelper>();
builder.Services.AddScoped<QuantumWalkHelper>();
builder.Services.AddScoped<QftAnalysisHelper>();
builder.Services.AddScoped<ThreatScoringHelper>();

// ── Web ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Quantum Cyber Analyzer API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionHandler>();   // must be first

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Quantum Cyber Analyzer v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ── Auto-migrate on startup ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

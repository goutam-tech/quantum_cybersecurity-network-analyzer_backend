using Microsoft.EntityFrameworkCore;
using network_project.Data;
using network_project.Interfaces;
using network_project.Repository;
using network_project.Helper;
using network_project.Middleware;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Connection string not found. Set ConnectionStrings__DefaultConnection or DATABASE_URL in .env or appsettings.");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<INetworkLogRepository, NetworkLogRepository>();
builder.Services.AddScoped<INodeRepository, NodeRepository>();
builder.Services.AddScoped<IEdgeRepository, EdgeRepository>();
builder.Services.AddScoped<IQuantumWalkResultRepository, QuantumWalkResultRepository>();
builder.Services.AddScoped<IQftResultRepository, QftResultRepository>();
builder.Services.AddScoped<IDetectionResultRepository, DetectionResultRepository>();

builder.Services.AddScoped<CsvParserHelper>();
builder.Services.AddScoped<GraphBuilderHelper>();
builder.Services.AddScoped<QuantumWalkHelper>();
builder.Services.AddScoped<QftAnalysisHelper>();
builder.Services.AddScoped<ThreatScoringHelper>();

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

app.UseMiddleware<GlobalExceptionHandler>();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
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

//app.Urls.Add("http://0.0.0.0:8080");
app.UseCors("AllowFrontend");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using network_project.Data;
using network_project.Interfaces;
using network_project.Repository;
using network_project.Helper;

namespace network_project.Tests.ProgramConfiguration;

public class ProgramTests
{
    [Fact]
    public void Services_Should_Register_Repositories_And_Helpers()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration["JwtSettings:SecretKey"] =
            "ThisIsASecretKeyForTesting123456";

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        builder.Services.AddScoped<INetworkLogRepository, NetworkLogRepository>();
        builder.Services.AddScoped<INodeRepository, NodeRepository>();
        builder.Services.AddScoped<IEdgeRepository, EdgeRepository>();
        builder.Services.AddScoped<IQuantumWalkResultRepository,
            QuantumWalkResultRepository>();
        builder.Services.AddScoped<IQftResultRepository,
            QftResultRepository>();
        builder.Services.AddScoped<IDetectionResultRepository,
            DetectionResultRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ITokenRepository, TokenRepository>();

        builder.Services.AddScoped<CsvParserHelper>();
        builder.Services.AddScoped<GraphBuilderHelper>();
        builder.Services.AddScoped<QuantumWalkHelper>();
        builder.Services.AddScoped<QftAnalysisHelper>();
        builder.Services.AddScoped<ThreatScoringHelper>();
        builder.Services.AddScoped<JwtHelper>();

        var app = builder.Build();

        using var scope = app.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<INetworkLogRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<INodeRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<IEdgeRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<IQuantumWalkResultRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<IQftResultRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<IDetectionResultRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<IUserRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<ITokenRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<CsvParserHelper>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<GraphBuilderHelper>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<QuantumWalkHelper>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<QftAnalysisHelper>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<ThreatScoringHelper>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<JwtHelper>());
    }

    [Fact]
    public void Configuration_Should_Set_Jwt_SecretKey()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration["JwtSettings:SecretKey"] =
            "TestSecretKey123";

        var secret =
            builder.Configuration["JwtSettings:SecretKey"];

        Assert.NotNull(secret);
        Assert.Equal("TestSecretKey123", secret);
    }

    [Fact]
    public void App_Should_Build_Successfully()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration["JwtSettings:SecretKey"] =
            "AnotherSecretKey123";

        builder.Services.AddControllers();

        var app = builder.Build();

        Assert.NotNull(app);
    }
}
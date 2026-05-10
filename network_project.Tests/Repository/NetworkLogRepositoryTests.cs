using Microsoft.EntityFrameworkCore;
using network_project.Data;
using network_project.Models;
using network_project.Repository;

namespace network_project.Tests.Repository;

public class NetworkLogRepositoryTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Logs()
    {
        var context = GetDbContext();

        context.NetworkLogs.AddRange(
            new NetworkLog { SourceIp = "1.1.1.1", Timestamp = DateTime.UtcNow },
            new NetworkLog { SourceIp = "2.2.2.2", Timestamp = DateTime.UtcNow }
        );

        await context.SaveChangesAsync();

        var repo = new NetworkLogRepository(context);

        var result = await repo.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Log()
    {
        var context = GetDbContext();

        var log = new NetworkLog
        {
            SourceIp = "1.1.1.1",
            Timestamp = DateTime.UtcNow
        };

        context.NetworkLogs.Add(log);
        await context.SaveChangesAsync();

        var repo = new NetworkLogRepository(context);

        var result = await repo.GetByIdAsync(log.LogId);

        Assert.NotNull(result);
        Assert.Equal(log.LogId, result!.LogId);
    }

    [Fact]
    public async Task AddAsync_Should_Add_Log()
    {
        var context = GetDbContext();

        var repo = new NetworkLogRepository(context);

        var log = new NetworkLog
        {
            SourceIp = "3.3.3.3",
            Timestamp = DateTime.UtcNow
        };

        await repo.AddAsync(log);
        await repo.SaveChangesAsync();

        Assert.Equal(1, await context.NetworkLogs.CountAsync());
    }

    [Fact]
    public async Task AddRangeAsync_Should_Add_Logs()
    {
        var context = GetDbContext();

        var repo = new NetworkLogRepository(context);

        var logs = new List<NetworkLog>
        {
            new NetworkLog { SourceIp = "1.1.1.1", Timestamp = DateTime.UtcNow },
            new NetworkLog { SourceIp = "2.2.2.2", Timestamp = DateTime.UtcNow }
        };

        await repo.AddRangeAsync(logs);
        await repo.SaveChangesAsync();

        Assert.Equal(2, await context.NetworkLogs.CountAsync());
    }

    [Fact]
    public async Task GetBySourceIpAsync_Should_Return_Filtered_Logs()
    {
        var context = GetDbContext();

        context.NetworkLogs.AddRange(
            new NetworkLog { SourceIp = "10.0.0.1", Timestamp = DateTime.UtcNow },
            new NetworkLog { SourceIp = "20.0.0.1", Timestamp = DateTime.UtcNow }
        );

        await context.SaveChangesAsync();

        var repo = new NetworkLogRepository(context);

        var result = await repo.GetBySourceIpAsync("10.0.0.1");

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByDateRangeAsync_Should_Return_Logs_Within_Range()
    {
        var context = GetDbContext();

        context.NetworkLogs.AddRange(
            new NetworkLog
            {
                SourceIp = "1.1.1.1",
                Timestamp = DateTime.UtcNow.AddDays(-1)
            },
            new NetworkLog
            {
                SourceIp = "2.2.2.2",
                Timestamp = DateTime.UtcNow
            }
        );

        await context.SaveChangesAsync();

        var repo = new NetworkLogRepository(context);

        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);

        var result = await repo.GetByDateRangeAsync(from, to);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetCountAsync_Should_Return_Total_Count()
    {
        var context = GetDbContext();

        context.NetworkLogs.AddRange(
            new NetworkLog { SourceIp = "1.1.1.1", Timestamp = DateTime.UtcNow },
            new NetworkLog { SourceIp = "2.2.2.2", Timestamp = DateTime.UtcNow }
        );

        await context.SaveChangesAsync();

        var repo = new NetworkLogRepository(context);

        var count = await repo.GetCountAsync();

        Assert.Equal(2, count);
    }
}
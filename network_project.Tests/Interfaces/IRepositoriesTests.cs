using Microsoft.EntityFrameworkCore;
using network_project.Data;
using network_project.Interfaces;
using network_project.Models;
using network_project.Repository;

namespace network_project.Tests.Interfaces;

public class RepositoryInterfaceTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task INetworkLogRepository_GetCountAsync_Should_Return_Count()
    {
        var context = GetDbContext();

        context.NetworkLogs.AddRange(
            new NetworkLog { SourceIp = "1.1.1.1", Timestamp = DateTime.UtcNow },
            new NetworkLog { SourceIp = "2.2.2.2", Timestamp = DateTime.UtcNow }
        );

        await context.SaveChangesAsync();

        INetworkLogRepository repo = new NetworkLogRepository(context);

        var result = await repo.GetCountAsync();

        Assert.Equal(2, result);
    }

    [Fact]
    public async Task INodeRepository_GetByIpAsync_Should_Return_Node()
    {
        var context = GetDbContext();

        context.Nodes.Add(new Node
        {
            IpAddress = "192.168.1.1",
            TotalConnections = 1
        });

        await context.SaveChangesAsync();

        INodeRepository repo = new NodeRepository(context);

        var result = await repo.GetByIpAsync("192.168.1.1");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task INodeRepository_UpsertNodeAsync_Should_Add_Node()
    {
        var context = GetDbContext();

        INodeRepository repo = new NodeRepository(context);

        await repo.UpsertNodeAsync("10.0.0.1");
        await repo.SaveChangesAsync();

        Assert.Single(context.Nodes);
    }

    [Fact]
    public async Task IEdgeRepository_GetEdgesForIpAsync_Should_Return_Edges()
    {
        var context = GetDbContext();

        context.Edges.Add(new Edge
        {
            SourceIp = "1.1.1.1",
            DestIp = "2.2.2.2",
            Weight = 1
        });

        await context.SaveChangesAsync();

        IEdgeRepository repo = new EdgeRepository(context);

        var result = await repo.GetEdgesForIpAsync("1.1.1.1");

        Assert.Single(result);
    }

    [Fact]
    public async Task IEdgeRepository_UpsertEdgeAsync_Should_Add_Edge()
    {
        var context = GetDbContext();

        IEdgeRepository repo = new EdgeRepository(context);

        await repo.UpsertEdgeAsync("1.1.1.1", "2.2.2.2");
        await repo.SaveChangesAsync();

        Assert.Single(context.Edges);
    }

    [Fact]
    public async Task IQuantumWalkResultRepository_GetTopAnomaliesAsync_Should_Return_Data()
    {
        var context = GetDbContext();

        var node = new Node
        {
            IpAddress = "5.5.5.5"
        };

        context.Nodes.Add(node);
        await context.SaveChangesAsync();

        context.QuantumWalkResults.Add(new QuantumWalkResult
        {
            NodeId = node.NodeId,
            AnomalyScore = 0.95
        });

        await context.SaveChangesAsync();

        IQuantumWalkResultRepository repo =
            new QuantumWalkResultRepository(context);

        var result = await repo.GetTopAnomaliesAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task IQftResultRepository_GetHighPeriodicityAsync_Should_Return_Data()
    {
        var context = GetDbContext();

        var node = new Node
        {
            IpAddress = "6.6.6.6"
        };

        context.Nodes.Add(node);
        await context.SaveChangesAsync();

        context.QftResults.Add(new QftResult
        {
            NodeId = node.NodeId,
            PeriodicityScore = 0.9
        });

        await context.SaveChangesAsync();

        IQftResultRepository repo = new QftResultRepository(context);

        var result = await repo.GetHighPeriodicityAsync(0.5);

        Assert.Single(result);
    }

    [Fact]
    public async Task IDetectionResultRepository_GetByThreatLevelAsync_Should_Return_Data()
    {
        var context = GetDbContext();

        var node = new Node
        {
            IpAddress = "7.7.7.7"
        };

        context.Nodes.Add(node);
        await context.SaveChangesAsync();

        context.DetectionResults.Add(new DetectionResult
        {
            NodeId = node.NodeId,
            ThreatLevel = "High",
            DetectedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        IDetectionResultRepository repo =
            new DetectionResultRepository(context);

        var result = await repo.GetByThreatLevelAsync("High");

        Assert.Single(result);
    }

    [Fact]
    public async Task IDetectionResultRepository_ClearAllAsync_Should_Remove_Data()
    {
        var context = GetDbContext();

        context.DetectionResults.Add(new DetectionResult
        {
            ThreatLevel = "Low",
            DetectedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        IDetectionResultRepository repo =
            new DetectionResultRepository(context);

        await repo.ClearAllAsync();

        Assert.Empty(context.DetectionResults);
    }
}
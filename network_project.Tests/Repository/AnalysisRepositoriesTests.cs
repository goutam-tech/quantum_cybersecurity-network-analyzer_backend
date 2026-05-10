using Microsoft.EntityFrameworkCore;
using network_project.Data;
using network_project.Models;
using network_project.Repository;

namespace network_project.Tests.Repository;

public class RepositoryTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetByIpAsync_Should_Return_Node()
    {
        var context = GetDbContext();

        context.Nodes.Add(new Node
        {
            IpAddress = "192.168.1.1",
            TotalConnections = 1
        });

        await context.SaveChangesAsync();

        var repo = new NodeRepository(context);

        var result = await repo.GetByIpAsync("192.168.1.1");

        Assert.NotNull(result);
        Assert.Equal("192.168.1.1", result!.IpAddress);
    }

    [Fact]
    public async Task GetHighAnomalyNodesAsync_Should_Return_Filtered_Nodes()
    {
        var context = GetDbContext();

        context.Nodes.AddRange(
            new Node { IpAddress = "1.1.1.1", AnomalyScore = 0.9 },
            new Node { IpAddress = "2.2.2.2", AnomalyScore = 0.4 }
        );

        await context.SaveChangesAsync();

        var repo = new NodeRepository(context);

        var result = await repo.GetHighAnomalyNodesAsync(0.5);

        Assert.Single(result);
    }

    [Fact]
    public async Task UpsertNodeAsync_Should_Add_New_Node()
    {
        var context = GetDbContext();
        var repo = new NodeRepository(context);

        await repo.UpsertNodeAsync("10.0.0.1");
        await context.SaveChangesAsync();

        var node = await context.Nodes.FirstOrDefaultAsync();

        Assert.NotNull(node);
        Assert.Equal(1, node!.TotalConnections);
    }

    [Fact]
    public async Task UpsertNodeAsync_Should_Increment_TotalConnections()
    {
        var context = GetDbContext();

        context.Nodes.Add(new Node
        {
            IpAddress = "10.0.0.1",
            TotalConnections = 1
        });

        await context.SaveChangesAsync();

        var repo = new NodeRepository(context);

        await repo.UpsertNodeAsync("10.0.0.1");
        await context.SaveChangesAsync();

        var node = await context.Nodes.FirstAsync();

        Assert.Equal(2, node.TotalConnections);
    }

    [Fact]
    public async Task GetEdgesForIpAsync_Should_Return_Matching_Edges()
    {
        var context = GetDbContext();

        context.Edges.AddRange(
            new Edge { SourceIp = "1.1.1.1", DestIp = "2.2.2.2", Weight = 1 },
            new Edge { SourceIp = "3.3.3.3", DestIp = "4.4.4.4", Weight = 1 }
        );

        await context.SaveChangesAsync();

        var repo = new EdgeRepository(context);

        var result = await repo.GetEdgesForIpAsync("1.1.1.1");

        Assert.Single(result);
    }

    [Fact]
    public async Task UpsertEdgeAsync_Should_Add_New_Edge()
    {
        var context = GetDbContext();
        var repo = new EdgeRepository(context);

        await repo.UpsertEdgeAsync("1.1.1.1", "2.2.2.2");
        await context.SaveChangesAsync();

        var edge = await context.Edges.FirstOrDefaultAsync();

        Assert.NotNull(edge);
        Assert.Equal(1, edge!.Weight);
    }

    [Fact]
    public async Task UpsertEdgeAsync_Should_Increment_Weight()
    {
        var context = GetDbContext();

        context.Edges.Add(new Edge
        {
            SourceIp = "1.1.1.1",
            DestIp = "2.2.2.2",
            Weight = 1
        });

        await context.SaveChangesAsync();

        var repo = new EdgeRepository(context);

        await repo.UpsertEdgeAsync("1.1.1.1", "2.2.2.2");
        await context.SaveChangesAsync();

        var edge = await context.Edges.FirstAsync();

        Assert.Equal(2, edge.Weight);
    }

    [Fact]
    public async Task GetByNodeIdAsync_Should_Return_QuantumWalkResult()
    {
        var context = GetDbContext();

        var node = new Node { IpAddress = "1.1.1.1" };

        context.Nodes.Add(node);
        await context.SaveChangesAsync();

        context.QuantumWalkResults.Add(new QuantumWalkResult
        {
            NodeId = node.NodeId,
            AnomalyScore = 0.95
        });

        await context.SaveChangesAsync();

        var repo = new QuantumWalkResultRepository(context);

        var result = await repo.GetByNodeIdAsync(node.NodeId);

        Assert.NotNull(result);
        Assert.Equal(node.NodeId, result!.NodeId);
    }

    [Fact]
    public async Task GetTopAnomaliesAsync_Should_Return_Top_Results()
    {
        var context = GetDbContext();

        var node = new Node
        {
            IpAddress = "1.1.1.1"
        };

        context.Nodes.Add(node);
        await context.SaveChangesAsync();

        context.QuantumWalkResults.AddRange(
            new QuantumWalkResult
            {
                NodeId = node.NodeId,
                AnomalyScore = 0.9
            },
            new QuantumWalkResult
            {
                NodeId = node.NodeId,
                AnomalyScore = 0.2
            }
        );

        await context.SaveChangesAsync();

        var repo = new QuantumWalkResultRepository(context);

        var result = await repo.GetTopAnomaliesAsync(1);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByNodeIdAsync_Should_Return_QftResult()
    {
        var context = GetDbContext();

        var node = new Node { IpAddress = "5.5.5.5" };

        context.Nodes.Add(node);
        await context.SaveChangesAsync();

        context.QftResults.Add(new QftResult
        {
            NodeId = node.NodeId,
            PeriodicityScore = 0.8
        });

        await context.SaveChangesAsync();

        var repo = new QftResultRepository(context);

        var result = await repo.GetByNodeIdAsync(node.NodeId);

        Assert.NotNull(result);
        Assert.Equal(node.NodeId, result!.NodeId);
    }

    [Fact]
    public async Task GetHighPeriodicityAsync_Should_Return_Filtered_Results()
    {
        var context = GetDbContext();

        var node = new Node
        {
            IpAddress = "5.5.5.5"
        };

        context.Nodes.Add(node);
        await context.SaveChangesAsync();

        context.QftResults.AddRange(
            new QftResult
            {
                NodeId = node.NodeId,
                PeriodicityScore = 0.95
            },
            new QftResult
            {
                NodeId = node.NodeId,
                PeriodicityScore = 0.2
            }
        );

        await context.SaveChangesAsync();

        var repo = new QftResultRepository(context);

        var result = await repo.GetHighPeriodicityAsync(0.5);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByThreatLevelAsync_Should_Return_Filtered_Results()
    {
        var context = GetDbContext();

        var node = new Node
        {
            IpAddress = "7.7.7.7"
        };

        context.Nodes.Add(node);
        await context.SaveChangesAsync();

        context.DetectionResults.AddRange(
            new DetectionResult
            {
                NodeId = node.NodeId,
                ThreatLevel = "High",
                DetectedAt = DateTime.UtcNow
            },
            new DetectionResult
            {
                NodeId = node.NodeId,
                ThreatLevel = "Low",
                DetectedAt = DateTime.UtcNow
            }
        );

        await context.SaveChangesAsync();

        var repo = new DetectionResultRepository(context);

        var result = await repo.GetByThreatLevelAsync("High");

        Assert.Single(result);
    }

    [Fact]
    public async Task GetLatestResultsAsync_Should_Return_Limited_Results()
    {
        var context = GetDbContext();

        var node = new Node
        {
            IpAddress = "8.8.8.8"
        };

        context.Nodes.Add(node);
        await context.SaveChangesAsync();

        context.DetectionResults.AddRange(
            new DetectionResult
            {
                NodeId = node.NodeId,
                ThreatLevel = "High",
                DetectedAt = DateTime.UtcNow
            },
            new DetectionResult
            {
                NodeId = node.NodeId,
                ThreatLevel = "Low",
                DetectedAt = DateTime.UtcNow.AddMinutes(-1)
            }
        );

        await context.SaveChangesAsync();

        var repo = new DetectionResultRepository(context);

        var result = await repo.GetLatestResultsAsync(1);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetThreatSummaryAsync_Should_Return_Grouped_Data()
    {
        var context = GetDbContext();

        var node = new Node { IpAddress = "7.7.7.7" };

        context.Nodes.Add(node);
        await context.SaveChangesAsync();

        context.DetectionResults.Add(new DetectionResult
        {
            NodeId = node.NodeId,
            ThreatLevel = "Critical",
            DetectedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var repo = new DetectionResultRepository(context);

        var result = await repo.GetThreatSummaryAsync();

        Assert.True(result.ContainsKey("Critical"));
    }

    [Fact]
    public async Task ClearAllAsync_Should_Remove_All_DetectionResults()
    {
        var context = GetDbContext();

        context.DetectionResults.Add(new DetectionResult
        {
            ThreatLevel = "Medium",
            DetectedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var repo = new DetectionResultRepository(context);

        await repo.ClearAllAsync();

        Assert.Empty(context.DetectionResults);
    }
}
using Microsoft.EntityFrameworkCore;
using network_project.Data;
using network_project.Interfaces;
using network_project.Models;

namespace network_project.Repository;

public class NodeRepository : BaseRepository<Node>, INodeRepository
{
    public NodeRepository(AppDbContext db) : base(db) { }

    public async Task<Node?> GetByIpAsync(string ipAddress)
        => await _db.Nodes.FirstOrDefaultAsync(n => n.IpAddress == ipAddress);

    public async Task<IEnumerable<Node>> GetHighAnomalyNodesAsync(double threshold)
        => await _db.Nodes
                    .Where(n => n.AnomalyScore >= threshold)
                    .OrderByDescending(n => n.AnomalyScore)
                    .ToListAsync();

    public async Task UpsertNodeAsync(string ipAddress)
    {
        var node = await _db.Nodes.FirstOrDefaultAsync(n => n.IpAddress == ipAddress);
        if (node is null)
        {
            await _db.Nodes.AddAsync(new Node { IpAddress = ipAddress, TotalConnections = 1 });
        }
        else
        {
            node.TotalConnections++;
            _db.Nodes.Update(node);
        }
    }
}

public class EdgeRepository : BaseRepository<Edge>, IEdgeRepository
{
    public EdgeRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Edge>> GetEdgesForIpAsync(string ipAddress)
        => await _db.Edges
                    .Where(e => e.SourceIp == ipAddress || e.DestIp == ipAddress)
                    .ToListAsync();

    public async Task UpsertEdgeAsync(string sourceIp, string destIp)
    {
        var edge = await _db.Edges
                            .FirstOrDefaultAsync(e => e.SourceIp == sourceIp && e.DestIp == destIp);
        if (edge is null)
        {
            await _db.Edges.AddAsync(new Edge { SourceIp = sourceIp, DestIp = destIp, Weight = 1 });
        }
        else
        {
            edge.Weight++;
            _db.Edges.Update(edge);
        }
    }
}

public class QuantumWalkResultRepository : BaseRepository<QuantumWalkResult>, IQuantumWalkResultRepository
{
    public QuantumWalkResultRepository(AppDbContext db) : base(db) { }

    public async Task<QuantumWalkResult?> GetByNodeIdAsync(int nodeId)
        => await _db.QuantumWalkResults
                    .Include(r => r.Node)
                    .FirstOrDefaultAsync(r => r.NodeId == nodeId);

    public async Task<IEnumerable<QuantumWalkResult>> GetTopAnomaliesAsync(int topN = 10)
        => await _db.QuantumWalkResults
                    .Include(r => r.Node)
                    .OrderByDescending(r => r.AnomalyScore)
                    .Take(topN)
                    .ToListAsync();
}

public class QftResultRepository : BaseRepository<QftResult>, IQftResultRepository
{
    public QftResultRepository(AppDbContext db) : base(db) { }

    public async Task<QftResult?> GetByNodeIdAsync(int nodeId)
        => await _db.QftResults
                    .Include(r => r.Node)
                    .FirstOrDefaultAsync(r => r.NodeId == nodeId);

    public async Task<IEnumerable<QftResult>> GetHighPeriodicityAsync(double threshold)
        => await _db.QftResults
                    .Include(r => r.Node)
                    .Where(r => r.PeriodicityScore >= threshold)
                    .OrderByDescending(r => r.PeriodicityScore)
                    .ToListAsync();
}

public class DetectionResultRepository : BaseRepository<DetectionResult>, IDetectionResultRepository
{
    public DetectionResultRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<DetectionResult>> GetByThreatLevelAsync(string threatLevel)
        => await _db.DetectionResults
                    .Include(r => r.Node)
                    .Where(r => r.ThreatLevel == threatLevel)
                    .OrderByDescending(r => r.DetectedAt)
                    .ToListAsync();

    public async Task<IEnumerable<DetectionResult>> GetLatestResultsAsync(int count = 50)
        => await _db.DetectionResults
                    .Include(r => r.Node)
                    .OrderByDescending(r => r.DetectedAt)
                    .Take(count)
                    .ToListAsync();

    public async Task<Dictionary<string, List<string>>> GetThreatSummaryAsync()
    {
        var results = await _db.DetectionResults
                               .Include(r => r.Node)
                               .ToListAsync();

        return results
            .GroupBy(r => r.ThreatLevel)
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => r.Node.IpAddress).Distinct().ToList());
    }
}

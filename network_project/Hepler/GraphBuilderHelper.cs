using network_project.Interfaces;
using network_project.Models;

namespace network_project.Helper;
public class GraphBuilderHelper
{
    private readonly INodeRepository _nodes;
    private readonly IEdgeRepository _edges;

    public GraphBuilderHelper(INodeRepository nodes, IEdgeRepository edges)
    {
        _nodes = nodes;
        _edges = edges;
    }

    public async Task BuildAsync(IEnumerable<NetworkLog> logs)
    {
        foreach (var log in logs)
        {
            await _nodes.UpsertNodeAsync(log.SourceIp);
            await _nodes.UpsertNodeAsync(log.DestIp);
            await _edges.UpsertEdgeAsync(log.SourceIp, log.DestIp);
        }

        await _nodes.SaveChangesAsync();
        await _edges.SaveChangesAsync();
    }

    public async Task<Dictionary<string, List<(string Neighbour, int Weight)>>> GetAdjacencyAsync()
    {
        var edges = await _edges.GetAllAsync();
        var adj   = new Dictionary<string, List<(string, int)>>();

        foreach (var e in edges)
        {
            if (!adj.ContainsKey(e.SourceIp)) adj[e.SourceIp] = [];
            if (!adj.ContainsKey(e.DestIp))   adj[e.DestIp]   = [];

            adj[e.SourceIp].Add((e.DestIp,   e.Weight));
            adj[e.DestIp  ].Add((e.SourceIp, e.Weight));
        }

        return adj;
    }
}
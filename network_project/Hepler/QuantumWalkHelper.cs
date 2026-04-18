using network_project.Interfaces;
using network_project.Models;

namespace network_project.Helper;

public class QuantumWalkHelper
{
    private readonly INodeRepository             _nodes;
    private readonly IQuantumWalkResultRepository _results;
    private const int Steps = 5;

    public QuantumWalkHelper(INodeRepository nodes, IQuantumWalkResultRepository results)
    {
        _nodes   = nodes;
        _results = results;
    }

    public async Task RunAsync(
        Dictionary<string, List<(string Neighbour, int Weight)>> adjacency)
    {
        var ips = adjacency.Keys.ToList();
        int n   = ips.Count;
        if (n == 0) return;

        double initAmplitude = 1.0 / Math.Sqrt(n);
        var amplitudes = ips.ToDictionary(ip => ip, _ => initAmplitude);

        for (int step = 0; step < Steps; step++)
        {
            var next = ips.ToDictionary(ip => ip, _ => 0.0);

            foreach (var ip in ips)
            {
                if (!adjacency.TryGetValue(ip, out var neighbours) || neighbours.Count == 0)
                {
                    next[ip] += amplitudes[ip];
                    continue;
                }

                double totalWeight = neighbours.Sum(nb => nb.Weight);
                foreach (var (neighbour, weight) in neighbours)
                {
                    double transferFraction = weight / totalWeight;
                    next[neighbour] += amplitudes[ip] * transferFraction;
                }
            }

            double norm = Math.Sqrt(next.Values.Sum(a => a * a));
            if (norm > 0)
                foreach (var key in next.Keys.ToList())
                    next[key] /= norm;

            amplitudes = next;
        }

        var probabilities = amplitudes.ToDictionary(kv => kv.Key, kv => kv.Value * kv.Value);
        double meanProb   = probabilities.Values.Average();
        double stdDev     = Math.Sqrt(probabilities.Values.Average(p => Math.Pow(p - meanProb, 2)));

        foreach (var ip in ips)
        {
            var node = await _nodes.GetByIpAsync(ip);
            if (node is null) continue;

            double prob    = probabilities[ip];
            double anomaly = stdDev > 0 ? Math.Abs(prob - meanProb) / stdDev : 0;
            anomaly = Math.Min(anomaly / 3.0, 1.0);
            node.AnomalyScore = anomaly;
            await _nodes.SaveChangesAsync();

            await _results.AddAsync(new QuantumWalkResult
            {
                NodeId           = node.NodeId,
                ProbabilityScore = prob,
                AnomalyScore     = anomaly
            });
        }

        await _results.SaveChangesAsync();
    }
}

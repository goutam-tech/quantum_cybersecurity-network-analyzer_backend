using Microsoft.Extensions.Configuration;
using network_project.Interfaces;
using network_project.Models;

namespace network_project.Helper;

public class ThreatScoringHelper
{
    private readonly INodeRepository _nodes;
    private readonly IQuantumWalkResultRepository _qwResults;
    private readonly IQftResultRepository _qftResults;
    private readonly IDetectionResultRepository _detections;

    private readonly double _qwWeight;
    private readonly double _qftWeight;
    private readonly double _anomalyThreshold;
    private readonly double _attackThreshold;

    public ThreatScoringHelper(
        INodeRepository nodes,
        IQuantumWalkResultRepository qwResults,
        IQftResultRepository qftResults,
        IDetectionResultRepository detections,
        IConfiguration config)
    {
        _nodes = nodes;
        _qwResults = qwResults;
        _qftResults = qftResults;
        _detections = detections;

        var section = config.GetSection("QuantumSettings");
        _qwWeight = section.GetValue<double>("QuantumWalkWeight", 0.6);
        _qftWeight = section.GetValue<double>("QftWeight", 0.4);
        _anomalyThreshold = section.GetValue<double>("AnomalyThreshold", 0.35);
        _attackThreshold = section.GetValue<double>("AttackThreshold", 0.65); 
    }

    public async Task<List<DetectionResult>> RunAsync()
    {
        var nodes = (await _nodes.GetAllAsync()).ToList();
        if (nodes.Count == 0) return [];

        var scoreMap = new List<(Node Node, double QwScore, double QftScore)>();

        foreach (var node in nodes)
        {
            var qw = await _qwResults.GetByNodeIdAsync(node.NodeId);
            var qft = await _qftResults.GetByNodeIdAsync(node.NodeId);

            double qwScore = qw?.AnomalyScore ?? 0;
            double qftScore = qft?.PeriodicityScore ?? 0;

            scoreMap.Add((node, qwScore, qftScore));
        }

        double maxQw = scoreMap.Max(s => s.QwScore);
        double maxQft = scoreMap.Max(s => s.QftScore);

        var created = new List<DetectionResult>();

        foreach (var (node, rawQw, rawQft) in scoreMap)
        {
            double normQw = maxQw > 0 ? rawQw / maxQw : 0;
            double normQft = maxQft > 0 ? rawQft / maxQft : 0;

            double threatScore = Math.Round((_qwWeight * normQw) + (_qftWeight * normQft), 4);

            string level;
            if (threatScore >= _attackThreshold)
                level = "Attack";
            else if (threatScore >= _anomalyThreshold)
                level = "Suspicious";
            else
                level = "Normal";

            var result = new DetectionResult
            {
                NodeId = node.NodeId,
                ThreatLevel = level,
                Confidence = threatScore,
                DetectedAt = DateTime.UtcNow
            };

            await _detections.AddAsync(result);
            created.Add(result);
        }

        await _detections.SaveChangesAsync();
        return created;
    }
}
using Microsoft.AspNetCore.Mvc;
using QuantumCyberAnalyzer.Dto;
using QuantumCyberAnalyzer.Helper;
using QuantumCyberAnalyzer.Interfaces;

namespace QuantumCyberAnalyzer.Controllers;

// ══════════════════════════════════════════════════════════════════════════════
// POST /analyze
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Runs the full analysis pipeline:
///   1. Quantum Walk  → QuantumWalkResults
///   2. QFT Analysis  → QFTResults
///   3. Threat Scoring → DetectionResults
/// </summary>
[ApiController]
[Route("[controller]")]
public class AnalyzeController : ControllerBase
{
    private readonly GraphBuilderHelper    _graphBuilder;
    private readonly QuantumWalkHelper     _qwHelper;
    private readonly QftAnalysisHelper     _qftHelper;
    private readonly ThreatScoringHelper   _scoringHelper;
    private readonly INodeRepository       _nodes;

    public AnalyzeController(
        GraphBuilderHelper graphBuilder,
        QuantumWalkHelper qwHelper,
        QftAnalysisHelper qftHelper,
        ThreatScoringHelper scoringHelper,
        INodeRepository nodes)
    {
        _graphBuilder  = graphBuilder;
        _qwHelper      = qwHelper;
        _qftHelper     = qftHelper;
        _scoringHelper = scoringHelper;
        _nodes         = nodes;
    }

    /// <summary>POST /analyze</summary>
    [HttpPost]
    public async Task<IActionResult> Analyze()
    {
        int nodeCount = (await _nodes.GetAllAsync()).Count();
        if (nodeCount == 0)
            return BadRequest(new { Message = "No data to analyze. Upload a CSV first." });

        // Step 1 – Quantum Walk
        var adjacency = await _graphBuilder.GetAdjacencyAsync();
        await _qwHelper.RunAsync(adjacency);

        // Step 2 – QFT
        await _qftHelper.RunAsync();

        // Step 3 – Threat Scoring
        var detections = await _scoringHelper.RunAsync();

        return Ok(new AnalysisResultDto(
            TotalNodesAnalyzed: nodeCount,
            ThreatsDetected:    detections.Count(d => d.ThreatLevel != "Normal"),
            Results:            detections.Select(MapDetection).ToList()));
    }

    private static DetectionResultDto MapDetection(Models.DetectionResult d) =>
        new(d.Id, d.NodeId, d.Node?.IpAddress ?? "unknown",
            d.ThreatLevel, d.Confidence, d.DetectedAt);
}

// ══════════════════════════════════════════════════════════════════════════════
// GET /results
// ══════════════════════════════════════════════════════════════════════════════

[ApiController]
[Route("[controller]")]
public class ResultsController : ControllerBase
{
    private readonly IDetectionResultRepository  _detections;
    private readonly IQuantumWalkResultRepository _qwResults;
    private readonly IQftResultRepository         _qftResults;

    public ResultsController(
        IDetectionResultRepository detections,
        IQuantumWalkResultRepository qwResults,
        IQftResultRepository qftResults)
    {
        _detections = detections;
        _qwResults  = qwResults;
        _qftResults = qftResults;
    }

    /// <summary>GET /results?count=50</summary>
    [HttpGet]
    public async Task<IActionResult> GetResults([FromQuery] int count = 50)
    {
        var results = await _detections.GetLatestResultsAsync(count);
        var dtos    = results.Select(MapDetection).ToList();
        return Ok(dtos);
    }

    /// <summary>GET /results/quantum-walk</summary>
    [HttpGet("quantum-walk")]
    public async Task<IActionResult> GetQuantumWalkResults([FromQuery] int top = 20)
    {
        var results = await _qwResults.GetTopAnomaliesAsync(top);
        var dtos = results.Select(r => new QuantumWalkResultDto(
            r.Id, r.NodeId, r.Node?.IpAddress ?? "",
            r.ProbabilityScore, r.AnomalyScore)).ToList();
        return Ok(dtos);
    }

    /// <summary>GET /results/qft</summary>
    [HttpGet("qft")]
    public async Task<IActionResult> GetQftResults([FromQuery] double threshold = 0.5)
    {
        var results = await _qftResults.GetHighPeriodicityAsync(threshold);
        var dtos = results.Select(r => new QftResultDto(
            r.Id, r.NodeId, r.Node?.IpAddress ?? "",
            r.DominantFrequency, r.PeriodicityScore)).ToList();
        return Ok(dtos);
    }

    private static DetectionResultDto MapDetection(Models.DetectionResult d) =>
        new(d.Id, d.NodeId, d.Node?.IpAddress ?? "unknown",
            d.ThreatLevel, d.Confidence, d.DetectedAt);
}

// ══════════════════════════════════════════════════════════════════════════════
// GET /threats
// ══════════════════════════════════════════════════════════════════════════════

[ApiController]
[Route("[controller]")]
public class ThreatsController : ControllerBase
{
    private readonly IDetectionResultRepository _detections;

    public ThreatsController(IDetectionResultRepository detections) =>
        _detections = detections;

    /// <summary>GET /threats  – grouped summary by threat level</summary>
    [HttpGet]
    public async Task<IActionResult> GetThreats()
    {
        var summary = await _detections.GetThreatSummaryAsync();
        var response = summary
            .Where(kv => kv.Key != "Normal")   // focus on actual threats
            .Select(kv => new ThreatSummaryDto(kv.Key, kv.Value.Count, kv.Value))
            .OrderByDescending(t => t.ThreatLevel == "Attack")
            .ToList();

        return Ok(response);
    }

    /// <summary>GET /threats/{level}  – e.g. /threats/Attack</summary>
    [HttpGet("{level}")]
    public async Task<IActionResult> GetByLevel(string level)
    {
        var results = await _detections.GetByThreatLevelAsync(level);
        return Ok(results.Select(d => new DetectionResultDto(
            d.Id, d.NodeId, d.Node?.IpAddress ?? "",
            d.ThreatLevel, d.Confidence, d.DetectedAt)));
    }
}

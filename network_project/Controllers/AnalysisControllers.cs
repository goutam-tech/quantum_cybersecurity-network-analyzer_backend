using Microsoft.AspNetCore.Mvc;
using network_project.Dto;
using network_project.Helper;
using network_project.Interfaces;

namespace network_project.Controllers;

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

    [HttpPost]
    public async Task<IActionResult> Analyze()
    {
        int nodeCount = (await _nodes.GetAllAsync()).Count();
        if (nodeCount == 0)
            return BadRequest(new { Message = "No data to analyze. Upload a CSV first." });

        var adjacency = await _graphBuilder.GetAdjacencyAsync();
        await _qwHelper.RunAsync(adjacency);

        await _qftHelper.RunAsync();

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

    [HttpGet]
    public async Task<IActionResult> GetResults([FromQuery] int count = 50)
    {
        var results = await _detections.GetLatestResultsAsync(count);
        var dtos    = results.Select(MapDetection).ToList();
        return Ok(dtos);
    }

    [HttpGet("quantum-walk")]
    public async Task<IActionResult> GetQuantumWalkResults([FromQuery] int top = 20)
    {
        var results = await _qwResults.GetTopAnomaliesAsync(top);
        var dtos = results.Select(r => new QuantumWalkResultDto(
            r.Id, r.NodeId, r.Node?.IpAddress ?? "",
            r.ProbabilityScore, r.AnomalyScore)).ToList();
        return Ok(dtos);
    }

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

[ApiController]
[Route("[controller]")]
public class ThreatsController : ControllerBase
{
    private readonly IDetectionResultRepository _detections;

    public ThreatsController(IDetectionResultRepository detections) =>
        _detections = detections;

    [HttpGet]
    public async Task<IActionResult> GetThreats()
    {
        var summary = await _detections.GetThreatSummaryAsync();
        var response = summary
            .Where(kv => kv.Key != "Normal")
            .Select(kv => new ThreatSummaryDto(kv.Key, kv.Value.Count, kv.Value))
            .OrderByDescending(t => t.ThreatLevel == "Attack")
            .ToList();

        return Ok(response);
    }

    [HttpGet("{level}")]
    public async Task<IActionResult> GetByLevel(string level)
    {
        var results = await _detections.GetByThreatLevelAsync(level);
        return Ok(results.Select(d => new DetectionResultDto(
            d.Id, d.NodeId, d.Node?.IpAddress ?? "",
            d.ThreatLevel, d.Confidence, d.DetectedAt)));
    }
}

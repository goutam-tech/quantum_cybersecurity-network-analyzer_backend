using Microsoft.AspNetCore.Mvc;
using QuantumCyberAnalyzer.Dto;
using QuantumCyberAnalyzer.Helper;
using QuantumCyberAnalyzer.Interfaces;

namespace QuantumCyberAnalyzer.Controllers;

/// <summary>
/// POST /upload  – accepts a CSV file, validates, parses, and stores logs.
///                 Then builds the graph (Nodes + Edges) automatically.
/// </summary>
[ApiController]
[Route("[controller]")]
public class UploadController : ControllerBase
{
    private readonly INetworkLogRepository _logRepo;
    private readonly CsvParserHelper       _csvParser;
    private readonly GraphBuilderHelper    _graphBuilder;

    public UploadController(
        INetworkLogRepository logRepo,
        CsvParserHelper csvParser,
        GraphBuilderHelper graphBuilder)
    {
        _logRepo      = logRepo;
        _csvParser    = csvParser;
        _graphBuilder = graphBuilder;
    }

    /// <summary>POST /upload</summary>
    [HttpPost]
    [RequestSizeLimit(50 * 1024 * 1024)]   // 50 MB max
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new UploadResponseDto(false, 0, "No file uploaded."));

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new UploadResponseDto(false, 0, "Only CSV files are accepted."));

        await using var stream = file.OpenReadStream();
        var (logs, error)      = _csvParser.Parse(stream);

        if (error is not null)
            return UnprocessableEntity(new UploadResponseDto(false, 0, error));

        if (logs.Count == 0)
            return BadRequest(new UploadResponseDto(false, 0, "CSV contained no valid records."));

        await _logRepo.AddRangeAsync(logs);
        await _logRepo.SaveChangesAsync();

        // Build / update graph from uploaded logs
        await _graphBuilder.BuildAsync(logs);

        return Ok(new UploadResponseDto(true, logs.Count,
            $"Uploaded {logs.Count} records and updated graph successfully."));
    }
}

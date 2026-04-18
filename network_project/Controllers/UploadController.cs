using Microsoft.AspNetCore.Mvc;
using network_project.Dto;
using network_project.Helper;
using network_project.Interfaces;

namespace network_project.Controllers;

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

    [HttpPost]
    [RequestSizeLimit(50 * 1024 * 1024)]
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

        await _graphBuilder.BuildAsync(logs);

        return Ok(new UploadResponseDto(true, logs.Count,
            $"Uploaded {logs.Count} records and updated graph successfully."));
    }
}

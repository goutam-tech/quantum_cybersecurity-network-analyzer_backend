using Microsoft.AspNetCore.Mvc;
using QuantumCyberAnalyzer.Dto;
using QuantumCyberAnalyzer.Interfaces;

namespace QuantumCyberAnalyzer.Controllers;

/// <summary>
/// GET /logs        – returns all network logs (paginated)
/// GET /logs/{id}   – returns a single log entry
/// </summary>
[ApiController]
[Route("[controller]")]
public class LogsController : ControllerBase
{
    private readonly INetworkLogRepository _repo;

    public LogsController(INetworkLogRepository repo) => _repo = repo;

    /// <summary>GET /logs?page=1&pageSize=100</summary>
    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var all = (await _repo.GetAllAsync()).ToList();

        var paged = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(Map)
            .ToList();

        return Ok(new
        {
            Total      = all.Count,
            Page       = page,
            PageSize   = pageSize,
            Records    = paged
        });
    }

    /// <summary>GET /logs/{id}</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetLog(int id)
    {
        var log = await _repo.GetByIdAsync(id);
        return log is null ? NotFound() : Ok(Map(log));
    }

    private static NetworkLogDto Map(Models.NetworkLog l) =>
        new(l.LogId, l.SourceIp, l.DestIp, l.Protocol, l.PacketSize, l.Timestamp);
}

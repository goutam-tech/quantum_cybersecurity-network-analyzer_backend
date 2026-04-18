using Microsoft.AspNetCore.Mvc;
using network_project.Dto;
using network_project.Interfaces;

namespace network_project.Controllers;

[ApiController]
[Route("[controller]")]
public class LogsController : ControllerBase
{
    private readonly INetworkLogRepository _repo;

    public LogsController(INetworkLogRepository repo) => _repo = repo;

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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetLog(int id)
    {
        var log = await _repo.GetByIdAsync(id);
        return log is null ? NotFound() : Ok(Map(log));
    }

    private static NetworkLogDto Map(Models.NetworkLog l) =>
        new(l.LogId, l.SourceIp, l.DestIp, l.Protocol, l.PacketSize, l.Timestamp);
}

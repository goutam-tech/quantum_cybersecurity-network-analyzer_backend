using Microsoft.EntityFrameworkCore;
using network_project.Data;
using network_project.Interfaces;
using network_project.Models;

namespace network_project.Repository;
public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _db;
    protected BaseRepository(AppDbContext db) => _db = db;

    public virtual async Task<IEnumerable<T>> GetAllAsync()
        => await _db.Set<T>().ToListAsync();

    public virtual async Task<T?> GetByIdAsync(int id)
        => await _db.Set<T>().FindAsync(id);

    public virtual async Task AddAsync(T entity)
        => await _db.Set<T>().AddAsync(entity);

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        => await _db.Set<T>().AddRangeAsync(entities);

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}

public class NetworkLogRepository : BaseRepository<NetworkLog>, INetworkLogRepository
{
    public NetworkLogRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<NetworkLog>> GetBySourceIpAsync(string ip)
        => await _db.NetworkLogs
                    .Where(l => l.SourceIp == ip)
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();

    public async Task<IEnumerable<NetworkLog>> GetByDateRangeAsync(DateTime from, DateTime to)
        => await _db.NetworkLogs
                    .Where(l => l.Timestamp >= from && l.Timestamp <= to)
                    .OrderBy(l => l.Timestamp)
                    .ToListAsync();

    public async Task<int> GetCountAsync()
        => await _db.NetworkLogs.CountAsync();
}
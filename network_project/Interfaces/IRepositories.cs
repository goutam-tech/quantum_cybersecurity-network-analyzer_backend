using network_project.Models;

namespace network_project.Interfaces;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task SaveChangesAsync();
}

public interface INetworkLogRepository : IRepository<NetworkLog>
{
    Task<IEnumerable<NetworkLog>> GetBySourceIpAsync(string ip);
    Task<IEnumerable<NetworkLog>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<int> GetCountAsync();
}

public interface INodeRepository : IRepository<Node>
{
    Task<Node?> GetByIpAsync(string ipAddress);
    Task<IEnumerable<Node>> GetHighAnomalyNodesAsync(double threshold);
    Task UpsertNodeAsync(string ipAddress);
}

public interface IEdgeRepository : IRepository<Edge>
{
    Task<IEnumerable<Edge>> GetEdgesForIpAsync(string ipAddress);
    Task UpsertEdgeAsync(string sourceIp, string destIp);
}

public interface IQuantumWalkResultRepository : IRepository<QuantumWalkResult>
{
    Task<QuantumWalkResult?> GetByNodeIdAsync(int nodeId);
    Task<IEnumerable<QuantumWalkResult>> GetTopAnomaliesAsync(int topN = 10);
}

public interface IQftResultRepository : IRepository<QftResult>
{
    Task<QftResult?> GetByNodeIdAsync(int nodeId);
    Task<IEnumerable<QftResult>> GetHighPeriodicityAsync(double threshold);
}

//public interface IDetectionResultRepository : IRepository<DetectionResult>
//{
//    Task<IEnumerable<DetectionResult>> GetByThreatLevelAsync(string threatLevel);
//    Task<IEnumerable<DetectionResult>> GetLatestResultsAsync(int count = 50);
//    Task<Dictionary<string, List<string>>> GetThreatSummaryAsync();
//}
public interface IDetectionResultRepository : IRepository<DetectionResult>
{
    Task<IEnumerable<DetectionResult>> GetByThreatLevelAsync(string threatLevel);
    Task<IEnumerable<DetectionResult>> GetLatestResultsAsync(int count = 50);
    Task<Dictionary<string, List<string>>> GetThreatSummaryAsync();
    Task ClearAllAsync();
}
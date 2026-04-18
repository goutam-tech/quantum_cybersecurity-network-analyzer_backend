using network_project.Models;

namespace network_project.Database
{
    //public class InMemoryDatabase
    //{
    //    public List<NetworkLog> NetworkLogs { get; set; }
    //    public List<NetworkNode> Nodes { get; set; }
    //    public List<NetworkEdge> Edges { get; set; }
    //    public List<QuantumWalkResult> QuantumWalkResults { get; set; }
    //    public List<QFTResult> QFTResults { get; set; }
    //    public List<DetectionResult> DetectionResults { get; set; }

    //    private int _logIdCounter = 1;
    //    private int _nodeIdCounter = 1;
    //    private int _edgeIdCounter = 1;

    //    public void InsertLog(NetworkLog log)
    //    {
    //        log.Id = _logIdCounter++;
    //        NetworkLogs.Add(log);
    //    }

    //    public NetworkNode? GetNodeByIp(string ip) =>
    //        Nodes.FirstOrDefault(n => n.IpAddress == ip);

    //    public NetworkNode UpsertNode(string ip)
    //    {
    //        var node = GetNodeByIp(ip);
    //        if (node == null)
    //        {
    //            node = new NetworkNode { Id = _nodeIdCounter++ };
    //            Nodes.Add(node);
    //        }
    //        else
    //        {
    //            node.TotalConnections++;
    //        }
    //        return node;
    //    }

    //    public void UpsertEdge(string sourceIp, string destIp)
    //    {
    //        var edge = Edges.FirstOrDefault(e => e.SourceIp == sourceIp && e.DestIp == destIp);
    //        if (edge == null)
    //        {
    //            edge = new NetworkEdge
    //            {
    //                Id = _edgeIdCounter++,
    //                SourceIp = sourceIp,
    //                DestIp = destIp,
    //                Weight = 1
    //            };
    //            Edges.Add(edge);
    //        }
    //        else
    //        {
    //            edge.Weight++;
    //        }
    //    }

    //    public void SaveQuantumWalkResultS(IEnumerable<QuantumWalkResult> results)
    //    {
    //        QuantumWalkResults.Clear();
    //        QuantumWalkResults.AddRange(results);
    //    }

    //    public void SaveQFTResults(IEnumerable<QFTResult> results)
    //    {
    //        QFTResults.Clear();
    //        QFTResults.AddRange(results);
    //    }

    //    public void SaveDetectionResults(IEnumerable<DetectionResult> results)
    //    {
    //        DetectionResults.Clear();
    //        DetectionResults.AddRange(results);
    //    }

    //    //public void ClearAll()
    //    //{
    //    //    NetworkLogs.Clear();
    //    //    Nodes.Clear();
    //    //    QuantumWalkResults.Clear();
    //    //    QFTResults.Clear();
    //    //    DetectionResults.Clear();
    //    //    _logIdCounter = _nodeIdCounter = _edgeIdCounter = 1;
    //    //}
    //    public void ClearAll()
    //    {
    //        NetworkLogs.Clear();
    //        Nodes.Clear();
    //        Edges.Clear();
    //        QuantumWalkResults.Clear();
    //        QFTResults.Clear();
    //        DetectionResults.Clear();
    //        _logIdCounter = _nodeIdCounter = _edgeIdCounter = 1;
    //    }
    //}

    public class InMemoryDatabase
    {
        // ── Tables ──────────────────────────────────────────────────────────
        public List<NetworkLog> NetworkLogs { get; } = new();
        public List<NetworkNode> Nodes { get; } = new();
        public List<NetworkEdge> Edges { get; } = new();
        public List<QuantumWalkResult> QuantumWalkResults { get; } = new();
        public List<QFTResult> QFTResults { get; } = new();
        public List<DetectionResult> DetectionResults { get; } = new();

        private int _logIdCounter = 1;
        private int _nodeIdCounter = 1;
        private int _edgeIdCounter = 1;

        // ── NetworkLogs ─────────────────────────────────────────────────────
        public void InsertLog(NetworkLog log)
        {
            log.Id = _logIdCounter++;
            NetworkLogs.Add(log);
        }

        // ── Nodes ────────────────────────────────────────────────────────────
        public NetworkNode? GetNodeByIp(string ip) =>
            Nodes.FirstOrDefault(n => n.IpAddress == ip);

        public NetworkNode UpsertNode(string ip)
        {
            var node = GetNodeByIp(ip);
            if (node == null)
            {
                node = new NetworkNode { Id = _nodeIdCounter++, IpAddress = ip, TotalConnections = 1 };
                Nodes.Add(node);
            }
            else
            {
                node.TotalConnections++;
            }
            return node;
        }

        // ── Edges ────────────────────────────────────────────────────────────
        public void UpsertEdge(string sourceIp, string destIp)
        {
            var edge = Edges.FirstOrDefault(e => e.SourceIp == sourceIp && e.DestIp == destIp);
            if (edge == null)
            {
                edge = new NetworkEdge
                {
                    Id = _edgeIdCounter++,
                    SourceIp = sourceIp,
                    DestIp = destIp,
                    Weight = 1
                };
                Edges.Add(edge);
            }
            else
            {
                edge.Weight++;
            }
        }

        // ── Results ──────────────────────────────────────────────────────────
        public void SaveQuantumWalkResults(IEnumerable<QuantumWalkResult> results)
        {
            QuantumWalkResults.Clear();
            QuantumWalkResults.AddRange(results);
        }

        public void SaveQFTResults(IEnumerable<QFTResult> results)
        {
            QFTResults.Clear();
            QFTResults.AddRange(results);
        }

        public void SaveDetectionResults(IEnumerable<DetectionResult> results)
        {
            DetectionResults.Clear();
            DetectionResults.AddRange(results);
        }

        public void ClearAll()
        {
            NetworkLogs.Clear();
            Nodes.Clear();
            Edges.Clear();
            QuantumWalkResults.Clear();
            QFTResults.Clear();
            DetectionResults.Clear();
            _logIdCounter = _nodeIdCounter = _edgeIdCounter = 1;
        }
    }
}

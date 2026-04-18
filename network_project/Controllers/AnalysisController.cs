using network_project.Database;
using network_project.Models;
using network_project.Services;
using QuantumCyberSecurity.Services;

namespace network_project.Api
{
    public class AnalysisController
    {
        private readonly InMemoryDatabase _db;
        private readonly CsvIngestionService _ingest;
        private readonly GraphService _graph;
        private readonly QuantumWalkService _walk;
        private readonly QFTService _qft;
        private readonly ThreatDetectionService _threat;

        public AnalysisController()
        {
            _db = new InMemoryDatabase();
            _ingest = new CsvIngestionService(_db);
            _graph = new GraphService(_db);
            _walk = new QuantumWalkService(_db, _graph);
            _qft = new QFTService(_db);
            _threat = new ThreatDetectionService(_db);
        }

        public ApiResponse<IngestionReport> Upload(string csvContent)
        {
            if (string.IsNullOrWhiteSpace(csvContent))
                return ApiResponse<IngestionReport>.Fail("CSV content is empty.");

            _db.ClearAll();
            var report = _ingest.IngestCsv(csvContent);

            return report.IsSuccess
                ? ApiResponse<IngestionReport>.Ok(report, $"Ingested {report.RowsInserted} rows.")
                : ApiResponse<IngestionReport>.Fail(string.Join("; ", report.Errors));
        }

        public ApiResponse<AnalysisSummary> Analyze(
            int walkSteps = 20,
            double walkDt = 0.1,
            int qftBucketMins = 5)
        {
            if (!_db.NetworkLogs.Any())
                return ApiResponse<AnalysisSummary>.Fail("No data loaded. Call /upload first.");

        
            var walkResults = _walk.Run(walkSteps, walkDt);

            var qftResults = _qft.Run(qftBucketMins);

            var detections = _threat.Run();

            var summary = new AnalysisSummary
            {
                TotalNodes = _db.Nodes.Count,
                TotalEdges = _db.Edges.Count,
                HighRiskCount = detections.Count(d => d.ThreatLevel == "HIGH"),
                MedRiskCount = detections.Count(d => d.ThreatLevel == "MEDIUM"),
                LowRiskCount = detections.Count(d => d.ThreatLevel == "LOW"),
                TopThreats = detections.Take(5).ToList()
            };

            return ApiResponse<AnalysisSummary>.Ok(summary, "Analysis complete.");
        }

        public ApiResponse<List<DetectionResult>> GetResults()
        {
            if (!_db.DetectionResults.Any())
                return ApiResponse<List<DetectionResult>>.Fail("No results available. Run /analyze first.");

            return ApiResponse<List<DetectionResult>>.Ok(
                _db.DetectionResults,
                $"{_db.DetectionResults.Count} results returned.");
        }
    }

    
    public class ApiResponse<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; } = "";
        public T? Data { get; init; }

        public static ApiResponse<T> Ok(T data, string msg = "") =>
            new() { Success = true, Message = msg, Data = data };
        public static ApiResponse<T> Fail(string msg) =>
            new() { Success = false, Message = msg };
    }

    public class AnalysisSummary
    {
        public int TotalNodes { get; set; }
        public int TotalEdges { get; set; }
        public int HighRiskCount { get; set; }
        public int MedRiskCount { get; set; }
        public int LowRiskCount { get; set; }
        public List<DetectionResult> TopThreats { get; set; } = new();
    }
}

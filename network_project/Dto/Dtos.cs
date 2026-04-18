namespace network_project.Dto;

public record UploadResponseDto(
    bool Success, 
    int RecordCount, 
    string Message
);

public record NetworkLogDto(
    int LogId,
    string SourceIp,
    string DestIp,
    string Protocol,
    int PacketSize,
    DateTime Timestamp
);

public record NodeDto(
    int NodeId,
    string IpAddress,
    int TotalConnections,
    double AnomalyScore
);

public record EdgeDto(
    int EdgeId,
    string SourceIp,
    string DestIp,
    int Weight
);

public record QuantumWalkResultDto(
    int Id,
    int NodeId,
    string IpAddress,
    double ProbabilityScore,
    double AnomalyScore
);

public record QftResultDto(
    int Id,
    int NodeId,
    string IpAddress,
    double DominantFrequency,
    double PeriodicityScore
);

public record DetectionResultDto(
    int Id,
    int NodeId,
    string IpAddress,
    string ThreatLevel,
    double Confidence,
    DateTime DetectedAt
);

public record AnalysisResultDto(
    int TotalNodesAnalyzed,
    int ThreatsDetected,
    List<DetectionResultDto> Results
);

public record ThreatSummaryDto(
    string ThreatLevel,
    int Count,
    List<string> AffectedIPs
);

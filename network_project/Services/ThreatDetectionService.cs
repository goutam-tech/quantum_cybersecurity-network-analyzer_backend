using network_project.Database;
using network_project.Models;

namespace network_project.Services
{
    public class ThreatDetectionService
    {
        private readonly InMemoryDatabase _db;

        private const double WeightWalk = 0.6;
        private const double WeightQFT = 0.4;

        private const double HighThreshold = 0.8;
        private const double MediumThreshold = 0.5;

        public ThreatDetectionService(InMemoryDatabase db) => _db = db;

        public List<DetectionResult> Run()
        {
            var walkResults = _db.QuantumWalkResults
                                 .ToDictionary(r => r.IpAddress);
            var qftResults = _db.QFTResults
                                 .ToDictionary(r => r.IpAddress);

            var results = new List<DetectionResult>();

            foreach (var node in _db.Nodes)
            {
                double walkScore = walkResults.TryGetValue(node.IpAddress, out var wr)
                                   ? wr.ProbabilityScore : 0;
                double qftScore = qftResults.TryGetValue(node.IpAddress, out var qr)
                                   ? qr.PeriodicityScore : 0;

                double threatScore = (walkScore * WeightWalk) + (qftScore * WeightQFT);
                threatScore = Math.Clamp(threatScore, 0, 1);

                string threatLevel = ClassifyThreat(threatScore);

                double confidence = ComputeConfidence(threatScore);

                results.Add(new DetectionResult
                {
                    NodeId = node.Id,
                    IpAddress = node.IpAddress,
                    TreadScore = threatScore,
                    ThreatLevel = threatLevel,
                    Confidence = confidence,
                    QuantumWalkScore = walkScore,
                    QFTScore = qftScore
                });
            }

            results.Sort((a, b) => b.TreadScore.CompareTo(a.TreadScore));

            _db.SaveDetectionResults(results);
            return results;
        }

        private static string ClassifyThreat(double score) => score switch
        {
            >= HighThreshold => "HIGH",
            >= MediumThreshold => "MEDIUM",
            _ => "LOW"
        };

        private static double ComputeConfidence(double score)
        {
            double distHigh = Math.Abs(score - HighThreshold);
            double distMedium = Math.Abs(score - MediumThreshold);
            double distEdge = score < 0.5 ? score : (1.0 - score);

            double minDist = Math.Min(distEdge, Math.Min(distHigh, distMedium));

            return Math.Clamp(minDist / 0.3, 0, 1);
        }
    }
}

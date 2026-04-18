namespace network_project.Models
{
    public class DetectionResult
    {
        public int NodeId { get; set; }
        public string IpAddress { get; set; } = "";
        public double TreadScore { get; set; }
        public string ThreatLevel { get; set; } = "";
        public double Confidence { get; set; }
        public double QuantumWalkScore { get; set; }
        public double QFTScore { get; set; }
    }
}

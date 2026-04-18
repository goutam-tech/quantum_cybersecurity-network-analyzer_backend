using System.Globalization;

namespace network_project.Models
{
    public class QuantumWalkResult
    {
        public int NodeId { get; set; }
        public string IpAddress { get; set; } = "";
        public double ProbabilityScore { get; set; }
    }
}

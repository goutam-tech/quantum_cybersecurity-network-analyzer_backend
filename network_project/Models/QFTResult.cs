namespace network_project.Models
{
    public class QFTResult
    {
        public int NodeId { get; set; }
        public string IpAddress { get; set; } = "";
        public double DomainFrequency { get; set; }
        public double PeriodicityScore { get; set; }
    }
}

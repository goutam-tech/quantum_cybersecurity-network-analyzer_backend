namespace network_project.Models
{
    public class NetworkEdge
    {
        public int Id { get; set; }
        public string SourceIp { get; set; } = "";
        public string DestIp { get; set; } = "";
        public int Weight { get; set; }
    }
}

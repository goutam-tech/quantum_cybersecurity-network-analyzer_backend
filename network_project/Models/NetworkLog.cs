namespace network_project.Models
{
    public class NetworkLog
    {
        public int Id { get; set; }
        public string SourceIp { get; set; } = "";
        public string DestIp { get; set; } = "";
        public string Protocol { get; set; } = "";
        public int PacketSize { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

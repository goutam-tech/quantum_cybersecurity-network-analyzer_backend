namespace network_project.Models
{
    public class NetworkNode
    {
        public int Id { get; set; }
        public string IpAddress { get; set; } = "";
        public int TotalConnections { get; set; }
    }
}

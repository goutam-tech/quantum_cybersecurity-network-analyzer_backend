using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace network_project.Models;

[Table("NetworkLogs")]
public class NetworkLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LogId { get; set; }

    [MaxLength(50)]
    public string SourceIp { get; set; } = string.Empty;

    [MaxLength(50)]
    public string DestIp { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Protocol { get; set; } = string.Empty;

    public int PacketSize { get; set; }
    public DateTime Timestamp { get; set; }
}

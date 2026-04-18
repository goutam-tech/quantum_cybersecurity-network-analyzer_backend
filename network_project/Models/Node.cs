using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace network_project.Models;

[Table("Nodes")]
public class Node
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int NodeId { get; set; }

    [MaxLength(50)]
    public string IpAddress { get; set; } = string.Empty;

    public int TotalConnections { get; set; }

    public double AnomalyScore { get; set; }

    public ICollection<QuantumWalkResult> QuantumWalkResults { get; set; } = new List<QuantumWalkResult>();
    public ICollection<QftResult> QftResults { get; set; } = new List<QftResult>();
    public ICollection<DetectionResult> DetectionResults { get; set; } = new List<DetectionResult>();
}

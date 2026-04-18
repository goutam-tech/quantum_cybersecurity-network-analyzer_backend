using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace network_project.Models;

[Table("Edges")]
public class Edge
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EdgeId { get; set; }

    [MaxLength(50)]
    public string SourceIp { get; set; } = string.Empty;

    [MaxLength(50)]
    public string DestIp { get; set; } = string.Empty;

    public int Weight { get; set; }
}

[Table("QuantumWalkResults")]
public class QuantumWalkResult
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int NodeId { get; set; }

    public double ProbabilityScore { get; set; }

    public double AnomalyScore { get; set; }

    [ForeignKey(nameof(NodeId))]
    public Node Node { get; set; } = null!;
}

[Table("QFTResults")]
public class QftResult
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int NodeId { get; set; }

    public double DominantFrequency { get; set; }

    public double PeriodicityScore { get; set; }

    [ForeignKey(nameof(NodeId))]
    public Node Node { get; set; } = null!;
}

[Table("DetectionResults")]
public class DetectionResult
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int NodeId { get; set; }

    [MaxLength(20)]
    public string ThreatLevel { get; set; } = "Normal";

    public double Confidence { get; set; }

    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(NodeId))]
    public Node Node { get; set; } = null!;
}

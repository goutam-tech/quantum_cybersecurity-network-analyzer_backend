using System.ComponentModel.DataAnnotations;
using network_project.Models;

namespace network_project.Tests.Models;

public class AnalysisModelsTests
{
    [Fact]
    public void Edge_Should_Set_Properties_Correctly()
    {
        var edge = new Edge
        {
            EdgeId = 1,
            SourceIp = "192.168.1.1",
            DestIp = "192.168.1.2",
            Weight = 5
        };

        Assert.Equal(1, edge.EdgeId);
        Assert.Equal("192.168.1.1", edge.SourceIp);
        Assert.Equal("192.168.1.2", edge.DestIp);
        Assert.Equal(5, edge.Weight);
    }

    [Fact]
    public void QuantumWalkResult_Should_Set_Properties_Correctly()
    {
        var node = new Node
        {
            NodeId = 1,
            IpAddress = "10.0.0.1"
        };

        var result = new QuantumWalkResult
        {
            Id = 1,
            NodeId = 1,
            ProbabilityScore = 0.85,
            AnomalyScore = 0.92,
            Node = node
        };

        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.NodeId);
        Assert.Equal(0.85, result.ProbabilityScore);
        Assert.Equal(0.92, result.AnomalyScore);
        Assert.Equal(node, result.Node);
    }

    [Fact]
    public void QftResult_Should_Set_Properties_Correctly()
    {
        var node = new Node
        {
            NodeId = 2,
            IpAddress = "10.0.0.2"
        };

        var result = new QftResult
        {
            Id = 1,
            NodeId = 2,
            DominantFrequency = 50.5,
            PeriodicityScore = 0.88,
            Node = node
        };

        Assert.Equal(1, result.Id);
        Assert.Equal(2, result.NodeId);
        Assert.Equal(50.5, result.DominantFrequency);
        Assert.Equal(0.88, result.PeriodicityScore);
        Assert.Equal(node, result.Node);
    }

    [Fact]
    public void DetectionResult_Should_Set_Properties_Correctly()
    {
        var node = new Node
        {
            NodeId = 3,
            IpAddress = "10.0.0.3"
        };

        var detectedAt = DateTime.UtcNow;

        var result = new DetectionResult
        {
            Id = 1,
            NodeId = 3,
            ThreatLevel = "High",
            Confidence = 0.97,
            DetectedAt = detectedAt,
            Node = node
        };

        Assert.Equal(1, result.Id);
        Assert.Equal(3, result.NodeId);
        Assert.Equal("High", result.ThreatLevel);
        Assert.Equal(0.97, result.Confidence);
        Assert.Equal(detectedAt, result.DetectedAt);
        Assert.Equal(node, result.Node);
    }

    [Fact]
    public void Edge_SourceIp_Should_Have_MaxLength_50()
    {
        var property = typeof(Edge).GetProperty(nameof(Edge.SourceIp));

        var attribute = (MaxLengthAttribute?)Attribute.GetCustomAttribute(
            property!,
            typeof(MaxLengthAttribute));

        Assert.NotNull(attribute);
        Assert.Equal(50, attribute!.Length);
    }

    [Fact]
    public void Edge_DestIp_Should_Have_MaxLength_50()
    {
        var property = typeof(Edge).GetProperty(nameof(Edge.DestIp));

        var attribute = (MaxLengthAttribute?)Attribute.GetCustomAttribute(
            property!,
            typeof(MaxLengthAttribute));

        Assert.NotNull(attribute);
        Assert.Equal(50, attribute!.Length);
    }

    [Fact]
    public void DetectionResult_ThreatLevel_Should_Have_MaxLength_20()
    {
        var property = typeof(DetectionResult)
            .GetProperty(nameof(DetectionResult.ThreatLevel));

        var attribute = (MaxLengthAttribute?)Attribute.GetCustomAttribute(
            property!,
            typeof(MaxLengthAttribute));

        Assert.NotNull(attribute);
        Assert.Equal(20, attribute!.Length);
    }
}
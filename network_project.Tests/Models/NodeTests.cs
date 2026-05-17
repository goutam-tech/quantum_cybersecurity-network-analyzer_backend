using System.ComponentModel.DataAnnotations;
using network_project.Models;

namespace network_project.Tests.Models;

public class NodeModelTests
{
    [Fact]
    public void Node_Should_Create_Instance_With_Default_Values()
    {
        var node = new Node();

        Assert.Equal(string.Empty, node.IpAddress);
        Assert.Equal(0, node.TotalConnections);
        Assert.Equal(0, node.AnomalyScore);

        Assert.NotNull(node.QuantumWalkResults);
        Assert.NotNull(node.QftResults);
        Assert.NotNull(node.DetectionResults);
    }

    [Fact]
    public void Node_Should_Set_Properties_Correctly()
    {
        var node = new Node
        {
            NodeId = 1,
            IpAddress = "192.168.1.1",
            TotalConnections = 10,
            AnomalyScore = 0.95
        };

        Assert.Equal(1, node.NodeId);
        Assert.Equal("192.168.1.1", node.IpAddress);
        Assert.Equal(10, node.TotalConnections);
        Assert.Equal(0.95, node.AnomalyScore);
    }

    [Fact]
    public void Node_Should_Initialize_Collections()
    {
        var node = new Node();

        Assert.Empty(node.QuantumWalkResults);
        Assert.Empty(node.QftResults);
        Assert.Empty(node.DetectionResults);
    }

    [Fact]
    public void IpAddress_Should_Have_MaxLength_50()
    {
        var property = typeof(Node)
            .GetProperty(nameof(Node.IpAddress));

        var attribute = (MaxLengthAttribute?)Attribute.GetCustomAttribute(
            property!,
            typeof(MaxLengthAttribute));

        Assert.NotNull(attribute);
        Assert.Equal(50, attribute!.Length);
    }

    [Fact]
    public void Node_Should_Allow_Adding_QuantumWalkResults()
    {
        var node = new Node();

        node.QuantumWalkResults.Add(new QuantumWalkResult
        {
            Id = 1,
            ProbabilityScore = 0.8,
            AnomalyScore = 0.9
        });

        Assert.Single(node.QuantumWalkResults);
    }

    [Fact]
    public void Node_Should_Allow_Adding_QftResults()
    {
        var node = new Node();

        node.QftResults.Add(new QftResult
        {
            Id = 1,
            DominantFrequency = 50,
            PeriodicityScore = 0.85
        });

        Assert.Single(node.QftResults);
    }

    [Fact]
    public void Node_Should_Allow_Adding_DetectionResults()
    {
        var node = new Node();

        node.DetectionResults.Add(new DetectionResult
        {
            Id = 1,
            ThreatLevel = "High",
            Confidence = 0.92
        });

        Assert.Single(node.DetectionResults);
    }
}
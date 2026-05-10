using System.ComponentModel.DataAnnotations;
using network_project.Models;

namespace network_project.Tests.Models;

public class NetworkLogModelTests
{
    [Fact]
    public void NetworkLog_Should_Create_Instance_With_Default_Values()
    {
        var log = new NetworkLog();

        Assert.Equal(string.Empty, log.SourceIp);
        Assert.Equal(string.Empty, log.DestIp);
        Assert.Equal(string.Empty, log.Protocol);
    }

    [Fact]
    public void NetworkLog_Should_Set_Properties_Correctly()
    {
        var timestamp = DateTime.UtcNow;

        var log = new NetworkLog
        {
            LogId = 1,
            SourceIp = "192.168.1.1",
            DestIp = "192.168.1.2",
            Protocol = "TCP",
            PacketSize = 512,
            Timestamp = timestamp
        };

        Assert.Equal(1, log.LogId);
        Assert.Equal("192.168.1.1", log.SourceIp);
        Assert.Equal("192.168.1.2", log.DestIp);
        Assert.Equal("TCP", log.Protocol);
        Assert.Equal(512, log.PacketSize);
        Assert.Equal(timestamp, log.Timestamp);
    }

    [Fact]
    public void SourceIp_Should_Have_MaxLength_50()
    {
        var property = typeof(NetworkLog)
            .GetProperty(nameof(NetworkLog.SourceIp));

        var attribute = (MaxLengthAttribute?)Attribute.GetCustomAttribute(
            property!,
            typeof(MaxLengthAttribute));

        Assert.NotNull(attribute);
        Assert.Equal(50, attribute!.Length);
    }

    [Fact]
    public void DestIp_Should_Have_MaxLength_50()
    {
        var property = typeof(NetworkLog)
            .GetProperty(nameof(NetworkLog.DestIp));

        var attribute = (MaxLengthAttribute?)Attribute.GetCustomAttribute(
            property!,
            typeof(MaxLengthAttribute));

        Assert.NotNull(attribute);
        Assert.Equal(50, attribute!.Length);
    }

    [Fact]
    public void Protocol_Should_Have_MaxLength_20()
    {
        var property = typeof(NetworkLog)
            .GetProperty(nameof(NetworkLog.Protocol));

        var attribute = (MaxLengthAttribute?)Attribute.GetCustomAttribute(
            property!,
            typeof(MaxLengthAttribute));

        Assert.NotNull(attribute);
        Assert.Equal(20, attribute!.Length);
    }
}
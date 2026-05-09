using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using network_project.Helper;
using network_project.Interfaces;
using network_project.Models;
using Xunit;

namespace network_project.Tests.Helper
{
    public class QftAnalysisHelperTests
    {
        private readonly Mock<INodeRepository> _nodeRepoMock;
        private readonly Mock<IQftResultRepository> _resultRepoMock;
        private readonly Mock<INetworkLogRepository> _logRepoMock;

        private readonly QftAnalysisHelper _helper;

        public QftAnalysisHelperTests()
        {
            _nodeRepoMock = new Mock<INodeRepository>();
            _resultRepoMock = new Mock<IQftResultRepository>();
            _logRepoMock = new Mock<INetworkLogRepository>();

            _helper = new QftAnalysisHelper(
                _nodeRepoMock.Object,
                _resultRepoMock.Object,
                _logRepoMock.Object
            );
        }

        [Fact]
        public async Task RunAsync_Should_Return_When_No_Logs_Exist()
        {
            _logRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<NetworkLog>());

            await _helper.RunAsync();

            _resultRepoMock.Verify(
                x => x.AddAsync(It.IsAny<QftResult>()),
                Times.Never);

            _resultRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task RunAsync_Should_Create_Qft_Results()
        {
            var node = new Node
            {
                NodeId = 1,
                IpAddress = "192.168.1.1"
            };

            var logs = new List<NetworkLog>
            {
                new NetworkLog
                {
                    SourceIp = "192.168.1.1",
                    DestIp = "192.168.1.2",
                    PacketSize = 100,
                    Timestamp = DateTime.UtcNow.AddSeconds(-30)
                },
                new NetworkLog
                {
                    SourceIp = "192.168.1.2",
                    DestIp = "192.168.1.1",
                    PacketSize = 200,
                    Timestamp = DateTime.UtcNow
                }
            };

            _logRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(logs);

            _nodeRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Node> { node });

            await _helper.RunAsync();

            _resultRepoMock.Verify(
                x => x.AddAsync(It.Is<QftResult>(r =>
                    r.NodeId == 1 &&
                    r.DominantFrequency >= 0 &&
                    r.PeriodicityScore >= 0 &&
                    r.PeriodicityScore <= 1
                )),
                Times.Once);

            _resultRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_Skip_Node_With_No_Logs()
        {
            var node = new Node
            {
                NodeId = 5,
                IpAddress = "10.10.10.10"
            };

            var logs = new List<NetworkLog>
            {
                new NetworkLog
                {
                    SourceIp = "1.1.1.1",
                    DestIp = "2.2.2.2",
                    PacketSize = 50,
                    Timestamp = DateTime.UtcNow
                }
            };

            _logRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(logs);

            _nodeRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Node> { node });

            await _helper.RunAsync();

            _resultRepoMock.Verify(
                x => x.AddAsync(It.IsAny<QftResult>()),
                Times.Never);

            _resultRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_Process_Multiple_Nodes()
        {
            var nodes = new List<Node>
            {
                new Node
                {
                    NodeId = 1,
                    IpAddress = "192.168.1.1"
                },
                new Node
                {
                    NodeId = 2,
                    IpAddress = "192.168.1.2"
                }
            };

            var logs = new List<NetworkLog>
            {
                new NetworkLog
                {
                    SourceIp = "192.168.1.1",
                    DestIp = "192.168.1.2",
                    PacketSize = 120,
                    Timestamp = DateTime.UtcNow.AddSeconds(-20)
                },
                new NetworkLog
                {
                    SourceIp = "192.168.1.2",
                    DestIp = "192.168.1.1",
                    PacketSize = 150,
                    Timestamp = DateTime.UtcNow
                }
            };

            _logRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(logs);

            _nodeRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(nodes);

            await _helper.RunAsync();

            _resultRepoMock.Verify(
                x => x.AddAsync(It.IsAny<QftResult>()),
                Times.Exactly(2));

            _resultRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_Handle_Same_Timestamps()
        {
            var now = DateTime.UtcNow;

            var node = new Node
            {
                NodeId = 1,
                IpAddress = "192.168.1.1"
            };

            var logs = new List<NetworkLog>
            {
                new NetworkLog
                {
                    SourceIp = "192.168.1.1",
                    DestIp = "192.168.1.2",
                    PacketSize = 100,
                    Timestamp = now
                },
                new NetworkLog
                {
                    SourceIp = "192.168.1.1",
                    DestIp = "192.168.1.3",
                    PacketSize = 150,
                    Timestamp = now
                }
            };

            _logRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(logs);

            _nodeRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Node> { node });

            await _helper.RunAsync();

            _resultRepoMock.Verify(
                x => x.AddAsync(It.IsAny<QftResult>()),
                Times.Once);

            _resultRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }
    }
}
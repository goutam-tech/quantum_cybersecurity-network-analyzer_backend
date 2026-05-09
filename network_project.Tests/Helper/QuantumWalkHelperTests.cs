using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using network_project.Helper;
using network_project.Interfaces;
using network_project.Models;
using Xunit;

namespace network_project.Tests.Helper
{
    public class QuantumWalkHelperTests
    {
        private readonly Mock<INodeRepository> _nodeRepoMock;
        private readonly Mock<IQuantumWalkResultRepository> _resultRepoMock;

        private readonly QuantumWalkHelper _helper;

        public QuantumWalkHelperTests()
        {
            _nodeRepoMock = new Mock<INodeRepository>();
            _resultRepoMock = new Mock<IQuantumWalkResultRepository>();

            _helper = new QuantumWalkHelper(
                _nodeRepoMock.Object,
                _resultRepoMock.Object
            );
        }

        [Fact]
        public async Task RunAsync_Should_Return_When_Graph_Is_Empty()
        {
            var adjacency = new Dictionary<string, List<(string, int)>>();

            await _helper.RunAsync(adjacency);

            _resultRepoMock.Verify(
                x => x.AddAsync(It.IsAny<QuantumWalkResult>()),
                Times.Never);

            _resultRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task RunAsync_Should_Process_Single_Node()
        {
            var node = new Node
            {
                NodeId = 1,
                IpAddress = "192.168.1.1"
            };

            var adjacency = new Dictionary<string, List<(string, int)>>
            {
                ["192.168.1.1"] = new()
            };

            _nodeRepoMock
                .Setup(x => x.GetByIpAsync("192.168.1.1"))
                .ReturnsAsync(node);

            await _helper.RunAsync(adjacency);

            _nodeRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);

            _resultRepoMock.Verify(
                x => x.AddAsync(It.Is<QuantumWalkResult>(r =>
                    r.NodeId == 1 &&
                    r.ProbabilityScore >= 0 &&
                    r.ProbabilityScore <= 1 &&
                    r.AnomalyScore >= 0 &&
                    r.AnomalyScore <= 1
                )),
                Times.Once);

            _resultRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_Process_Multiple_Nodes()
        {
            var node1 = new Node
            {
                NodeId = 1,
                IpAddress = "192.168.1.1"
            };

            var node2 = new Node
            {
                NodeId = 2,
                IpAddress = "192.168.1.2"
            };

            var adjacency = new Dictionary<string, List<(string, int)>>
            {
                ["192.168.1.1"] = new()
                {
                    ("192.168.1.2", 5)
                },

                ["192.168.1.2"] = new()
                {
                    ("192.168.1.1", 5)
                }
            };

            _nodeRepoMock
                .Setup(x => x.GetByIpAsync("192.168.1.1"))
                .ReturnsAsync(node1);

            _nodeRepoMock
                .Setup(x => x.GetByIpAsync("192.168.1.2"))
                .ReturnsAsync(node2);

            await _helper.RunAsync(adjacency);

            _resultRepoMock.Verify(
                x => x.AddAsync(It.IsAny<QuantumWalkResult>()),
                Times.Exactly(2));

            _nodeRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Exactly(2));

            _resultRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_Skip_When_Node_Not_Found()
        {
            var adjacency = new Dictionary<string, List<(string, int)>>
            {
                ["192.168.1.10"] = new()
        {
            ("192.168.1.20", 1)
        },

                ["192.168.1.20"] = new()
        {
            ("192.168.1.10", 1)
        }
            };

            _nodeRepoMock
                .Setup(x => x.GetByIpAsync("192.168.1.10"))
                .ReturnsAsync((Node?)null);

            _nodeRepoMock
                .Setup(x => x.GetByIpAsync("192.168.1.20"))
                .ReturnsAsync((Node?)null);

            await _helper.RunAsync(adjacency);

            _resultRepoMock.Verify(
                x => x.AddAsync(It.IsAny<QuantumWalkResult>()),
                Times.Never);

            _nodeRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);

            _resultRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_Handle_Isolated_Node()
        {
            var node = new Node
            {
                NodeId = 7,
                IpAddress = "10.0.0.1"
            };

            var adjacency = new Dictionary<string, List<(string, int)>>
            {
                ["10.0.0.1"] = new()
            };

            _nodeRepoMock
                .Setup(x => x.GetByIpAsync("10.0.0.1"))
                .ReturnsAsync(node);

            await _helper.RunAsync(adjacency);

            _resultRepoMock.Verify(
                x => x.AddAsync(It.Is<QuantumWalkResult>(r =>
                    r.NodeId == 7
                )),
                Times.Once);

            Assert.True(node.AnomalyScore >= 0);
            Assert.True(node.AnomalyScore <= 1);
        }

        [Fact]
        public async Task RunAsync_Should_Handle_Weighted_Graph()
        {
            var node1 = new Node
            {
                NodeId = 1,
                IpAddress = "A"
            };

            var node2 = new Node
            {
                NodeId = 2,
                IpAddress = "B"
            };

            var node3 = new Node
            {
                NodeId = 3,
                IpAddress = "C"
            };

            var adjacency = new Dictionary<string, List<(string, int)>>
            {
                ["A"] = new()
                {
                    ("B", 10),
                    ("C", 1)
                },

                ["B"] = new()
                {
                    ("A", 10)
                },

                ["C"] = new()
                {
                    ("A", 1)
                }
            };

            _nodeRepoMock.Setup(x => x.GetByIpAsync("A"))
                .ReturnsAsync(node1);

            _nodeRepoMock.Setup(x => x.GetByIpAsync("B"))
                .ReturnsAsync(node2);

            _nodeRepoMock.Setup(x => x.GetByIpAsync("C"))
                .ReturnsAsync(node3);

            await _helper.RunAsync(adjacency);

            _resultRepoMock.Verify(
                x => x.AddAsync(It.IsAny<QuantumWalkResult>()),
                Times.Exactly(3));

            _resultRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }
    }
}
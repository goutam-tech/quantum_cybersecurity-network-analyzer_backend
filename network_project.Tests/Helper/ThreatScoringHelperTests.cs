using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using network_project.Helper;
using network_project.Interfaces;
using network_project.Models;
using Xunit;

namespace network_project.Tests.Helper
{
    public class ThreatScoringHelperTests
    {
        private readonly Mock<INodeRepository> _nodeRepoMock;
        private readonly Mock<IQuantumWalkResultRepository> _qwRepoMock;
        private readonly Mock<IQftResultRepository> _qftRepoMock;
        private readonly Mock<IDetectionResultRepository> _detectionRepoMock;

        private readonly IConfiguration _configuration;
        private readonly ThreatScoringHelper _helper;

        public ThreatScoringHelperTests()
        {
            _nodeRepoMock = new Mock<INodeRepository>();
            _qwRepoMock = new Mock<IQuantumWalkResultRepository>();
            _qftRepoMock = new Mock<IQftResultRepository>();
            _detectionRepoMock = new Mock<IDetectionResultRepository>();

            var settings = new Dictionary<string, string?>
            {
                { "QuantumSettings:QuantumWalkWeight", "0.6" },
                { "QuantumSettings:QftWeight", "0.4" },
                { "QuantumSettings:AnomalyThreshold", "0.35" },
                { "QuantumSettings:AttackThreshold", "0.65" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            _helper = new ThreatScoringHelper(
                _nodeRepoMock.Object,
                _qwRepoMock.Object,
                _qftRepoMock.Object,
                _detectionRepoMock.Object,
                _configuration
            );
        }

        [Fact]
        public async Task RunAsync_Should_Return_Empty_When_No_Nodes()
        {
            _nodeRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Node>());

            var result = await _helper.RunAsync();

            Assert.Empty(result);

            _detectionRepoMock.Verify(
                x => x.AddAsync(It.IsAny<DetectionResult>()),
                Times.Never);

            _detectionRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task RunAsync_Should_Create_Normal_Detection()
        {
            var node = new Node
            {
                NodeId = 1,
                IpAddress = "192.168.1.1"
            };

            _nodeRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Node> { node });

            _qwRepoMock
                .Setup(x => x.GetByNodeIdAsync(1))
                .ReturnsAsync(new QuantumWalkResult
                {
                    NodeId = 1,
                    AnomalyScore = 0.1
                });

            _qftRepoMock
                .Setup(x => x.GetByNodeIdAsync(1))
                .ReturnsAsync(new QftResult
                {
                    NodeId = 1,
                    PeriodicityScore = 0.1
                });

            var results = await _helper.RunAsync();

            Assert.Single(results);

            Assert.Equal("Attack", results[0].ThreatLevel);

            _detectionRepoMock.Verify(
                x => x.AddAsync(It.IsAny<DetectionResult>()),
                Times.Once);

            _detectionRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_Create_Suspicious_Detection()
        {
            var nodes = new List<Node>
            {
                new Node { NodeId = 1, IpAddress = "A" },
                new Node { NodeId = 2, IpAddress = "B" }
            };

            _nodeRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(nodes);

            _qwRepoMock.Setup(x => x.GetByNodeIdAsync(1))
                .ReturnsAsync(new QuantumWalkResult
                {
                    AnomalyScore = 0.4
                });

            _qwRepoMock.Setup(x => x.GetByNodeIdAsync(2))
                .ReturnsAsync(new QuantumWalkResult
                {
                    AnomalyScore = 1.0
                });

            _qftRepoMock.Setup(x => x.GetByNodeIdAsync(1))
                .ReturnsAsync(new QftResult
                {
                    PeriodicityScore = 0.4
                });

            _qftRepoMock.Setup(x => x.GetByNodeIdAsync(2))
                .ReturnsAsync(new QftResult
                {
                    PeriodicityScore = 1.0
                });

            var results = await _helper.RunAsync();

            Assert.Equal(2, results.Count);

            Assert.Contains(results,
                r => r.ThreatLevel == "Suspicious" ||
                     r.ThreatLevel == "Attack");
        }

        [Fact]
        public async Task RunAsync_Should_Create_Attack_Detection()
        {
            var node = new Node
            {
                NodeId = 99,
                IpAddress = "10.0.0.1"
            };

            _nodeRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Node> { node });

            _qwRepoMock
                .Setup(x => x.GetByNodeIdAsync(99))
                .ReturnsAsync(new QuantumWalkResult
                {
                    AnomalyScore = 1.0
                });

            _qftRepoMock
                .Setup(x => x.GetByNodeIdAsync(99))
                .ReturnsAsync(new QftResult
                {
                    PeriodicityScore = 1.0
                });

            var results = await _helper.RunAsync();

            Assert.Single(results);

            var detection = results.First();

            Assert.Equal("Attack", detection.ThreatLevel);
            Assert.True(detection.Confidence >= 0.65);

            _detectionRepoMock.Verify(
                x => x.AddAsync(It.IsAny<DetectionResult>()),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_Should_Handle_Missing_Qw_And_Qft_Results()
        {
            var node = new Node
            {
                NodeId = 50,
                IpAddress = "172.16.0.1"
            };

            _nodeRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Node> { node });

            _qwRepoMock
                .Setup(x => x.GetByNodeIdAsync(50))
                .ReturnsAsync((QuantumWalkResult?)null);

            _qftRepoMock
                .Setup(x => x.GetByNodeIdAsync(50))
                .ReturnsAsync((QftResult?)null);

            var results = await _helper.RunAsync();

            Assert.Single(results);

            Assert.Equal("Normal", results[0].ThreatLevel);
            Assert.Equal(0, results[0].Confidence);
        }

        [Fact]
        public async Task RunAsync_Should_Save_Changes_Once()
        {
            var node = new Node
            {
                NodeId = 1,
                IpAddress = "127.0.0.1"
            };

            _nodeRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Node> { node });

            _qwRepoMock
                .Setup(x => x.GetByNodeIdAsync(1))
                .ReturnsAsync(new QuantumWalkResult
                {
                    AnomalyScore = 0.5
                });

            _qftRepoMock
                .Setup(x => x.GetByNodeIdAsync(1))
                .ReturnsAsync(new QftResult
                {
                    PeriodicityScore = 0.5
                });

            await _helper.RunAsync();

            _detectionRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }
    }
}
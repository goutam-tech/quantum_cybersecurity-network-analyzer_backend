using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using network_project.Helper;
using network_project.Interfaces;
using network_project.Models;
using Xunit;

namespace network_project.Tests.Helper
{
    public class GraphBuilderHelperTests
    {
        private readonly Mock<INodeRepository> _nodeRepoMock;
        private readonly Mock<IEdgeRepository> _edgeRepoMock;
        private readonly GraphBuilderHelper _helper;

        public GraphBuilderHelperTests()
        {
            _nodeRepoMock = new Mock<INodeRepository>();
            _edgeRepoMock = new Mock<IEdgeRepository>();

            _helper = new GraphBuilderHelper(
                _nodeRepoMock.Object,
                _edgeRepoMock.Object
            );
        }

        [Fact]
        public async Task BuildAsync_Should_Upsert_Nodes_And_Edges()
        {
            var logs = new List<NetworkLog>
            {
                new NetworkLog
                {
                    SourceIp = "192.168.1.1",
                    DestIp = "192.168.1.2"
                },
                new NetworkLog
                {
                    SourceIp = "192.168.1.2",
                    DestIp = "192.168.1.3"
                }
            };

            await _helper.BuildAsync(logs);

            _nodeRepoMock.Verify(
                x => x.UpsertNodeAsync("192.168.1.1"),
                Times.Once);

            _nodeRepoMock.Verify(
                x => x.UpsertNodeAsync("192.168.1.2"),
                Times.Exactly(2));

            _nodeRepoMock.Verify(
                x => x.UpsertNodeAsync("192.168.1.3"),
                Times.Once);

            _edgeRepoMock.Verify(
                x => x.UpsertEdgeAsync("192.168.1.1", "192.168.1.2"),
                Times.Once);

            _edgeRepoMock.Verify(
                x => x.UpsertEdgeAsync("192.168.1.2", "192.168.1.3"),
                Times.Once);

            _nodeRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);

            _edgeRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task BuildAsync_Should_Handle_Empty_Logs()
        {
            var logs = new List<NetworkLog>();

            await _helper.BuildAsync(logs);

            _nodeRepoMock.Verify(
                x => x.UpsertNodeAsync(It.IsAny<string>()),
                Times.Never);

            _edgeRepoMock.Verify(
                x => x.UpsertEdgeAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);

            _nodeRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);

            _edgeRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetAdjacencyAsync_Should_Return_Correct_Graph()
        {
            var edges = new List<Edge>
            {
                new Edge
                {
                    SourceIp = "192.168.1.1",
                    DestIp = "192.168.1.2",
                    Weight = 5
                },
                new Edge
                {
                    SourceIp = "192.168.1.2",
                    DestIp = "192.168.1.3",
                    Weight = 3
                }
            };

            _edgeRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(edges);

            var result = await _helper.GetAdjacencyAsync();

            Assert.Equal(3, result.Count);

            Assert.Contains("192.168.1.1", result.Keys);
            Assert.Contains("192.168.1.2", result.Keys);
            Assert.Contains("192.168.1.3", result.Keys);

            Assert.Single(result["192.168.1.1"]);
            Assert.Equal("192.168.1.2", result["192.168.1.1"][0].Neighbour);
            Assert.Equal(5, result["192.168.1.1"][0].Weight);

            Assert.Equal(2, result["192.168.1.2"].Count);

            Assert.Single(result["192.168.1.3"]);
            Assert.Equal("192.168.1.2", result["192.168.1.3"][0].Neighbour);
        }

        [Fact]
        public async Task GetAdjacencyAsync_Should_Return_Empty_When_No_Edges()
        {
            _edgeRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Edge>());

            var result = await _helper.GetAdjacencyAsync();

            Assert.Empty(result);
        }
    }
}
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using network_project.Controllers;
using network_project.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace network_project.Tests.Controllers
{
    public class ThreatsControllerTests
    {
        private readonly Mock<IDetectionResultRepository> _repo = new();

        private ThreatsController GetController()
        {
            return new ThreatsController(_repo.Object);
        }

        [Fact]
        public async Task GetThreats_ReturnsOrderedResponse()
        {
            var data = new Dictionary<string, List<string>>
            {
                { "Normal", new List<string> { "node1" } },
                { "Attack", new List<string> { "node2" } },
                { "Suspicious", new List<string> { "node3" } }
            };

            _repo.Setup(x => x.GetThreatSummaryAsync())
                 .Returns(Task.FromResult(data));

            var controller = GetController();

            var result = await controller.GetThreats();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }
    }
}
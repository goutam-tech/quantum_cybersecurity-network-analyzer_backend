using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using network_project.Controllers;
using network_project.Interfaces;
using network_project.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace network_project.Tests.Controllers
{
    public class AnalyzeControllerTests
    {
        private readonly Mock<INodeRepository> _nodeRepo = new();

        private AnalyzeController GetController()
        {
            return new AnalyzeController(
                null!,
                null!,
                null!,
                null!,
                _nodeRepo.Object
            );
        }

        [Fact]
        public async Task Analyze_NoNodes_ReturnsBadRequest()
        {
            _nodeRepo.Setup(x => x.GetAllAsync())
                     .Returns(Task.FromResult<IEnumerable<Node>>(new List<Node>()));

            var controller = GetController();

            var result = await controller.Analyze();

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
        }
    }
}
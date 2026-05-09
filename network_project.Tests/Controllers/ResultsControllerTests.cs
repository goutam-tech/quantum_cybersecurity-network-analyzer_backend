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
    public class ResultsControllerTests
    {
        private readonly Mock<IDetectionResultRepository> _detections = new();
        private readonly Mock<IQuantumWalkResultRepository> _qw = new();
        private readonly Mock<IQftResultRepository> _qft = new();

        private ResultsController GetController()
        {
            return new ResultsController(
                _detections.Object,
                _qw.Object,
                _qft.Object
            );
        }

        [Fact]
        public async Task GetResults_ReturnsOk()
        {
            var list = new List<DetectionResult>
            {
                new DetectionResult()
            };

            _detections.Setup(x => x.GetLatestResultsAsync(50))
                       .ReturnsAsync(list);

            var controller = GetController();

            var result = await controller.GetResults();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetQuantumWalkResults_ReturnsOk()
        {
            var list = new List<QuantumWalkResult>
            {
                new QuantumWalkResult()
            };

            _qw.Setup(x => x.GetTopAnomaliesAsync(20))
               .ReturnsAsync(list);

            var controller = GetController();

            var result = await controller.GetQuantumWalkResults();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetQftResults_ReturnsOk()
        {
            var list = new List<QftResult>
            {
                new QftResult()
            };

            _qft.Setup(x => x.GetHighPeriodicityAsync(It.IsAny<double>()))
                .ReturnsAsync(list);

            var controller = GetController();

            var result = await controller.GetQftResults(0.1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetQftResults_EmptyFallback_ReturnsOk()
        {
            _qft.SetupSequence(x => x.GetHighPeriodicityAsync(It.IsAny<double>()))
                .ReturnsAsync(new List<QftResult>())        // first empty
                .ReturnsAsync(new List<QftResult> { new QftResult() }); // fallback

            var controller = GetController();

            var result = await controller.GetQftResults(0.1);

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
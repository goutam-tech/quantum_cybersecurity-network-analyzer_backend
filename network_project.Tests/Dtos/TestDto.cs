using System;
using System.Collections.Generic;
using network_project.Dto;
using Xunit;

namespace network_project.Tests.Dto
{
    public class AnalysisDtoTests
    {
        [Fact]
        public void UploadResponseDto_Should_Create_Correctly()
        {
            var dto = new UploadResponseDto(
                true,
                150,
                "Upload successful"
            );

            Assert.True(dto.Success);
            Assert.Equal(150, dto.RecordCount);
            Assert.Equal("Upload successful", dto.Message);
        }

        [Fact]
        public void NetworkLogDto_Should_Create_Correctly()
        {
            var timestamp = DateTime.UtcNow;

            var dto = new NetworkLogDto(
                1,
                "192.168.1.1",
                "192.168.1.2",
                "TCP",
                512,
                timestamp
            );

            Assert.Equal(1, dto.LogId);
            Assert.Equal("192.168.1.1", dto.SourceIp);
            Assert.Equal("192.168.1.2", dto.DestIp);
            Assert.Equal("TCP", dto.Protocol);
            Assert.Equal(512, dto.PacketSize);
            Assert.Equal(timestamp, dto.Timestamp);
        }

        [Fact]
        public void NodeDto_Should_Create_Correctly()
        {
            var dto = new NodeDto(
                10,
                "10.0.0.1",
                25,
                0.87
            );

            Assert.Equal(10, dto.NodeId);
            Assert.Equal("10.0.0.1", dto.IpAddress);
            Assert.Equal(25, dto.TotalConnections);
            Assert.Equal(0.87, dto.AnomalyScore);
        }

        [Fact]
        public void EdgeDto_Should_Create_Correctly()
        {
            var dto = new EdgeDto(
                100,
                "192.168.1.1",
                "192.168.1.5",
                12
            );

            Assert.Equal(100, dto.EdgeId);
            Assert.Equal("192.168.1.1", dto.SourceIp);
            Assert.Equal("192.168.1.5", dto.DestIp);
            Assert.Equal(12, dto.Weight);
        }

        [Fact]
        public void QuantumWalkResultDto_Should_Create_Correctly()
        {
            var dto = new QuantumWalkResultDto(
                1,
                5,
                "172.16.0.1",
                0.92,
                0.81
            );

            Assert.Equal(1, dto.Id);
            Assert.Equal(5, dto.NodeId);
            Assert.Equal("172.16.0.1", dto.IpAddress);
            Assert.Equal(0.92, dto.ProbabilityScore);
            Assert.Equal(0.81, dto.AnomalyScore);
        }

        [Fact]
        public void QftResultDto_Should_Create_Correctly()
        {
            var dto = new QftResultDto(
                2,
                7,
                "10.10.10.10",
                3.14,
                0.75
            );

            Assert.Equal(2, dto.Id);
            Assert.Equal(7, dto.NodeId);
            Assert.Equal("10.10.10.10", dto.IpAddress);
            Assert.Equal(3.14, dto.DominantFrequency);
            Assert.Equal(0.75, dto.PeriodicityScore);
        }

        [Fact]
        public void DetectionResultDto_Should_Create_Correctly()
        {
            var detectedAt = DateTime.UtcNow;

            var dto = new DetectionResultDto(
                1,
                20,
                "8.8.8.8",
                "High",
                0.98,
                detectedAt
            );

            Assert.Equal(1, dto.Id);
            Assert.Equal(20, dto.NodeId);
            Assert.Equal("8.8.8.8", dto.IpAddress);
            Assert.Equal("High", dto.ThreatLevel);
            Assert.Equal(0.98, dto.Confidence);
            Assert.Equal(detectedAt, dto.DetectedAt);
        }

        [Fact]
        public void AnalysisResultDto_Should_Create_Correctly()
        {
            var results = new List<DetectionResultDto>
            {
                new DetectionResultDto(
                    1,
                    2,
                    "192.168.1.100",
                    "Medium",
                    0.76,
                    DateTime.UtcNow
                )
            };

            var dto = new AnalysisResultDto(
                50,
                3,
                results
            );

            Assert.Equal(50, dto.TotalNodesAnalyzed);
            Assert.Equal(3, dto.ThreatsDetected);
            Assert.Single(dto.Results);
            Assert.Equal(results, dto.Results);
        }

        [Fact]
        public void ThreatSummaryDto_Should_Create_Correctly()
        {
            var affectedIps = new List<string>
            {
                "192.168.1.10",
                "192.168.1.11"
            };

            var dto = new ThreatSummaryDto(
                "Critical",
                2,
                affectedIps
            );

            Assert.Equal("Critical", dto.ThreatLevel);
            Assert.Equal(2, dto.Count);
            Assert.Equal(2, dto.AffectedIPs.Count);
            Assert.Contains("192.168.1.10", dto.AffectedIPs);
            Assert.Contains("192.168.1.11", dto.AffectedIPs);
        }
    }
}
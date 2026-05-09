using System;
using System.IO;
using System.Linq;
using System.Text;
using network_project.Helper;
using Xunit;

namespace network_project.Tests.Helper
{
    public class CsvParserHelperTests
    {
        private readonly CsvParserHelper _helper;

        public CsvParserHelperTests()
        {
            _helper = new CsvParserHelper();
        }

        private Stream GenerateStream(string content)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        [Fact]
        public void Parse_Should_Return_Logs_When_Csv_Is_Valid()
        {
            var csv =
@"sourceip,destip,protocol,packetsize,timestamp
192.168.1.1,192.168.1.2,TCP,512,01-05-2026 10:30";

            using var stream = GenerateStream(csv);

            var (logs, error) = _helper.Parse(stream);

            Assert.Null(error);
            Assert.Single(logs);

            var log = logs.First();

            Assert.Equal("192.168.1.1", log.SourceIp);
            Assert.Equal("192.168.1.2", log.DestIp);
            Assert.Equal("TCP", log.Protocol);
            Assert.Equal(512, log.PacketSize);
        }

        [Fact]
        public void Parse_Should_Return_Error_When_Csv_Is_Empty()
        {
            using var stream = GenerateStream("");

            var (logs, error) = _helper.Parse(stream);

            Assert.Empty(logs);
            Assert.Equal("CSV file is empty.", error);
        }

        [Fact]
        public void Parse_Should_Return_Error_When_Headers_Are_Missing()
        {
            var csv =
@"sourceip,destip,protocol
192.168.1.1,192.168.1.2,TCP";

            using var stream = GenerateStream(csv);

            var (logs, error) = _helper.Parse(stream);

            Assert.Empty(logs);
            Assert.NotNull(error);
            Assert.Contains("Missing columns", error);
        }

        [Fact]
        public void Parse_Should_Skip_Row_When_PacketSize_Is_Invalid()
        {
            var csv =
@"sourceip,destip,protocol,packetsize,timestamp
192.168.1.1,192.168.1.2,TCP,INVALID,01-05-2026 10:30";

            using var stream = GenerateStream(csv);

            var (logs, error) = _helper.Parse(stream);

            Assert.Null(error);
            Assert.Empty(logs);
        }

        [Fact]
        public void Parse_Should_Use_CurrentTime_When_Timestamp_Is_Invalid()
        {
            var csv =
@"sourceip,destip,protocol,packetsize,timestamp
192.168.1.1,192.168.1.2,TCP,256,INVALID_DATE";

            using var stream = GenerateStream(csv);

            var before = DateTime.UtcNow;

            var (logs, error) = _helper.Parse(stream);

            var after = DateTime.UtcNow;

            Assert.Null(error);
            Assert.Single(logs);

            var timestamp = logs.First().Timestamp;

            Assert.True(timestamp >= before.AddSeconds(-1));
            Assert.True(timestamp <= after.AddSeconds(1));
        }

        [Fact]
        public void Parse_Should_Ignore_Empty_Rows()
        {
            var csv =
@"sourceip,destip,protocol,packetsize,timestamp

192.168.1.1,192.168.1.2,TCP,128,01-05-2026 10:30

";

            using var stream = GenerateStream(csv);

            var (logs, error) = _helper.Parse(stream);

            Assert.Null(error);
            Assert.Single(logs);
        }

        [Fact]
        public void Parse_Should_Parse_Multiple_Records()
        {
            var csv =
@"sourceip,destip,protocol,packetsize,timestamp
192.168.1.1,192.168.1.2,TCP,100,01-05-2026 10:30
192.168.1.3,192.168.1.4,UDP,200,01-05-2026 11:00";

            using var stream = GenerateStream(csv);

            var (logs, error) = _helper.Parse(stream);

            Assert.Null(error);
            Assert.Equal(2, logs.Count);

            Assert.Equal("TCP", logs[0].Protocol);
            Assert.Equal("UDP", logs[1].Protocol);
        }

        [Fact]
        public void Parse_Should_Handle_Semicolon_Separated_Values()
        {
            var csv =
@"sourceip,destip,protocol,packetsize,timestamp
192.168.1.1;192.168.1.2;TCP;300;01-05-2026 10:30";

            using var stream = GenerateStream(csv);

            var (logs, error) = _helper.Parse(stream);

            Assert.Null(error);
            Assert.Single(logs);

            Assert.Equal(300, logs[0].PacketSize);
        }
    }
}
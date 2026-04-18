using network_project.Database;
using network_project.Models;
using System.Net;

namespace network_project.Services
{
    public class CsvIngestionService
    {
        private readonly InMemoryDatabase _db;
        public CsvIngestionService(InMemoryDatabase db)
        {
            _db = db;
        }

        public IngestionReport IngestCsv(string csvContent)
        {
            var report = new IngestionReport();
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if(lines.Length < 2)
            {
                report.Errors.Add("CSV has no data rows");
                return report;
            }

            var headers = ParseLine(lines[0]);
            int scrIdx = IndexOf(headers, "source_ip");
            int dstIdx = IndexOf(headers, "dest_ip");
            int proIdx = IndexOf(headers, "protocol");
            int pktIdx = IndexOf(headers, "packet_size");
            int tsIdx = IndexOf(headers, "timestamp");

            if(scrIdx < 0 || dstIdx < 0 || proIdx < 0 || pktIdx < 0 || tsIdx < 0)
            {
                report.Errors.Add("Missing required colums: source_ip, dest_ip, protocol, packet_size, timestamp");
                return report;
            }

            var seen = new HashSet<string>();

            for(int i = 1; i < lines.Length; i++)
            {
                var fileds = ParseLine(lines[i]);
                if(fileds.Length <= Math.Max(scrIdx, Math.Max(dstIdx, Math.Max(proIdx, Math.Max(pktIdx, tsIdx)))))
                {
                    report.SkippedRows++;
                    report.Errors.Add($"Row {i}: insufficient columns.");
                    continue;
                }

                string sourceIp = fileds[scrIdx].Trim();
                string destIp = fileds[dstIdx].Trim();
                string protocol = fileds[proIdx].Trim();
                string packetStr = fileds[pktIdx].Trim();
                string tsStr = fileds[tsIdx].Trim();

                var errors = Validate(sourceIp, destIp, packetStr, tsStr, i);
                if (errors.Count > 0)
                {
                    report.Errors.AddRange(errors);
                    report.SkippedRows++;
                    continue;
                }

                int packetSize = int.Parse(packetStr);
                DateTime ts = DateTime.Parse(tsStr);

                string key = $"{sourceIp}|{destIp}|{ts:O}";
                if (!seen.Add(key))
                {
                    report.DuplicatesRemoved++;
                    continue;
                }

                _db.InsertLog(new NetworkLog
                {
                    SourceIp = sourceIp,
                    DestIp = destIp,
                    Protocol = protocol,
                    PacketSize = packetSize,
                    Timestamp = ts
                });

                report.RowsInserted++;
            }

            BuildNodesAndEdges();

            return report;
        }

        private void BuildNodesAndEdges()
        {
            foreach (var log in _db.NetworkLogs)
            {
                _db.UpsertNode(log.SourceIp);
                _db.UpsertNode(log.DestIp);
                _db.UpsertEdge(log.SourceIp, log.DestIp);
            }
        }

        private static List<string> Validate(string src, string dst, string pkt, string ts, int row)
        {
            var errs = new List<string>();

            if (!IsValidIp(src))
                errs.Add($"Row {row}: invalid source_ip '{src}'.");
            if (!IsValidIp(dst))
                errs.Add($"Row {row}: invalid dest_ip '{dst}'.");
            if (!int.TryParse(pkt, out int size) || size <= 0)
                errs.Add($"Row {row}: packet_size must be a positive integer (got '{pkt}').");
            if (!DateTime.TryParse(ts, out _))
                errs.Add($"Row {row}: invalid timestamp '{ts}'.");

            return errs;
        }

        private static bool IsValidIp(string ip) =>
            IPAddress.TryParse(ip, out _);

        private static string[] ParseLine(string line) =>
            line.Split(',');

        private static int IndexOf(string[] headers, string name)
        {
            for (int i = 0; i < headers.Length; i++)
                if (headers[i].Trim().Equals(name, StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }
    }

    public class IngestionReport
    {
        public int RowsInserted { get; set; }
        public int SkippedRows { get; set; }
        public int DuplicatesRemoved { get; set; }
        public List<string> Errors { get; } = new();
        public bool IsSuccess => !Errors.Any(e => e.Contains("Missing required"));
    }
}

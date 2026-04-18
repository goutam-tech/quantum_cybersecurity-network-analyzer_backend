using System.Globalization;
using network_project.Models;

namespace network_project.Helper;

public class CsvParserHelper
{
    private static readonly string[] RequiredHeaders =
        { "source_ip", "dest_ip", "protocol", "packet_size", "timestamp" };

    public (List<NetworkLog> Logs, string? Error) Parse(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var headerLine = reader.ReadLine();

        if (string.IsNullOrWhiteSpace(headerLine))
            return ([], "CSV file is empty.");

        var headers = headerLine.Split(',')
                                .Select(h => h.Trim().ToLowerInvariant())
                                .ToArray();

        var missing = RequiredHeaders.Except(headers).ToList();
        if (missing.Count > 0)
            return ([], $"Missing columns: {string.Join(", ", missing)}");

        var idx = new Dictionary<string, int>();
        for (int i = 0; i < headers.Length; i++)
            idx[headers[i]] = i;

        var logs = new List<NetworkLog>();
        int lineNum = 1;

        while (!reader.EndOfStream)
        {
            lineNum++;
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = line.Split(',');
            if (cols.Length < RequiredHeaders.Length) continue;

            if (!int.TryParse(cols[idx["packet_size"]].Trim(), out var packetSize))
            {
                continue;
            }

            if (!DateTime.TryParse(cols[idx["timestamp"]].Trim(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var ts))
            {
                ts = DateTime.UtcNow;
            }

            logs.Add(new NetworkLog
            {
                SourceIp   = cols[idx["source_ip"]].Trim(),
                DestIp     = cols[idx["dest_ip"]].Trim(),
                Protocol   = cols[idx["protocol"]].Trim(),
                PacketSize = packetSize,
                Timestamp  = ts
            });
        }

        return (logs, null);
    }
}

using network_project.Interfaces;
using network_project.Models;

namespace network_project.Helper;
public class QftAnalysisHelper
{
    private readonly INodeRepository    _nodes;
    private readonly IQftResultRepository _results;
    private readonly INetworkLogRepository _logs;

    private const int BucketCount = 16;

    public QftAnalysisHelper(
        INodeRepository nodes,
        IQftResultRepository results,
        INetworkLogRepository logs)
    {
        _nodes   = nodes;
        _results = results;
        _logs    = logs;
    }

    public async Task RunAsync()
    {
        var allLogs = (await _logs.GetAllAsync()).ToList();
        if (allLogs.Count == 0) return;

        var nodes = await _nodes.GetAllAsync();

        DateTime minTs = allLogs.Min(l => l.Timestamp);
        DateTime maxTs = allLogs.Max(l => l.Timestamp);
        double   span  = (maxTs - minTs).TotalSeconds;
        if (span <= 0) span = 1;

        foreach (var node in nodes)
        {
            var ipLogs = allLogs
                .Where(l => l.SourceIp == node.IpAddress || l.DestIp == node.IpAddress)
                .ToList();

            if (ipLogs.Count == 0) continue;

            double[] signal = new double[BucketCount];
            foreach (var log in ipLogs)
            {
                double relPos   = (log.Timestamp - minTs).TotalSeconds / span;
                int    bucket   = Math.Min((int)(relPos * BucketCount), BucketCount - 1);
                signal[bucket] += log.PacketSize;
            }

            var (dominantFreq, periodicityScore) = ComputeDft(signal);

            await _results.AddAsync(new QftResult
            {
                NodeId            = node.NodeId,
                DominantFrequency = dominantFreq,
                PeriodicityScore  = periodicityScore
            });
        }

        await _results.SaveChangesAsync();
    }

    private static (double DominantFreq, double PeriodicityScore) ComputeDft(double[] signal)
    {
        int n = signal.Length;
        double[] magnitudes = new double[n / 2];

        for (int k = 0; k < n / 2; k++)
        {
            double real = 0, imag = 0;
            for (int t = 0; t < n; t++)
            {
                double angle = 2 * Math.PI * k * t / n;
                real += signal[t] * Math.Cos(angle);
                imag -= signal[t] * Math.Sin(angle);
            }
            magnitudes[k] = Math.Sqrt(real * real + imag * imag);
        }

        double maxMag     = 0;
        int    dominantK  = 1;
        for (int k = 1; k < magnitudes.Length; k++)
        {
            if (magnitudes[k] > maxMag)
            {
                maxMag    = magnitudes[k];
                dominantK = k;
            }
        }

        double totalMag       = magnitudes.Skip(1).Sum();
        double periodicityScore = totalMag > 0 ? maxMag / totalMag : 0;

        return (dominantK, Math.Min(periodicityScore, 1.0));
    }
}

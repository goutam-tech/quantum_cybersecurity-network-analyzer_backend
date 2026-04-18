using network_project.Database;
using network_project.Models;

namespace network_project.Services
{
    public class QFTService
    {
        private readonly InMemoryDatabase _db;

        public QFTService(InMemoryDatabase db) => _db = db;

        public List<QFTResult> Run(int bucketMinutes = 5)
        {
            var results = new List<QFTResult>();
            var nodes = _db.Nodes;
            var logs = _db.NetworkLogs;

            if (!logs.Any()) return results;

            DateTime minTs = logs.Min(l => l.Timestamp);
            DateTime maxTs = logs.Max(l => l.Timestamp);
            TimeSpan span = maxTs - minTs;
            int bucketCount = Math.Max(2, (int)Math.Ceiling(span.TotalMinutes / bucketMinutes));

            foreach (var node in nodes)
            {
                double[] timeSeries = BuildTimeSeries(node.IpAddress, logs, minTs, bucketCount, bucketMinutes);

                Complex[] spectrum = QFT(timeSeries);

                int dominantBin = FindDominantBin(spectrum);
                double dominantFreq = BinToFrequencyHz(dominantBin, spectrum.Length, bucketMinutes);

                double periodicityScore = ComputePeriodicityScore(spectrum);

                results.Add(new QFTResult
                {
                    NodeId = node.Id,
                    IpAddress = node.IpAddress,
                    DominantFrequency = dominantFreq,
                    PeriodicityScore = periodicityScore
                });
            }

            double maxScore = results.Max(r => r.PeriodicityScore);
            if (maxScore > 0)
                foreach (var r in results)
                    r.PeriodicityScore /= maxScore;

            _db.SaveQFTResults(results);
            return results;
        }

        
        private static double[] BuildTimeSeries(
            string ip,
            List<NetworkLog> logs,
            DateTime minTs,
            int bucketCount,
            int bucketMinutes)
        {
            double[] series = new double[bucketCount];

            foreach (var log in logs)
            {
                if (log.SourceIp != ip && log.DestIp != ip) continue;
                int bucket = (int)((log.Timestamp - minTs).TotalMinutes / bucketMinutes);
                bucket = Math.Clamp(bucket, 0, bucketCount - 1);
                series[bucket]++;
            }

            return series;
        }

        public static Complex[] QFT(double[] signal)
        {
            int n = NextPowerOfTwo(signal.Length);
            Complex[] x = new Complex[n];
            for (int i = 0; i < signal.Length; i++)
                x[i] = new Complex(signal[i], 0);
        
            CooleyTukeyFFT(x, n);

            double norm = 1.0 / Math.Sqrt(n);
            for (int i = 0; i < n; i++)
                x[i] = norm * x[i];

            return x;
        }

        private static void CooleyTukeyFFT(Complex[] x, int n)
        {
            int bits = (int)Math.Log2(n);
            for (int i = 1, j = 0; i < n; i++)
            {
                int bit = n >> 1;
                for (; (j & bit) != 0; bit >>= 1)
                    j ^= bit;
                j ^= bit;

                if (i < j)
                    (x[i], x[j]) = (x[j], x[i]);
            }

            for (int len = 2; len <= n; len <<= 1)
            {
                double angle = -2.0 * Math.PI / len;
                Complex wLen = Complex.Euler(angle);

                for (int i = 0; i < n; i += len)
                {
                    Complex w = new Complex(1, 0);

                    for (int j = 0; j < len / 2; j++)
                    {
                        Complex u = x[i + j];
                        Complex v = w * x[i + j + len / 2];

                        x[i + j] = u + v;
                        x[i + j + len / 2] = u - v;

                        w = w * wLen;
                    }
                }
            }
        }

        private static int FindDominantBin(Complex[] spectrum)
        {
            int half = spectrum.Length / 2;
            int dominantBin = 1;
            double maxMag = 0;

            for (int k = 1; k < half; k++)
            {
                double mag = spectrum[k].Magnitude;
                if (mag > maxMag)
                {
                    maxMag = mag;
                    dominantBin = k;
                }
            }
            return dominantBin;
        }

        private static double BinToFrequencyHz(int bin, int n, int bucketMinutes)
        {
            double samplingPeriodSec = bucketMinutes * 60.0;
            double samplingFreqHz = 1.0 / samplingPeriodSec;
            return (double)bin / n * samplingFreqHz;
        }

        private static double ComputePeriodicityScore(Complex[] spectrum)
        {
            int half = spectrum.Length / 2;
            if (half <= 1) return 0;

            double maxMag = 0;
            double sumMag = 0;
            int count = 0;

            for (int k = 1; k < half; k++)
            {
                double mag = spectrum[k].Magnitude;
                sumMag += mag;
                count++;
                if (mag > maxMag) maxMag = mag;
            }

            double meanMag = (count > 0) ? sumMag / count : 1;
            return (meanMag > 0) ? maxMag / meanMag : 0;
        }

        private static int NextPowerOfTwo(int n)
        {
            if (n <= 1) return 1;
            int p = 1;
            while (p < n) p <<= 1;
            return p;
        }
    }
}

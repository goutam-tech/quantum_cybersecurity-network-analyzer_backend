using network_project.Database;
using network_project.Models;
using network_project.Services;

namespace QuantumCyberSecurity.Services
{
    public class QuantumWalkService
    {
        private readonly InMemoryDatabase _db;
        private readonly GraphService _graph;

        public QuantumWalkService(InMemoryDatabase db, GraphService graph)
        {
            _db = db;
            _graph = graph;
        }

        public List<QuantumWalkResult> Run(int steps = 20, double dt = 0.1)
        {
            var (ipIndex, adj) = _graph.BuildAdjacencyMatrix();
            int n = adj.GetLength(0);

            if (n == 0)
                return new List<QuantumWalkResult>();

            double[] degrees = _graph.ComputeDegrees(adj);
            double[,] H = BuildNormalisedHamiltonian(adj, degrees, n);

            double initAmp = 1.0 / Math.Sqrt(n);
            Complex[] psi = new Complex[n];
            for (int i = 0; i < n; i++)
                psi[i] = new Complex(initAmp, 0);

            for (int step = 0; step < steps; step++)
            {
                psi = EvolveSingleStep(H, psi, n, dt);
            }

            double[] probs = new double[n];
            double total = 0;
            for (int i = 0; i < n; i++)
            {
                probs[i] = psi[i].MagnitudeSquared;
                total += probs[i];
            }

            if (total > 0)
                for (int i = 0; i < n; i++)
                    probs[i] /= total;

            var indexToIp = ipIndex.ToDictionary(kv => kv.Value, kv => kv.Key);
            var results = new List<QuantumWalkResult>(n);

            for (int i = 0; i < n; i++)
            {
                string ip = indexToIp[i];
                var node = _db.GetNodeByIp(ip);
                results.Add(new QuantumWalkResult
                {
                    NodeId = node?.Id ?? i,
                    IpAddress = ip,
                    ProbabilityScore = probs[i]
                });
            }

            double maxProb = results.Max(r => r.ProbabilityScore);
            if (maxProb > 0)
                foreach (var r in results)
                    r.ProbabilityScore /= maxProb;

            _db.SaveQuantumWalkResultS(results);
            return results;
        }

        private static double[,] BuildNormalisedHamiltonian(double[,] adj, double[] degrees, int n)
        {
            double[,] H = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j) { H[i, j] = 0; continue; }

                    double denom = Math.Sqrt(degrees[i] * degrees[j]);
                    H[i, j] = (denom > 0) ? adj[i, j] / denom : 0;
                }
            }
            return H;
        }

        private static Complex[] EvolveSingleStep(double[,] H, Complex[] psi, int n, double dt)
        {
            Complex[] psiNew = new Complex[n];

            for (int i = 0; i < n; i++)
            {
                Complex hPsi = new Complex(0, 0);
                for (int j = 0; j < n; j++)
                {
                    if (H[i, j] == 0) continue;
                    hPsi = hPsi + H[i, j] * psi[j];
                }

                Complex rotation = new Complex(hPsi.Imaginary, -hPsi.Real);
                psiNew[i] = psi[i] + dt * rotation;
            }

            double norm = 0;
            for (int i = 0; i < n; i++) norm += psiNew[i].MagnitudeSquared;
            norm = Math.Sqrt(norm);
            if (norm > 0)
                for (int i = 0; i < n; i++)
                    psiNew[i] = (1.0 / norm) * psiNew[i];

            return psiNew;
        }
    }
}

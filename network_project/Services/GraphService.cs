using network_project.Database;

namespace network_project.Services
{
    public class GraphService
    {
        private readonly InMemoryDatabase _db;

        public GraphService(InMemoryDatabase db) => _db = db;

        public (Dictionary<string, int> ipIndex, double[,] adjMatrix) BuildAdjacencyMatrix()
        {
            var nodes = _db.Nodes;
            int n = nodes.Count;

            var ipIndex = new Dictionary<string, int>();
            for(int i = 0; i < n; i++)
            {
                ipIndex[nodes[i].IpAddress] = i;
            }

            double[,] adj = new double[n, n];

            foreach (var edge in _db.Edges)
            {
                if(ipIndex.TryGetValue(edge.SourceIp, out int si) && 
                    ipIndex.TryGetValue(edge.DestIp, out int di))
                {
                    adj[si, di] += edge.Weight;
                    adj[di, si] += edge.Weight;
                }
            }


            return (ipIndex, adj);
        }

        public double[,] NormalizeToTransitionMatrix(double[,] adj)
        {
            int n = adj.GetLength(0);
            double[,] P = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                double rowSum = 0;
                for (int j = 0; j < n; j++) rowSum += adj[i, j];

                if (rowSum == 0) continue;

                for (int j = 0; j < n; j++)
                    P[i, j] = adj[i, j] / rowSum;
            }

            return P;
        }

        public double[] ComputeDegress(double[,] adj)
        {
            int n = adj.GetLength(0);
            double[] degrees = new double[n];
            for (int i = 0; i < n; i++)
            {
                for(int j = 0; j < n; j++)
                {
                    degrees[i] += adj[i, j];
                }
            }

            return degrees;
        }

        internal double[] ComputeDegrees(double[,] adj)
        {
            throw new NotImplementedException();
        }
    }
}

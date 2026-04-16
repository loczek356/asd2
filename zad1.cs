using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASD
{
    public static class FlowExtender
    {
        /// <summary>
        /// Metod wylicza minimalny s-t-przekrój.
        /// </summary>
        /// <param name="undirectedGraph">Nieskierowany graf</param>
        /// <param name="s">wierzchołek źródłowy</param>
        /// <param name="t">wierzchołek docelowy</param>
        /// <param name="minCut">minimalny przekrój</param>
        /// <returns>wartość przekroju</returns>
        public static double MinCut(this Graph<double> undirectedGraph, int s, int t, out Edge<double>[] minCut)
        {
            DiGraph<double> G = new DiGraph<double>(undirectedGraph);
            DiGraph<double> R = new DiGraph<double>(G);
            var res = Flows.FordFulkerson(G, s, t);

            var przepl = res.Item2;
            for (int i = 0; i < G.VertexCount; i++)
            {
                foreach (var edge in G.OutEdges(i))
                {
                    if (res.Item2.HasEdge(i, edge.To))
                    {
                        if (edge.Weight - res.Item2.GetEdgeWeight(i, edge.To) == 0)
                        {
                            R.RemoveEdge(i, edge.To);
                        }
                        else
                        {
                            R.SetEdgeWeight(i, edge.To, edge.Weight - res.Item2.GetEdgeWeight(i, edge.To));
                        }
                    }
                }
            }

            int[] visited = new int[R.VertexCount];


            var bfs = R.BFS();
            var bfsprzep = res.Item2.BFS();
            visited[s] = 1;
            foreach (var edge in bfs.SearchFrom(s))
            {
                visited[edge.To] = 1;
            }

            List<Edge<double>> ret = new List<Edge<double>>();
            foreach (var edge in bfsprzep.SearchFrom(s))
            {
                if (visited[edge.From] == 1 && visited[edge.To] == 0)
                {
                    ret.Add(edge);
                }
            }

            minCut = ret.ToArray();
            return res.Item1;
        }

        /// <summary>
        /// Metada liczy spójność krawędziową grafu oraz minimalny zbiór rozcinający.
        /// </summary>
        /// <param name="undirectedGraph">nieskierowany graf</param>
        /// <param name="cutingSet">zbiór krawędzi rozcinających</param>
        /// <returns>spójność krawędziowa</returns>
        public static int EdgeConnectivity(this Graph<double> undirectedGraph, out Edge<double>[] cutingSet)
        {
            DiGraph<double> G = new DiGraph<double>(undirectedGraph);
            DiGraph<double> R = new DiGraph<double>(G);
            int s = 0;
            int bestT = -1;
            double minCut = Int32.MaxValue;
            for (int t = 1; t < G.VertexCount; t++)
            {
                var resCan = Flows.FordFulkerson(G, s, t);
                if (resCan.Item1 < minCut)
                {
                    minCut = resCan.Item1;
                    bestT = t;
                }
            }

            var res = Flows.FordFulkerson(G, s, bestT);

            var przepl = res.Item2;
            for (int i = 0; i < G.VertexCount; i++)
            {
                foreach (var edge in G.OutEdges(i))
                {
                    if (res.Item2.HasEdge(i, edge.To))
                    {
                        if (edge.Weight - res.Item2.GetEdgeWeight(i, edge.To) == 0)
                        {
                            R.RemoveEdge(i, edge.To);
                        }
                        else
                        {
                            R.SetEdgeWeight(i, edge.To, edge.Weight - res.Item2.GetEdgeWeight(i, edge.To));
                        }
                    }
                }
            }

            int[] visited = new int[R.VertexCount];


            var bfs = R.BFS();
            var bfsprzep = res.Item2.BFS();
            visited[s] = 1;
            foreach (var edge in bfs.SearchFrom(s))
            {
                visited[edge.To] = 1;
            }

            List<Edge<double>> ret = new List<Edge<double>>();
            foreach (var edge in bfsprzep.SearchFrom(s))
            {
                if (visited[edge.From] == 1 && visited[edge.To] == 0)
                {
                    ret.Add(edge);
                }
            }

            cutingSet = ret.ToArray();
            return (int)res.Item1;
        }
    }
}

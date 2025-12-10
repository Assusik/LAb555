using GraphEditorApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace GraphEditorApp.Algorithms
{
    public class DijkstraAlgorithm
    {
        public List<AlgorithmStep> Run(Graph graph, int startId, int targetId)
        {
            var steps = new List<AlgorithmStep>();
            if (graph == null) return steps;

            var dist = graph.Vertices.ToDictionary(v => v.Id, v => int.MaxValue);
            var prev = new Dictionary<int, int?>();
            var used = new HashSet<int>();

            dist[startId] = 0;

            steps.Add(new AlgorithmStep { Description = $"Начало Dijkstra с вершины {graph.GetVertexById(startId)?.Name}" });

            while (used.Count < graph.Vertices.Count)
            {
                int v = graph.Vertices.Where(x => !used.Contains(x.Id)).OrderBy(x => dist[x.Id]).Select(x => x.Id).FirstOrDefault();
                if (dist[v] == int.MaxValue) break;

                used.Add(v);

                foreach (var e in graph.Edges.Where(e => e.FromVertexId == v || (!e.IsDirected && e.ToVertexId == v)))
                {
                    int to = e.FromVertexId == v ? e.ToVertexId : e.FromVertexId;
                    if (used.Contains(to)) continue;

                    int nd = dist[v] + e.Weight;
                    if (nd < dist[to])
                    {
                        dist[to] = nd;
                        prev[to] = v;

                        steps.Add(new AlgorithmStep
                        {
                            Description = $"Обновляем {graph.GetVertexById(to)?.Name} через ребро {v}->{to} = {nd}",
                            ActiveVertices = new List<Vertex> { graph.GetVertexById(to) },
                            ActiveEdges = new List<Edge> { e },
                            VisitedVertices = used.Select(id => graph.GetVertexById(id)).ToList()
                        });
                    }
                }
            }

            // Восстанавливаем путь
            var path = new List<int>();
            int cur = targetId;
            while (cur != startId)
            {
                if (!prev.ContainsKey(cur)) break;
                path.Add(cur);
                cur = prev[cur].Value;
            }
            path.Add(startId);
            path.Reverse();

            steps.Add(new AlgorithmStep
            {
                Description = $"Кратчайший путь: {string.Join(" -> ", path.Select(id => graph.GetVertexById(id)?.Name))}",
                ActiveVertices = path.Select(id => graph.GetVertexById(id)).ToList(),
                ActiveEdges = path.Zip(path.Skip(1), (a, b) => graph.Edges.FirstOrDefault(e =>
                    (e.FromVertexId == a && e.ToVertexId == b) || (!e.IsDirected && e.FromVertexId == b && e.ToVertexId == a))
                ).Where(e => e != null).ToList(),
                VisitedVertices = used.Select(id => graph.GetVertexById(id)).ToList()
            });

            return steps;
        }
    }
}

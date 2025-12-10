using GraphEditorApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace GraphEditorApp.Algorithms
{
    public class BFSAlgorithm
    {
        public List<AlgorithmStep> Run(Graph graph, int startVertexId)
        {
            var steps = new List<AlgorithmStep>();
            if (graph == null) return steps;

            var visited = new HashSet<int>();
            var queue = new Queue<int>();

            visited.Add(startVertexId);
            queue.Enqueue(startVertexId);

            steps.Add(new AlgorithmStep
            {
                Description = $"Начало BFS с вершины {graph.GetVertexById(startVertexId)?.Name}",
                ActiveVertices = new List<Vertex> { graph.GetVertexById(startVertexId) },
                VisitedVertices = visited.Select(id => graph.GetVertexById(id)).ToList()
            });

            while (queue.Count > 0)
            {
                int v = queue.Dequeue();

                foreach (var nb in graph.GetNeighbors(v))
                {
                    if (!visited.Contains(nb))
                    {
                        visited.Add(nb);
                        queue.Enqueue(nb);

                        var edge = graph.Edges.FirstOrDefault(e =>
                            (e.FromVertexId == v && e.ToVertexId == nb) || (!e.IsDirected && e.FromVertexId == nb && e.ToVertexId == v));

                        steps.Add(new AlgorithmStep
                        {
                            Description = $"Посещаем вершину {graph.GetVertexById(nb)?.Name} через ребро {v}->{nb}",
                            ActiveVertices = new List<Vertex> { graph.GetVertexById(nb) },
                            ActiveEdges = edge != null ? new List<Edge> { edge } : new List<Edge>(),
                            VisitedVertices = visited.Select(id => graph.GetVertexById(id)).ToList(),
                            VisitedEdges = graph.Edges.Where(e => visited.Contains(e.FromVertexId) && visited.Contains(e.ToVertexId)).ToList()
                        });
                    }
                }
            }

            steps.Add(new AlgorithmStep
            {
                Description = $"BFS завершён. Путь: {string.Join(" -> ", visited.Select(id => graph.GetVertexById(id)?.Name))}",
                VisitedVertices = visited.Select(id => graph.GetVertexById(id)).ToList()
            });

            return steps;
        }
    }
}

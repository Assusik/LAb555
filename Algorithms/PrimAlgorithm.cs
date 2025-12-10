using GraphEditorApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace GraphEditorApp.Algorithms
{
    public class PrimAlgorithm
    {
        public List<AlgorithmStep> Run(Graph graph)
        {
            var steps = new List<AlgorithmStep>();
            if (graph == null || graph.Vertices.Count == 0) return steps;

            var visited = new HashSet<int>();
            var mst = new List<Edge>();

            var start = graph.Vertices[0];
            visited.Add(start.Id);

            steps.Add(new AlgorithmStep { Description = $"Старт Prim с {start.Name}" });

            while (visited.Count < graph.Vertices.Count)
            {
                var candidate = graph.Edges
                    .Where(e => (visited.Contains(e.FromVertexId) && !visited.Contains(e.ToVertexId)) || (visited.Contains(e.ToVertexId) && !visited.Contains(e.FromVertexId)))
                    .OrderBy(e => e.Weight)
                    .FirstOrDefault();

                if (candidate == null) break;

                mst.Add(candidate);
                int newV = visited.Contains(candidate.FromVertexId) ? candidate.ToVertexId : candidate.FromVertexId;
                visited.Add(newV);

                steps.Add(new AlgorithmStep
                {
                    Description = $"Добавляем ребро {candidate.FromVertexId}->{candidate.ToVertexId} (w={candidate.Weight})",
                    ActiveEdges = new List<Edge> { candidate },
                    ActiveVertices = new List<Vertex> { graph.GetVertexById(newV) },
                    VisitedEdges = mst.ToList(),
                    VisitedVertices = visited.Select(id => graph.GetVertexById(id)).ToList()
                });
            }

            steps.Add(new AlgorithmStep
            {
                Description = "MST готов",
                VisitedEdges = mst.ToList(),
                VisitedVertices = visited.Select(id => graph.GetVertexById(id)).ToList()
            });

            return steps;
        }
    }
}

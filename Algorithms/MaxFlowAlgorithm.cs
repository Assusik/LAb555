using GraphEditorApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphEditorApp.Algorithms
{
    public class MaxFlowAlgorithm
    {
        public (List<AlgorithmStep> Steps, int FinalFlow) Run(Graph graph, int sourceId, int sinkId)
        {
            var steps = new List<AlgorithmStep>();
            if (graph == null) return (steps, 0);

            var edgeById = graph.Edges.ToDictionary(e => e.Id, e => e);
            var capacity = graph.Edges.ToDictionary(e => e.Id, e => e.Capacity > 0 ? e.Capacity : e.Weight);
            var flow = graph.Edges.ToDictionary(e => e.Id, e => 0);
            int maxFlow = 0;

            while (true)
            {
                var parent = new Dictionary<int, (int edgeId, int dir)>();
                var visited = new HashSet<int>();
                var q = new Queue<int>();
                q.Enqueue(sourceId);
                visited.Add(sourceId);

                while (q.Count > 0)
                {
                    var v = q.Dequeue();
                    foreach (var e in graph.Edges)
                    {
                        if (e.FromVertexId == v && capacity[e.Id] - flow[e.Id] > 0 && !visited.Contains(e.ToVertexId))
                        {
                            visited.Add(e.ToVertexId);
                            parent[e.ToVertexId] = (e.Id, 1);
                            q.Enqueue(e.ToVertexId);
                        }
                        else if (e.ToVertexId == v && flow[e.Id] > 0 && !visited.Contains(e.FromVertexId))
                        {
                            visited.Add(e.FromVertexId);
                            parent[e.FromVertexId] = (e.Id, -1);
                            q.Enqueue(e.FromVertexId);
                        }
                    }
                }

                if (!visited.Contains(sinkId))
                {
                    // Финальный шаг: подсветка всех ребер с ненулевым потоком
                    steps.Add(new AlgorithmStep
                    {
                        Description = $"Максимальный поток достигнут: {maxFlow}",
                        VisitedEdges = graph.Edges.Where(e => flow[e.Id] > 0).ToList()
                    });
                    break;
                }

                var path = new List<(Edge edge, int dir)>();
                int bottleneck = int.MaxValue;
                int cur = sinkId;

                while (cur != sourceId)
                {
                    var info = parent[cur];
                    var e = edgeById[info.edgeId];
                    path.Add((e, info.dir));
                    bottleneck = Math.Min(bottleneck, info.dir == 1 ? capacity[e.Id] - flow[e.Id] : flow[e.Id]);
                    cur = info.dir == 1 ? e.FromVertexId : e.ToVertexId;
                }
                path.Reverse();

                steps.Add(new AlgorithmStep
                {
                    Description = $"Найден путь увеличения потока на {bottleneck}",
                    ActiveEdges = path.Select(t => t.edge).ToList(),
                    ActiveVertices = path.Select(t => graph.GetVertexById(t.edge.FromVertexId)).ToList(),
                    VisitedEdges = graph.Edges.Where(e => flow[e.Id] > 0).ToList()
                });

                foreach (var (edge, dir) in path)
                {
                    if (dir == 1) flow[edge.Id] += bottleneck;
                    else flow[edge.Id] -= bottleneck;
                }
                maxFlow += bottleneck;

                // обновляем текущий поток в ребрах
                foreach (var e in graph.Edges) e.Flow = flow[e.Id];

                steps.Add(new AlgorithmStep
                {
                    Description = $"После увеличения: {string.Join(", ", graph.Edges.Select(e => $"{e.FromVertexId}->{e.ToVertexId}:{e.Flow}/{capacity[e.Id]}"))}",
                    VisitedEdges = graph.Edges.Where(e => flow[e.Id] > 0).ToList()
                });
            }

            return (steps, maxFlow);
        }
    }
}

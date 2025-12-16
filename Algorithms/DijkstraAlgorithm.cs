using GraphEditorApp.Models;
using System;
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

            var visited = new HashSet<int>();
            var distances = new Dictionary<int, int>();
            var previous = new Dictionary<int, int?>();

            // -----------------------------
            // 1. Инициализация: все вершины
            // -----------------------------
            foreach (var v in graph.Vertices)
            {
                distances[v.Id] = v.Id == startId ? 0 : int.MaxValue;
                previous[v.Id] = null;
                v.Properties["Distance"] = v.Id == startId ? "0" : "∞";
            }

            foreach (var e in graph.Edges)
                e.DisplayWeight = "∞";

            // Создаём первый шаг после выставления всех значений
            steps.Add(new AlgorithmStep
            {
                Description = "Инициализация: все вершины имеют вес ∞, стартовая = 0",
                ActiveVertices = new List<Vertex> { graph.GetVertexById(startId) },
                VisitedVertices = graph.Vertices.Select(v => new Vertex
                {
                    Id = v.Id,
                    Name = v.Name,
                    Properties = new Dictionary<string, string>(v.Properties)
                }).ToList(),
                ActiveEdges = graph.Edges.Select(edge => new Edge
                {
                    Id = edge.Id,
                    FromVertexId = edge.FromVertexId,
                    ToVertexId = edge.ToVertexId,
                    Weight = edge.Weight,
                    DisplayWeight = edge.DisplayWeight,
                    IsDirected = edge.IsDirected
                }).ToList()
            });

            // -----------------------------
            // 2. Основной цикл
            // -----------------------------
            while (visited.Count < graph.Vertices.Count)
            {
                // Выбираем непосещённую вершину с минимальным расстоянием
                int? current = graph.Vertices
                    .Where(v => !visited.Contains(v.Id))
                    .OrderBy(v => distances[v.Id])
                    .Select(v => (int?)v.Id)
                    .FirstOrDefault();

                if (current == null || distances[current.Value] == int.MaxValue)
                    break; // больше достижимых вершин нет

                visited.Add(current.Value);
                var currentVertex = graph.GetVertexById(current.Value);

                // -----------------------------
                // Обновляем расстояния соседей
                // -----------------------------
                foreach (var edge in graph.Edges.Where(e => e.FromVertexId == current || e.ToVertexId == current))
                {
                    int neighborId = edge.FromVertexId == current ? edge.ToVertexId : edge.FromVertexId;
                    if (visited.Contains(neighborId)) continue;

                    int newDist = distances[current.Value] + edge.Weight;
                    if (newDist < distances[neighborId])
                    {
                        distances[neighborId] = newDist;
                        previous[neighborId] = current.Value;
                        graph.GetVertexById(neighborId).Properties["Distance"] = newDist.ToString();

                        // Для визуализации обновляем DisplayWeight на ребре
                        edge.DisplayWeight = newDist.ToString();
                    }
                }

                // -----------------------------
                // Добавляем шаг
                // -----------------------------
                steps.Add(new AlgorithmStep
                {
                    Description = $"Текущая вершина: {currentVertex.Name}, обновлены соседние вершины",
                    VisitedVertices = graph.Vertices.Where(v => visited.Contains(v.Id)).ToList(),
                    ActiveVertices = new List<Vertex> { currentVertex },
                    ActiveEdges = graph.Edges.Where(e => e.FromVertexId == current || e.ToVertexId == current).ToList()
                });

                if (current.Value == targetId) break;
            }

            // -----------------------------
            // 3. Финальный шаг: путь до цели
            // -----------------------------
            var path = new List<Vertex>();
            int? stepId = targetId;
            while (stepId != null)
            {
                path.Insert(0, graph.GetVertexById(stepId.Value));
                stepId = previous[stepId.Value];
            }

            steps.Add(new AlgorithmStep
            {
                Description = $"Алгоритм завершён. Кратчайший путь до {graph.GetVertexById(targetId)?.Name}: {string.Join(" -> ", path.Select(v => v.Name))}",
                VisitedVertices = graph.Vertices.ToList(),
                ActiveVertices = path,
                ActiveEdges = GetEdgesFromPath(graph, path)
            });

            return steps;
        }

        private List<Edge> GetEdgesFromPath(Graph graph, List<Vertex> path)
        {
            var edges = new List<Edge>();
            for (int i = 0; i < path.Count - 1; i++)
            {
                var edge = graph.Edges.FirstOrDefault(e =>
                    (e.FromVertexId == path[i].Id && e.ToVertexId == path[i + 1].Id) ||
                    (!e.IsDirected && e.FromVertexId == path[i + 1].Id && e.ToVertexId == path[i].Id));
                if (edge != null) edges.Add(edge);
            }
            return edges;
        }
    }
}

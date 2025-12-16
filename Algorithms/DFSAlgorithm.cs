using GraphEditorApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace GraphEditorApp.Algorithms
{
    public class DFSAlgorithm
    {
        private Graph _graph;
        private List<AlgorithmStep> _steps;
        private HashSet<int> _visited;
        private Stack<int> _stack; // для отображения текущего пути

        public List<AlgorithmStep> Run(Graph graph, int startVertexId)
        {
            _graph = graph;
            _steps = new List<AlgorithmStep>();
            _visited = new HashSet<int>();
            _stack = new Stack<int>();

            _steps.Add(new AlgorithmStep
            {
                Description = $"Начало DFS с вершины {_graph.GetVertexById(startVertexId)?.Name}. Стэк пуст.",
                ActiveVertices = new List<Vertex> { _graph.GetVertexById(startVertexId) }
            });

            Dfs(startVertexId);

            _steps.Add(new AlgorithmStep
            {
                Description = $"DFS завершён. Путь: {string.Join(" -> ", _visited.Select(id => _graph.GetVertexById(id)?.Name))}",
                VisitedVertices = _visited.Select(id => _graph.GetVertexById(id)).ToList()
            });

            return _steps;
        }

        private void Dfs(int v)
        {
            _visited.Add(v);
            _stack.Push(v);

            _steps.Add(new AlgorithmStep
            {
                Description = $"Заходим в {_graph.GetVertexById(v)?.Name}. Стэк: {string.Join(" -> ", _stack.Select(id => _graph.GetVertexById(id)?.Name))}",
                ActiveVertices = new List<Vertex> { _graph.GetVertexById(v) },
                VisitedVertices = _visited.Select(id => _graph.GetVertexById(id)).ToList()
            });

            foreach (var nb in _graph.GetNeighbors(v))
            {
                if (!_visited.Contains(nb))
                {
                    var edge = _graph.Edges.FirstOrDefault(e =>
                        (e.FromVertexId == v && e.ToVertexId == nb) || (!e.IsDirected && e.FromVertexId == nb && e.ToVertexId == v));

                    _steps.Add(new AlgorithmStep
                    {
                        Description = $"Идём по ребру к {_graph.GetVertexById(nb)?.Name}. Стэк: {string.Join(" -> ", _stack.Select(id => _graph.GetVertexById(id)?.Name))}",
                        ActiveVertices = new List<Vertex> { _graph.GetVertexById(nb) },
                        ActiveEdges = edge != null ? new List<Edge> { edge } : new List<Edge>(),
                        VisitedVertices = _visited.Select(id => _graph.GetVertexById(id)).ToList()
                    });

                    Dfs(nb);
                }
            }

            _stack.Pop();

            _steps.Add(new AlgorithmStep
            {
                Description = $"Выходим из {_graph.GetVertexById(v)?.Name}. Стэк после выхода: {string.Join(" -> ", _stack.Select(id => _graph.GetVertexById(id)?.Name))}",
                VisitedVertices = _visited.Select(id => _graph.GetVertexById(id)).ToList()
            });
        }
    }
}

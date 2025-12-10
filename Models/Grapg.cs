using System.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace GraphEditorApp.Models
{
    public class Graph
    {
        public List<Vertex> Vertices { get; set; }
        public List<Edge> Edges { get; set; }
        public bool IsDirected { get; set; }
        public bool IsWeighted { get; set; }
        public string Name { get; set; }

        public Graph()
        {
            Vertices = new List<Vertex>();
            Edges = new List<Edge>();
            Name = "New Graph";
        }

        public Vertex GetVertexById(int id) => Vertices.FirstOrDefault(v => v.Id == id);
        public Edge GetEdgeById(int id) => Edges.FirstOrDefault(e => e.Id == id);
        public List<Edge> GetOutgoingEdges(Vertex v)
        {
            return Edges.Where(e => e.FromVertexId == v.Id).ToList();
        }

        public void AddVertex(Point position)
        {
            int newId = Vertices.Count > 0 ? Vertices.Max(v => v.Id) + 1 : 0;
            Vertices.Add(new Vertex(newId, position));
        }

        public bool AddEdge(int fromId, int toId, int weight = 1, bool directed = false, int capacity = 0)
        {
            if (!Vertices.Any(v => v.Id == fromId) || !Vertices.Any(v => v.Id == toId))
                return false;

            if (Edges.Any(e => e.FromVertexId == fromId && e.ToVertexId == toId))
                return false;

            if (!directed && Edges.Any(e => e.FromVertexId == toId && e.ToVertexId == fromId))
                return false;

            int newId = Edges.Count > 0 ? Edges.Max(e => e.Id) + 1 : 0;
            var edge = new Edge(fromId, toId, weight, directed) { Id = newId, Capacity = capacity };
            Edges.Add(edge);
            return true;
        }

        public void RemoveVertex(int vertexId)
        {
            Vertices.RemoveAll(v => v.Id == vertexId);
            Edges.RemoveAll(e => e.FromVertexId == vertexId || e.ToVertexId == vertexId);
        }

        public void RemoveEdge(int edgeId)
        {
            Edges.RemoveAll(e => e.Id == edgeId);
        }

        public int[,] GetAdjacencyMatrix()
        {
            int n = Vertices.Count;
            int[,] matrix = new int[n, n];
            var vertexIds = Vertices.Select(v => v.Id).OrderBy(id => id).ToList();

            foreach (var edge in Edges)
            {
                int fromIndex = vertexIds.IndexOf(edge.FromVertexId);
                int toIndex = vertexIds.IndexOf(edge.ToVertexId);

                if (edge.IsDirected)
                {
                    matrix[fromIndex, toIndex] = edge.Weight;
                }
                else
                {
                    matrix[fromIndex, toIndex] = edge.Weight;
                    matrix[toIndex, fromIndex] = edge.Weight;
                }
            }

            return matrix;
        }

        public List<int> GetNeighbors(int vertexId)
        {
            var neighbors = new List<int>();

            foreach (var edge in Edges)
            {
                if (edge.FromVertexId == vertexId)
                    neighbors.Add(edge.ToVertexId);

                if (!edge.IsDirected && edge.ToVertexId == vertexId)
                    neighbors.Add(edge.FromVertexId);
            }

            return neighbors.Distinct().ToList();
        }

        public void ResetColors()
        {
            foreach (var vertex in Vertices)
                vertex.Color = Color.LightBlue;

            foreach (var edge in Edges)
                edge.Color = Color.Black;
        }
    }
}

using GraphEditorApp.Models;
using System.Collections.Generic;

namespace GraphEditorApp.Models
{
    public class AlgorithmStep
    {
        public string Description { get; set; } = "";

        // Вершины, которые уже посещены
        public List<Vertex> VisitedVertices { get; set; } = new List<Vertex>();

        // Рёбра, которые уже использованы/подсвечены
        public List<Edge> VisitedEdges { get; set; } = new List<Edge>();

        // Текущие активные вершины
        public List<Vertex> ActiveVertices { get; set; } = new List<Vertex>();

        // Текущие активные рёбра
        public List<Edge> ActiveEdges { get; set; } = new List<Edge>();
    }
}

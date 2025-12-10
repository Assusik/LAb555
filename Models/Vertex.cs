using System.Drawing;
using System.Collections.Generic;

namespace GraphEditorApp.Models
{
    public class Vertex
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Point Position { get; set; }
        public Color Color { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public Vertex()
        {
            Properties = new Dictionary<string, string>();
            Color = Color.LightBlue;
        }

        public Vertex(int id, Point position, string name = "") : this()
        {
            Id = id;
            Position = position;
            Name = string.IsNullOrEmpty(name) ? $"V{id}" : name;
        }

        public override string ToString() => Name;
    }
}

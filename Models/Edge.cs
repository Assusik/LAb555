using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;

namespace GraphEditorApp.Models
{
    public class Edge
    {
        public int Id { get; set; }
        public int FromVertexId { get; set; }
        public int ToVertexId { get; set; }

        [DisplayName("Вес")]
        public int Weight { get; set; }

        [DisplayName("Пропускная способность")]
        public int Capacity { get; set; }

        [DisplayName("Поток")]
        public int Flow { get; set; }

        [Browsable(false)]
        public Color Color { get; set; }

        [DisplayName("Направленное")]
        public bool IsDirected { get; set; }

        [Browsable(false)]
        public Dictionary<string, string> Properties { get; set; }

        [Browsable(false)]
        public string Display
        {
            get
            {
                string arrow = IsDirected ? "→" : "—";
                if (Capacity > 0)
                    return $"{FromVertexId} {arrow} {ToVertexId} (w:{Weight}, f:{Flow}/{Capacity})";
                else
                    return $"{FromVertexId} {arrow} {ToVertexId} (w:{Weight})";
            }
        }

        public Edge()
        {
            Properties = new Dictionary<string, string>();
            Color = Color.Black;
            Weight = 1;
            Capacity = 0;
            Flow = 0;
            IsDirected = false;
        }

        public Edge(int from, int to, int weight = 1, bool directed = false) : this()
        {
            FromVertexId = from;
            ToVertexId = to;
            Weight = weight;
            IsDirected = directed;
        }

        public override string ToString() => Display;
    }
}

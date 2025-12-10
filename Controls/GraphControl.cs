using GraphEditorApp.Models;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using GraphEditorApp.Dialogs;
using System.Collections.Generic;

namespace GraphEditorApp.Controls
{
    public class GraphControl : Control
    {
        private const int VertexRadius = 20;
        private const int SelectionRadius = 25;

        public Graph CurrentGraph { get; set; }
        public Vertex SelectedVertex { get; set; }
        public Edge SelectedEdge { get; set; }
        public Vertex HoveredVertex { get; set; }
        public Edge HoveredEdge { get; set; }
        public Point DragStart { get; set; }
        public bool IsDragging { get; set; }
        public bool IsCreatingEdge { get; set; }
        public Vertex EdgeStartVertex { get; set; }
        public bool IsSelectingForEdge { get; set; }

        // Для отображения результатов алгоритмов
        public List<int> ShortestPath { get; set; }
        public Dictionary<int, int> EdgeFlows { get; set; }
        public List<int> MSTEdges { get; set; }

        public event Action<Vertex> VertexClicked;
        public event Action<Edge> EdgeClicked;
        public event Action GraphChanged;
        public event Action<Vertex> VertexSelectedForEdge;


        public GraphControl()
        {
            CurrentGraph = new Graph();
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            ShortestPath = new List<int>();
            EdgeFlows = new Dictionary<int, int>();
            MSTEdges = new List<int>();
            Font = new Font("Segoe UI", 9);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (CurrentGraph == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Рисуем рёбра
            foreach (var edge in CurrentGraph.Edges)
            {
                var fromVertex = CurrentGraph.Vertices.FirstOrDefault(v => v.Id == edge.FromVertexId);
                var toVertex = CurrentGraph.Vertices.FirstOrDefault(v => v.Id == edge.ToVertexId);

                if (fromVertex != null && toVertex != null)
                {
                    Color edgeColor = edge.Color;
                    float edgeWidth = 2;

                    if (edge == SelectedEdge || edge == HoveredEdge)
                    {
                        edgeColor = Color.Red;
                        edgeWidth = 3;
                    }

                    var pen = new Pen(edgeColor, edgeWidth);

                    Point from = fromVertex.Position;
                    Point to = toVertex.Position;

                    // Сдвигаем линию к краю круга
                    var dir = new PointF(to.X - from.X, to.Y - from.Y);
                    float len = (float)Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);
                    if (len > 0)
                    {
                        var nx = dir.X / len;
                        var ny = dir.Y / len;
                        from = new Point((int)(from.X + nx * VertexRadius), (int)(from.Y + ny * VertexRadius));
                        to = new Point((int)(to.X - nx * VertexRadius), (int)(to.Y - ny * VertexRadius));
                    }

                    e.Graphics.DrawLine(pen, from, to);

                    if (edge.IsDirected)
                        DrawArrow(e.Graphics, pen, from, to);

                    // Текст
                    string edgeText = GetEdgeText(edge);
                    var textPos = new Point((from.X + to.X) / 2, (from.Y + to.Y) / 2 - 10);
                    var textSize = e.Graphics.MeasureString(edgeText, Font);
                    var textRect = new Rectangle(textPos.X - (int)textSize.Width / 2 - 4, textPos.Y - (int)textSize.Height / 2, (int)textSize.Width + 8, (int)textSize.Height + 4);
                    e.Graphics.FillRectangle(Brushes.White, textRect);
                    e.Graphics.DrawRectangle(Pens.Black, textRect);
                    using (var sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center;
                        e.Graphics.DrawString(edgeText, Font, Brushes.Black, textRect, sf);
                    }

                    pen.Dispose();
                }
            }

            // Рисуем вершины
            foreach (var vertex in CurrentGraph.Vertices)
            {
                Color vertexColor = vertex.Color;
                var brush = new SolidBrush(vertexColor);
                var rect = new Rectangle(vertex.Position.X - VertexRadius, vertex.Position.Y - VertexRadius, VertexRadius * 2, VertexRadius * 2);

                if (ShortestPath.Contains(vertex.Id))
                {
                    var outer = new Rectangle(vertex.Position.X - VertexRadius - 4, vertex.Position.Y - VertexRadius - 4, VertexRadius * 2 + 8, VertexRadius * 2 + 8);
                    e.Graphics.FillEllipse(Brushes.Gold, outer);
                }

                e.Graphics.FillEllipse(brush, rect);
                e.Graphics.DrawEllipse(Pens.Black, rect);

                string vertexText = vertex.Name;
                if (vertex.Properties.ContainsKey("Distance"))
                    vertexText += $"\n({vertex.Properties["Distance"]})";

                using (var sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    e.Graphics.DrawString(vertexText, Font, Brushes.Black, rect, sf);
                }

                brush.Dispose();
            }

            // Рисуем создаваемое ребро
            if (IsCreatingEdge && EdgeStartVertex != null)
            {
                using (var pen = new Pen(Color.Red, 2) { DashStyle = DashStyle.Dash })
                {
                    Point mp = PointToClient(Cursor.Position);
                    e.Graphics.DrawLine(pen, EdgeStartVertex.Position, mp);

                    string tip = "Выберите вторую вершину";
                    var tipPos = new Point((EdgeStartVertex.Position.X + mp.X) / 2, (EdgeStartVertex.Position.Y + mp.Y) / 2);
                    var tipSize = e.Graphics.MeasureString(tip, Font);
                    var tipRect = new Rectangle(tipPos.X - (int)tipSize.Width / 2, tipPos.Y - 20, (int)tipSize.Width + 6, (int)tipSize.Height + 4);
                    e.Graphics.FillRectangle(Brushes.LightYellow, tipRect);
                    e.Graphics.DrawRectangle(Pens.Black, tipRect);
                    e.Graphics.DrawString(tip, Font, Brushes.Black, tipRect);
                }
            }
        }

        private string GetEdgeText(Edge edge)
        {
            if (edge.Capacity > 0 && edge.IsDirected)
                return $"{edge.Flow}/{edge.Capacity}";
            if (edge.IsDirected)
                return $"{edge.Weight}→";
            return edge.Weight.ToString();
        }

        private void DrawArrow(Graphics g, Pen pen, Point from, Point to)
        {
            float angle = (float)Math.Atan2(to.Y - from.Y, to.X - from.X);
            int arrowSize = 10;
            PointF[] pts = new PointF[3];
            pts[0] = new PointF(to.X, to.Y);
            pts[1] = new PointF(to.X - arrowSize * (float)Math.Cos(angle - Math.PI / 6), to.Y - arrowSize * (float)Math.Sin(angle - Math.PI / 6));
            pts[2] = new PointF(to.X - arrowSize * (float)Math.Cos(angle + Math.PI / 6), to.Y - arrowSize * (float)Math.Sin(angle + Math.PI / 6));
            g.FillPolygon(pen.Brush, pts);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                var vertex = GetVertexAtPoint(e.Location);
                var edge = GetEdgeAtPoint(e.Location);

                if (vertex != null)
                {
                    SelectedVertex = vertex;
                    SelectedEdge = null;
                    VertexClicked?.Invoke(vertex);

                    if (IsSelectingForEdge)
                        VertexSelectedForEdge?.Invoke(vertex);
                    else if (IsCreatingEdge)
                    {
                        if (EdgeStartVertex != null && EdgeStartVertex != vertex)
                        {
                            using (var dialog = new EdgeDialog())
                            {
                                if (dialog.ShowDialog() == DialogResult.OK)
                                {
                                    CurrentGraph.AddEdge(EdgeStartVertex.Id, vertex.Id, dialog.EdgeWeight, dialog.IsDirected, dialog.EdgeCapacity);
                                    GraphChanged?.Invoke();
                                }
                            }
                        }
                        EdgeStartVertex = null;
                        IsCreatingEdge = false;
                    }
                    else
                    {
                        DragStart = e.Location;
                        IsDragging = true;
                    }
                }
                else if (edge != null)
                {
                    SelectedEdge = edge;
                    SelectedVertex = null;
                    EdgeClicked?.Invoke(edge);
                }
                else
                {
                    SelectedVertex = null;
                    SelectedEdge = null;
                    VertexClicked?.Invoke(null);
                    EdgeClicked?.Invoke(null);
                }

                Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                var vertex = GetVertexAtPoint(e.Location);
                if (vertex != null)
                {
                    var ctx = new ContextMenuStrip();

                    var rename = new ToolStripMenuItem("Переименовать");
                    rename.Click += (s, ev) =>
                    {
                        string newName = Microsoft.VisualBasic.Interaction.InputBox("Введите новое имя:", "Переименование", vertex.Name);
                        if (!string.IsNullOrEmpty(newName))
                        {
                            vertex.Name = newName;
                            GraphChanged?.Invoke();
                            Invalidate();
                        }
                    };

                    var remove = new ToolStripMenuItem("Удалить");
                    remove.Click += (s, ev) =>
                    {
                        CurrentGraph.RemoveVertex(vertex.Id);
                        GraphChanged?.Invoke();
                        Invalidate();
                    };

                    ctx.Items.Add(rename);
                    ctx.Items.Add(remove);
                    ctx.Show(this, e.Location);
                }
                else
                {
                    CurrentGraph.AddVertex(e.Location);
                    GraphChanged?.Invoke();
                    Invalidate();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsDragging && SelectedVertex != null)
            {
                SelectedVertex.Position = e.Location;
                Invalidate();
                GraphChanged?.Invoke();
            }
            else
            {
                var prevV = HoveredVertex;
                var prevE = HoveredEdge;

                HoveredVertex = GetVertexAtPoint(e.Location);
                HoveredEdge = GetEdgeAtPoint(e.Location);

                if (prevV != HoveredVertex || prevE != HoveredEdge)
                    Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            IsDragging = false;
        }

        private Vertex GetVertexAtPoint(Point point)
        {
            foreach (var vertex in CurrentGraph.Vertices)
            {
                var dx = point.X - vertex.Position.X;
                var dy = point.Y - vertex.Position.Y;
                var dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist <= SelectionRadius)
                    return vertex;
            }
            return null;
        }

        private Edge GetEdgeAtPoint(Point point)
        {
            const int tol = 6;
            foreach (var edge in CurrentGraph.Edges)
            {
                var from = CurrentGraph.Vertices.FirstOrDefault(v => v.Id == edge.FromVertexId);
                var to = CurrentGraph.Vertices.FirstOrDefault(v => v.Id == edge.ToVertexId);
                if (from == null || to == null) continue;

                double dist = PointToLineDistance(point, from.Position, to.Position);
                if (dist <= tol) return edge;
            }
            return null;
        }


        private double PointToLineDistance(Point p, Point a, Point b)
        {
            double A = p.X - a.X;
            double B = p.Y - a.Y;
            double C = b.X - a.X;
            double D = b.Y - a.Y;

            double dot = A * C + B * D;
            double lenSq = C * C + D * D;
            double param = (lenSq != 0) ? dot / lenSq : -1;

            double xx, yy;
            if (param < 0)
            {
                xx = a.X;
                yy = a.Y;
            }
            else if (param > 1)
            {
                xx = b.X;
                yy = b.Y;
            }
            else
            {
                xx = a.X + param * C;
                yy = a.Y + param * D;
            }

            double dx = p.X - xx;
            double dy = p.Y - yy;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public void StartCreatingEdge()
        {
            IsCreatingEdge = true;
            EdgeStartVertex = SelectedVertex;
            Invalidate();
        }

        public void StartSelectingForEdge()
        {
            IsSelectingForEdge = true;
            IsCreatingEdge = false;
            EdgeStartVertex = null;
        }

        public void ClearSelection()
        {
            SelectedVertex = null;
            SelectedEdge = null;
            IsCreatingEdge = false;
            IsSelectingForEdge = false;
            EdgeStartVertex = null;
            Invalidate();
        }

        public void ClearAlgorithmResults()
        {
            ShortestPath.Clear();
            EdgeFlows.Clear();
            MSTEdges.Clear();

            foreach (var edge in CurrentGraph.Edges)
                edge.Flow = 0;

            foreach (var vertex in CurrentGraph.Vertices)
                if (vertex.Properties.ContainsKey("Distance"))
                    vertex.Properties.Remove("Distance");

            Invalidate();
        }
    }
}

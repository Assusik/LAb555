using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GraphEditorApp.Controls;
using GraphEditorApp.Models;
using GraphEditorApp.Services;
using GraphEditorApp.Algorithms;
using System.Collections.Generic;

namespace GraphEditorApp.Forms
{
    public class MainForm : Form
    {
        private GraphControl graphControl;
        private Button btnLoad, btnSave, btnAddEdge, btnClear, btnRunBFS, btnRunDFS, btnRunDijkstra, btnRunPrim, btnRunMaxFlow;
        private Button btnPrev, btnNext, btnAuto;
        private ComboBox cbStart, cbTarget, cbSource, cbSink;
        private TextBox txtLog;
        private TextBox stepDescription;
        private System.Windows.Forms.Timer autoTimer;
        private List<AlgorithmStep> currentSteps = new List<AlgorithmStep>();
        private int currentStepIndex = 0;

        public MainForm()
        {
            Text = "Graph Editor — Practical 5 (BFS/DFS/Dijkstra/MST/MaxFlow)";
            Width = 1200;
            Height = 820;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            graphControl = new GraphControl { Left = 10, Top = 10, Width = 820, Height = 740 };
            Controls.Add(graphControl);

            int x = 840, y = 10, w = 320;
            btnLoad = new Button { Left = x, Top = y, Width = 100, Text = "Загрузить" };
            btnSave = new Button { Left = x + 110, Top = y, Width = 100, Text = "Сохранить" };
            btnClear = new Button { Left = x + 220, Top = y, Width = 100, Text = "Новый" };
            y += 40;

            btnAddEdge = new Button { Left = x, Top = y, Width = 100, Text = "Создать ребро" };
            btnRunBFS = new Button { Left = x + 110, Top = y, Width = 100, Text = "Run BFS" };
            btnRunDFS = new Button { Left = x + 220, Top = y, Width = 100, Text = "Run DFS" };
            y += 40;

            cbStart = new ComboBox { Left = x, Top = y, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cbTarget = new ComboBox { Left = x + 160, Top = y, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            y += 40;

            btnRunDijkstra = new Button { Left = x, Top = y, Width = 150, Text = "Dijkstra (shortest)" };
            btnRunPrim = new Button { Left = x + 160, Top = y, Width = 150, Text = "Prim (MST)" };
            y += 40;

            cbSource = new ComboBox { Left = x, Top = y, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cbSink = new ComboBox { Left = x + 160, Top = y, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            y += 40;

            btnRunMaxFlow = new Button { Left = x, Top = y, Width = 320, Text = "MaxFlow (Edmonds–Karp)" };
            y += 40;

            btnPrev = new Button { Left = x, Top = y, Width = 100, Text = "Назад" };
            btnNext = new Button { Left = x + 110, Top = y, Width = 100, Text = "Далее" };
            btnAuto = new Button { Left = x + 220, Top = y, Width = 100, Text = "Auto" };
            y += 40;

            stepDescription = new TextBox { Left = x, Top = y, Width = w, Height = 60, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
            y += 70;

            txtLog = new TextBox { Left = x, Top = y, Width = w, Height = 540, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };

            Controls.AddRange(new Control[] {
                btnLoad, btnSave, btnClear, btnAddEdge, btnRunBFS, btnRunDFS, cbStart, cbTarget,
                btnRunDijkstra, btnRunPrim, cbSource, cbSink, btnRunMaxFlow, btnPrev, btnNext, btnAuto,
                stepDescription, txtLog
            });

            // events
            graphControl.GraphChanged += GraphControl_GraphChanged;
            graphControl.VertexClicked += GraphControl_VertexClicked;
            graphControl.EdgeClicked += GraphControl_EdgeClicked;

            btnLoad.Click += BtnLoad_Click;
            btnSave.Click += BtnSave_Click;
            btnClear.Click += BtnClear_Click;
            btnAddEdge.Click += BtnAddEdge_Click;
            btnRunBFS.Click += BtnRunBFS_Click;
            btnRunDFS.Click += BtnRunDFS_Click;
            btnRunDijkstra.Click += BtnRunDijkstra_Click;
            btnRunPrim.Click += BtnRunPrim_Click;
            btnRunMaxFlow.Click += BtnRunMaxFlow_Click;
            btnPrev.Click += BtnPrev_Click;
            btnNext.Click += BtnNext_Click;
            btnAuto.Click += BtnAuto_Click;

            autoTimer = new System.Windows.Forms.Timer { Interval = 800 };
            autoTimer.Tick += (s, e) =>
            {
                if (currentStepIndex < currentSteps.Count - 1) ShowStep(++currentStepIndex);
                else autoTimer.Stop();
            };

            InitializeSample();
        }

        private void InitializeSample()
        {
            var g = graphControl.CurrentGraph;
            g.Vertices.Clear();
            g.Edges.Clear();

            g.AddVertex(new Point(150, 100)); g.Vertices.Last().Name = "A";
            g.AddVertex(new Point(350, 80)); g.Vertices.Last().Name = "B";
            g.AddVertex(new Point(550, 120)); g.Vertices.Last().Name = "C";
            g.AddVertex(new Point(250, 300)); g.Vertices.Last().Name = "D";
            g.AddVertex(new Point(450, 320)); g.Vertices.Last().Name = "E";

            g.AddEdge(0, 1, 2, false);
            g.AddEdge(0, 3, 6, false);
            g.AddEdge(1, 2, 3, false);
            g.AddEdge(1, 3, 8, false);
            g.AddEdge(1, 4, 5, false);
            g.AddEdge(2, 4, 7, false);
            g.AddEdge(3, 4, 9, false);

            RefreshCombos();
            graphControl.Invalidate();
            Log("Sample graph created.");
        }

        private void GraphControl_GraphChanged()
        {
            RefreshCombos();
        }

        private void GraphControl_VertexClicked(Vertex v)
        {
            // при клике по вершине — выделим её в комбобокс
            RefreshCombos();
        }

        private void GraphControl_EdgeClicked(Edge e)
        {
            if (e != null) Log($"Edge clicked: {e.FromVertexId}->{e.ToVertexId} w={e.Weight} cap={e.Capacity}");
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "JSON|*.json|CSV|*.csv" };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            try
            {
                if (ofd.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    var g = FileService.LoadGraphFromJson(ofd.FileName);
                    graphControl.CurrentGraph = g;
                    graphControl.CurrentGraph.ResetColors();
                    graphControl.Invalidate();
                    Log($"Graph loaded from {ofd.FileName}");
                }
                else
                {
                    var g = FileService.LoadGraphFromCsv(ofd.FileName);
                    graphControl.CurrentGraph = g;
                    graphControl.CurrentGraph.ResetColors();
                    graphControl.Invalidate();
                    Log($"Graph loaded from CSV {ofd.FileName}");
                }
                RefreshCombos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Filter = "JSON|*.json|CSV (adjacency)|*.csv" };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            try
            {
                if (sfd.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    FileService.SaveGraphToJson(graphControl.CurrentGraph, sfd.FileName);
                    Log($"Graph saved to {sfd.FileName}");
                }
                else
                {
                    FileService.SaveAdjacencyMatrixToCsv(graphControl.CurrentGraph, sfd.FileName);
                    Log($"Adjacency matrix exported to {sfd.FileName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            graphControl.CurrentGraph = new Models.Graph();
            RefreshCombos();
            graphControl.Invalidate();
            Log("New graph created.");
        }

        private void BtnAddEdge_Click(object sender, EventArgs e) => graphControl.StartCreatingEdge();

        private void BtnRunBFS_Click(object sender, EventArgs e)
        {
            graphControl.ClearAlgorithmResults();
            if (cbStart.SelectedItem == null) { MessageBox.Show("Выберите стартовую вершину"); return; }
            var v = cbStart.SelectedItem as Vertex;
            var alg = new BFSAlgorithm();
            currentSteps = alg.Run(graphControl.CurrentGraph, v.Id);
            currentStepIndex = 0;
            Log("BFS started.");
            ShowStep(0);
        }

        private void BtnRunDFS_Click(object sender, EventArgs e)
        {
            graphControl.ClearAlgorithmResults();
            if (cbStart.SelectedItem == null) { MessageBox.Show("Выберите стартовую вершину"); return; }
            var v = cbStart.SelectedItem as Vertex;
            var alg = new DFSAlgorithm();
            currentSteps = alg.Run(graphControl.CurrentGraph, v.Id);
            currentStepIndex = 0;
            Log("DFS started.");
            ShowStep(0);
        }

        private void BtnRunDijkstra_Click(object sender, EventArgs e)
        {
            graphControl.ClearAlgorithmResults();
            if (cbStart.SelectedItem == null || cbTarget.SelectedItem == null) { MessageBox.Show("Выберите старт и цель"); return; }
            var s = (cbStart.SelectedItem as Vertex).Id;
            var t = (cbTarget.SelectedItem as Vertex).Id;
            var alg = new DijkstraAlgorithm();
            currentSteps = alg.Run(graphControl.CurrentGraph, s, t);
            currentStepIndex = 0;
            Log("Dijkstra started.");
            ShowStep(0);
        }

        private void BtnRunPrim_Click(object sender, EventArgs e)
        {
            graphControl.ClearAlgorithmResults();
            var alg = new PrimAlgorithm();
            currentSteps = alg.Run(graphControl.CurrentGraph);
            currentStepIndex = 0;
            Log("Prim (MST) started.");
            ShowStep(0);
        }

        private void BtnRunMaxFlow_Click(object sender, EventArgs e)
        {
            graphControl.ClearAlgorithmResults();
            if (cbSource.SelectedItem == null || cbSink.SelectedItem == null) { MessageBox.Show("Выберите source и sink"); return; }
            var s = (cbSource.SelectedItem as Vertex).Id;
            var t = (cbSink.SelectedItem as Vertex).Id;

            // Убедимся, что рёбра направлены (для транспортной сети)
            foreach (var edge in graphControl.CurrentGraph.Edges)
                edge.IsDirected = true;

            var alg = new MaxFlowAlgorithm();
            var tuple = alg.Run(graphControl.CurrentGraph, s, t);
            currentSteps = tuple.Steps;
            currentStepIndex = 0;
            Log($"MaxFlow started (source={s}, sink={t})");
            ShowStep(0);
            Log($"Final flow: {tuple.FinalFlow}");
        }

        private void BtnPrev_Click(object sender, EventArgs e)
        {
            if (currentSteps == null || currentSteps.Count == 0) return;
            if (currentStepIndex > 0) ShowStep(--currentStepIndex);
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            if (currentSteps == null || currentSteps.Count == 0) return;
            if (currentStepIndex < currentSteps.Count - 1) ShowStep(++currentStepIndex);
        }

        private void BtnAuto_Click(object sender, EventArgs e)
        {
            if (autoTimer.Enabled) { autoTimer.Stop(); btnAuto.Text = "Auto"; }
            else { autoTimer.Start(); btnAuto.Text = "Stop"; }
        }

        private void RefreshCombos()
        {
            cbStart.Items.Clear(); cbTarget.Items.Clear(); cbSource.Items.Clear(); cbSink.Items.Clear();
            foreach (var v in graphControl.CurrentGraph.Vertices)
            {
                cbStart.Items.Add(v);
                cbTarget.Items.Add(v);
                cbSource.Items.Add(v);
                cbSink.Items.Add(v);
            }
        }

        private void ShowStep(int index)
        {
            if (currentSteps == null || index < 0 || index >= currentSteps.Count) return;
            var step = currentSteps[index];
            currentStepIndex = index;

            // вывод описания
            stepDescription.Text = $"Шаг {index + 1}/{currentSteps.Count}: {step.Description}";

            // ВАЖНО: не стирать навечно — сбрасываем только перед запуском (делается в ClearAlgorithmResults)
            // Сброс цветов делаем один раз до показа шагов
            graphControl.CurrentGraph.ResetColors();

            // Подсветка уже посещённых вершин
            foreach (var v in step.VisitedVertices)
            {
                var vv = graphControl.CurrentGraph.Vertices.FirstOrDefault(x => x.Id == v.Id);
                if (vv != null) vv.Color = Color.Green; // посещённые вершины зелёные
            }

            // Подсветка активных вершин текущего шага
            foreach (var v in step.ActiveVertices)
            {
                var vv = graphControl.CurrentGraph.Vertices.FirstOrDefault(x => x.Id == v.Id);
                if (vv != null) vv.Color = Color.Orange; // текущие активные вершины оранжевые
            }

            // Подсветка уже посещённых рёбер
            foreach (var e in step.VisitedEdges)
            {
                var ee = graphControl.CurrentGraph.Edges.FirstOrDefault(x => x.Id == e.Id);
                if (ee != null) ee.Color = Color.Green; // посещённые рёбра зелёные
            }

            // Подсветка активных рёбер текущего шага
            foreach (var e in step.ActiveEdges)
            {
                var ee = graphControl.CurrentGraph.Edges.FirstOrDefault(x => x.Id == e.Id);
                if (ee != null) ee.Color = Color.LimeGreen; // текущие активные рёбра ярко-зелёные
            }


            // Перерисуем
            graphControl.Invalidate();
            Log(stepDescription.Text);
        }

        private void Log(string text)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }
    }
}

using GraphEditorApp.Models;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System;

namespace GraphEditorApp.Services
{
    public static class FileService
    {
        public static void SaveGraphToJson(Graph graph, string filePath)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto
                };
                string json = JsonConvert.SerializeObject(graph, settings);
                File.WriteAllText(filePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сохранения JSON: {ex.Message}", ex);
            }
        }

        public static Graph LoadGraphFromJson(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                return JsonConvert.DeserializeObject<Graph>(json, settings);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки JSON: {ex.Message}", ex);
            }
        }

        public static void SaveAdjacencyMatrixToCsv(Graph graph, string filePath)
        {
            try
            {
                var matrix = graph.GetAdjacencyMatrix();
                var sb = new StringBuilder();

                sb.Append(",");
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    var vertex = graph.Vertices[i];
                    sb.Append($"{vertex.Name}");
                    if (i < matrix.GetLength(0) - 1)
                        sb.Append(",");
                }
                sb.AppendLine();

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    var vertex = graph.Vertices[i];
                    sb.Append($"{vertex.Name},");
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        sb.Append(matrix[i, j]);
                        if (j < matrix.GetLength(1) - 1)
                            sb.Append(",");
                    }
                    sb.AppendLine();
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сохранения CSV: {ex.Message}", ex);
            }
        }

        public static Graph LoadGraphFromCsv(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length < 2)
                    throw new ArgumentException("Неверный формат CSV файла");

                var header = lines[0].Split(',');
                int size = header.Length - 1;

                var graph = new Graph();

                for (int i = 0; i < size; i++)
                {
                    string vertexName = header[i + 1].Trim();
                    graph.AddVertex(new System.Drawing.Point(100 + i * 100, 100 + (i % 5) * 60));
                    graph.Vertices[i].Name = vertexName;
                }

                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');
                    for (int j = 1; j < values.Length; j++)
                    {
                        if (int.TryParse(values[j], out int weight) && weight > 0)
                        {
                            graph.AddEdge(i - 1, j - 1, weight, false);
                        }
                    }
                }

                return graph;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки CSV: {ex.Message}", ex);
            }
        }
    }
}

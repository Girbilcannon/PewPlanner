using System.IO;
using System.Linq;
using System.Text.Json;
using PewPlanner.Models;

namespace PewPlanner.Services
{
    public static class GraphSerializer
    {
        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            WriteIndented = true
        };

        public static void Save(string filePath, GraphDocument document)
        {
            foreach (var connection in document.Connections)
                connection.RefreshSerializedRefs();

            string json = JsonSerializer.Serialize(document, WriteOptions);
            File.WriteAllText(filePath, json);
        }

        public static GraphDocument Load(string filePath)
        {
            if (!File.Exists(filePath))
                return new GraphDocument();

            string json = File.ReadAllText(filePath);

            var document = JsonSerializer.Deserialize<GraphDocument>(json);
            if (document == null)
                return new GraphDocument();

            foreach (var node in document.Nodes)
            {
                for (int i = 0; i < node.Inputs.Count; i++)
                {
                    node.Inputs[i].Node = node;
                    node.Inputs[i].Index = i;
                }

                for (int i = 0; i < node.Outputs.Count; i++)
                {
                    node.Outputs[i].Node = node;
                    node.Outputs[i].Index = i;
                }
            }

            foreach (var connection in document.Connections)
            {
                var fromNode = document.Nodes.FirstOrDefault(n => n.Title == connection.FromNodeTitle);
                var toNode = document.Nodes.FirstOrDefault(n => n.Title == connection.ToNodeTitle);

                if (fromNode == null || toNode == null)
                    continue;

                if (connection.FromSocketIndex < 0 || connection.FromSocketIndex >= fromNode.Outputs.Count)
                    continue;

                if (connection.ToSocketIndex < 0 || connection.ToSocketIndex >= toNode.Inputs.Count)
                    continue;

                connection.From = fromNode.Outputs[connection.FromSocketIndex];
                connection.To = toNode.Inputs[connection.ToSocketIndex];

                connection.From.IsConnected = true;
                connection.To.IsConnected = true;
            }

            return document;
        }
    }
}
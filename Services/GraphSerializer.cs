using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
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
            EnsureUniqueNodeIds(document);
            RebuildSerializedSocketLinks(document);

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
            if (string.IsNullOrWhiteSpace(json))
                return new GraphDocument();

            JsonNode? root = JsonNode.Parse(json);
            if (root is not JsonObject rootObject)
                return new GraphDocument();

            var document = rootObject.Deserialize<GraphDocument>();
            if (document == null)
                return new GraphDocument();

            RebindNodeSockets(document);
            EnsureUniqueNodeIds(document);
            RebindConnections(document, rootObject);
            RebuildSerializedSocketLinks(document);

            return document;
        }

        private static void RebindNodeSockets(GraphDocument document)
        {
            foreach (var node in document.Nodes)
            {
                node.EnsureId();

                if (node.Inputs == null)
                    node.Inputs = new();

                if (node.Outputs == null)
                    node.Outputs = new();

                for (int i = 0; i < node.Inputs.Count; i++)
                {
                    node.Inputs[i].Node = node;
                    node.Inputs[i].IsInput = true;
                    node.Inputs[i].Index = i;
                    node.Inputs[i].IsConnected = false;
                }

                for (int i = 0; i < node.Outputs.Count; i++)
                {
                    node.Outputs[i].Node = node;
                    node.Outputs[i].IsInput = false;
                    node.Outputs[i].Index = i;
                    node.Outputs[i].IsConnected = false;
                }
            }
        }

        private static void EnsureUniqueNodeIds(GraphDocument document)
        {
            var used = new HashSet<string>();

            foreach (var node in document.Nodes)
            {
                node.EnsureId();

                while (used.Contains(node.Id))
                    node.Id = GraphNode.CreateNewId();

                used.Add(node.Id);
            }
        }

        private static void RebindConnections(GraphDocument document, JsonObject rootObject)
        {
            JsonArray? rawConnections = rootObject["Connections"] as JsonArray;
            var nodeById = document.Nodes.ToDictionary(n => n.Id, n => n);

            for (int i = 0; i < document.Connections.Count; i++)
            {
                var connection = document.Connections[i];
                JsonObject? raw = rawConnections?[i] as JsonObject;

                string fromNodeId = connection.FromNodeId;
                string toNodeId = connection.ToNodeId;

                // Backward compatibility for old files.
                if (string.IsNullOrWhiteSpace(fromNodeId))
                {
                    string? oldTitle = raw?["FromNodeTitle"]?.GetValue<string>();
                    fromNodeId = ResolveLegacyNodeId(document, oldTitle);
                }

                if (string.IsNullOrWhiteSpace(toNodeId))
                {
                    string? oldTitle = raw?["ToNodeTitle"]?.GetValue<string>();
                    toNodeId = ResolveLegacyNodeId(document, oldTitle);
                }

                if (string.IsNullOrWhiteSpace(fromNodeId) || string.IsNullOrWhiteSpace(toNodeId))
                    continue;

                if (!nodeById.TryGetValue(fromNodeId, out var fromNode))
                    continue;

                if (!nodeById.TryGetValue(toNodeId, out var toNode))
                    continue;

                if (connection.FromSocketIndex < 0 || connection.FromSocketIndex >= fromNode.Outputs.Count)
                    continue;

                if (connection.ToSocketIndex < 0 || connection.ToSocketIndex >= toNode.Inputs.Count)
                    continue;

                connection.From = fromNode.Outputs[connection.FromSocketIndex];
                connection.To = toNode.Inputs[connection.ToSocketIndex];

                connection.From.IsConnected = true;
                connection.To.IsConnected = true;
                connection.RefreshSerializedRefs();
            }
        }

        private static string ResolveLegacyNodeId(GraphDocument document, string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            var node = document.Nodes.FirstOrDefault(n => n.Title == title);
            if (node == null)
                return string.Empty;

            node.EnsureId();
            return node.Id;
        }

        private static void RebuildSerializedSocketLinks(GraphDocument document)
        {
            foreach (var node in document.Nodes)
            {
                foreach (var input in node.Inputs)
                {
                    input.IsConnected = false;
                    input.ClearSerializedConnection();
                }

                foreach (var output in node.Outputs)
                {
                    output.IsConnected = false;
                    output.ClearSerializedConnection();
                }
            }

            foreach (var connection in document.Connections)
            {
                connection.RefreshSerializedRefs();

                connection.From.IsConnected = true;
                connection.To.IsConnected = true;

                // Input socket explicitly records its true source.
                connection.To.ConnectedFromNodeId = connection.From.Node.Id;
                connection.To.ConnectedFromSocketIndex = connection.From.Index;
            }
        }
    }
}
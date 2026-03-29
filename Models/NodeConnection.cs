using System.Text.Json.Serialization;

namespace PewPlanner.Models
{
    public class NodeConnection
    {
        [JsonIgnore]
        public NodeSocket From { get; set; } = null!;

        [JsonIgnore]
        public NodeSocket To { get; set; } = null!;

        public string FromNodeId { get; set; } = string.Empty;
        public int FromSocketIndex { get; set; }

        public string ToNodeId { get; set; } = string.Empty;
        public int ToSocketIndex { get; set; }

        public NodeConnection()
        {
        }

        public NodeConnection(NodeSocket from, NodeSocket to)
        {
            From = from;
            To = to;
            RefreshSerializedRefs();
        }

        public void RefreshSerializedRefs()
        {
            From.Node.EnsureId();
            To.Node.EnsureId();

            FromNodeId = From.Node.Id;
            FromSocketIndex = From.Index;

            ToNodeId = To.Node.Id;
            ToSocketIndex = To.Index;
        }
    }
}
using System.Text.Json.Serialization;

namespace PewPlanner.Models
{
    public class NodeConnection
    {
        [JsonIgnore]
        public NodeSocket From { get; set; } = null!;

        [JsonIgnore]
        public NodeSocket To { get; set; } = null!;

        public string FromNodeTitle { get; set; } = string.Empty;
        public int FromSocketIndex { get; set; }

        public string ToNodeTitle { get; set; } = string.Empty;
        public int ToSocketIndex { get; set; }

        public NodeConnection()
        {
        }

        public NodeConnection(NodeSocket from, NodeSocket to)
        {
            From = from;
            To = to;

            FromNodeTitle = from.Node.Title;
            FromSocketIndex = from.Index;

            ToNodeTitle = to.Node.Title;
            ToSocketIndex = to.Index;
        }

        public void RefreshSerializedRefs()
        {
            FromNodeTitle = From.Node.Title;
            FromSocketIndex = From.Index;

            ToNodeTitle = To.Node.Title;
            ToSocketIndex = To.Index;
        }
    }
}
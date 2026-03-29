using System.Drawing;
using System.Text.Json.Serialization;

namespace PewPlanner.Models
{
    public class NodeSocket
    {
        [JsonIgnore]
        public GraphNode Node { get; set; } = null!;

        public bool IsInput { get; set; }
        public int Index { get; set; }
        public bool IsConnected { get; set; }

        // Serialized directional link data for tools that inspect sockets directly.
        // Meaningful for input sockets because each input accepts only one incoming connection.
        public string ConnectedFromNodeId { get; set; } = string.Empty;
        public int ConnectedFromSocketIndex { get; set; } = -1;

        public NodeSocket()
        {
        }

        public NodeSocket(GraphNode node, bool isInput, int index)
        {
            Node = node;
            IsInput = isInput;
            Index = index;
            IsConnected = false;
            ConnectedFromNodeId = string.Empty;
            ConnectedFromSocketIndex = -1;
        }

        public void ClearSerializedConnection()
        {
            ConnectedFromNodeId = string.Empty;
            ConnectedFromSocketIndex = -1;
        }

        public Point GetPosition()
        {
            return IsInput
                ? Node.GetInputSocketPosition(Index)
                : Node.GetOutputSocketPosition(Index);
        }
    }
}
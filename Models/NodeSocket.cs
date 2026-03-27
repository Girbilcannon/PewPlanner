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

        public NodeSocket()
        {
        }

        public NodeSocket(GraphNode node, bool isInput, int index)
        {
            Node = node;
            IsInput = isInput;
            Index = index;
            IsConnected = false;
        }

        public Point GetPosition()
        {
            return IsInput
                ? Node.GetInputSocketPosition(Index)
                : Node.GetOutputSocketPosition(Index);
        }
    }
}
using System.Collections.Generic;

namespace PewPlanner.Models
{
    public class GraphDocument
    {
        public List<GraphNode> Nodes { get; set; } = new();
        public List<NodeConnection> Connections { get; set; } = new();
    }
}
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json.Serialization;

namespace PewPlanner.Models
{
    public class GraphNode
    {
        public string Title { get; set; } = "Node";
        public string Notes { get; set; } = string.Empty;

        public int X { get; set; }
        public int Y { get; set; }

        public int Width { get; set; } = 170;
        public int HeaderHeight { get; set; } = 34;
        public int SocketSpacing { get; set; } = 20;
        public int BodyPaddingBottom { get; set; } = 14;

        public int NodeColorArgb { get; set; } = Color.FromArgb(34, 39, 47).ToArgb();

        public List<NodeSocket> Inputs { get; set; } = new();
        public List<NodeSocket> Outputs { get; set; } = new();

        public GraphNode()
        {
        }

        public GraphNode(string title, int x, int y)
        {
            Title = title;
            X = x;
            Y = y;

            Inputs.Add(new NodeSocket(this, true, 0));
            Outputs.Add(new NodeSocket(this, false, 0));
        }

        [JsonIgnore]
        public Color NodeColor
        {
            get => Color.FromArgb(NodeColorArgb);
            set => NodeColorArgb = value.ToArgb();
        }

        public int Height
        {
            get
            {
                int socketRows = Inputs.Count > Outputs.Count ? Inputs.Count : Outputs.Count;
                socketRows = socketRows < 1 ? 1 : socketRows;
                return HeaderHeight + 12 + (socketRows * SocketSpacing) + BodyPaddingBottom;
            }
        }

        [JsonIgnore]
        public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

        public Point GetInputSocketPosition(int index)
        {
            return new Point(X, Y + HeaderHeight + 16 + (index * SocketSpacing));
        }

        public Point GetOutputSocketPosition(int index)
        {
            return new Point(X + Width, Y + HeaderHeight + 16 + (index * SocketSpacing));
        }
    }
}
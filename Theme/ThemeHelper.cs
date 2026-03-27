using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PewPlanner
{
    internal static class Theme
    {
        public static readonly Color AppBack = Color.FromArgb(20, 24, 30);
        public static readonly Color CardBack = Color.FromArgb(34, 39, 47);
        public static readonly Color CardBackAlt = Color.FromArgb(42, 48, 57);
        public static readonly Color Border = Color.FromArgb(58, 66, 78);
        public static readonly Color Text = Color.FromArgb(232, 236, 241);
        public static readonly Color MutedText = Color.FromArgb(160, 169, 181);
        public static readonly Color Accent = Color.FromArgb(95, 168, 255);
        public static readonly Color AccentSoft = Color.FromArgb(60, 95, 168, 255);
        public static readonly Color Success = Color.FromArgb(72, 201, 130);
        public static readonly Color Warning = Color.FromArgb(255, 189, 89);
        public static readonly Color Danger = Color.FromArgb(255, 94, 98);
        public static readonly Color ButtonBack = Color.FromArgb(52, 59, 69);
        public static readonly Color ButtonHover = Color.FromArgb(66, 75, 88);
        public static readonly Color ButtonPressed = Color.FromArgb(78, 88, 102);
        public static readonly Color TitleBar = Color.FromArgb(18, 22, 28);
        public static readonly Color InputBack = Color.FromArgb(28, 33, 40);
        public static readonly Color GridMajor = Color.FromArgb(34, 255, 255, 255);
        public static readonly Color GridMinor = Color.FromArgb(16, 255, 255, 255);
        public static readonly Color NodeHeader = Color.FromArgb(28, 33, 40);
        public static readonly Color SocketFill = Color.FromArgb(210, 220, 235);

        public static GraphicsPath CreateRoundRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();

            path.StartFigure();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }

        public static void ApplyTextBoxStyle(TextBox tb)
        {
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.BackColor = InputBack;
            tb.ForeColor = Text;
            tb.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
        }

        public static Label MakeLabel(string text, int x, int y, bool muted = false, bool bold = false, int width = 0)
        {
            var lbl = new Label
            {
                Text = text,
                Left = x,
                Top = y,
                AutoSize = width <= 0,
                ForeColor = muted ? MutedText : Text,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", bold ? 9.75f : 9.25f, bold ? FontStyle.Bold : FontStyle.Regular)
            };

            if (width > 0)
                lbl.Width = width;

            return lbl;
        }
    }

    internal class HudButton : Button
    {
        private bool _hover;
        private bool _pressed;

        public bool AccentStyle { get; set; }

        public HudButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            ForeColor = Theme.Text;
            BackColor = Theme.CardBack;
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            Cursor = Cursors.Hand;
            TabStop = false;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color surfaceBack = Parent?.BackColor ?? Theme.AppBack;

            e.Graphics.Clear(surfaceBack);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color fill = AccentStyle ? Theme.Accent : Theme.ButtonBack;
            Color border = AccentStyle ? Color.FromArgb(120, Theme.Accent) : Theme.Border;
            Color text = Theme.Text;

            if (_hover)
                fill = AccentStyle ? ControlPaint.Light(Theme.Accent, 0.08f) : Theme.ButtonHover;

            if (_pressed)
                fill = AccentStyle ? ControlPaint.Dark(Theme.Accent, 0.08f) : Theme.ButtonPressed;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = Theme.CreateRoundRect(rect, 12);
            using var fillBrush = new SolidBrush(fill);
            using var borderPen = new Pen(border);

            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                rect,
                text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
    }
}
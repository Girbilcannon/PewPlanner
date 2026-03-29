using System;
using System.Drawing;
using System.Windows.Forms;
using PewPlanner.Models;
using PewPlanner.UI;

namespace PewPlanner
{
    public class MainForm : Form
    {
        private const int ResizeBorderSize = 8;
        private const int OuterMargin = 8;
        private const int TitleBarHeight = 56;
        private const int RightPaneWidth = 310;
        private const int FooterHeight = 56;

        private Panel _titleBar = null!;
        private Label _lblTitle = null!;
        private HudButton _btnAddNode = null!;
        private HudButton _btnRecenter = null!;
        private HudButton _btnMinimize = null!;
        private HudButton _btnMaximize = null!;
        private HudButton _btnClose = null!;

        private GraphCanvas _graph = null!;

        private Panel _rightPane = null!;
        private Label _lblPropHeader = null!;
        private Label _lblNodeName = null!;
        private TextBox _txtNodeName = null!;
        private Label _lblNotes = null!;
        private TextBox _txtNotes = null!;
        private Label _lblColor = null!;
        private Panel _pnlColorPreview = null!;
        private HudButton _btnPickColor = null!;

        private HudButton _btnSave = null!;
        private HudButton _btnLoad = null!;
        private HudButton _btnZoomOut = null!;
        private HudButton _btnZoomIn = null!;
        private Button _btnClearGraph = null!;

        private Point _dragStart;
        private bool _updatingProperties;
        private GraphNode? _currentNode;

        public MainForm()
        {
            Text = "PewPlanner";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 700);
            Width = 1400;
            Height = 900;
            BackColor = Theme.AppBack;
            ForeColor = Theme.Text;
            DoubleBuffered = true;
            KeyPreview = true;

            BuildUI();
        }

        private void BuildUI()
        {
            SuspendLayout();

            BuildTitleBar();
            BuildRightPane();
            BuildGraphCanvas();
            BuildFooterButtons();

            ResumeLayout(false);
        }

        private void BuildTitleBar()
        {
            _titleBar = new Panel
            {
                BackColor = Theme.TitleBar
            };

            _titleBar.MouseDown += TitleBar_MouseDown;
            _titleBar.MouseMove += TitleBar_MouseMove;
            _titleBar.DoubleClick += (_, _) => ToggleMaximize();

            _lblTitle = new Label
            {
                Text = "PewPlanner Node Graph",
                Left = 18,
                Top = 16,
                AutoSize = true,
                ForeColor = Theme.Text,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 13.5f, FontStyle.Bold)
            };

            _btnAddNode = new HudButton
            {
                Text = "Add Node +",
                Top = 12,
                Width = 110,
                Height = 32
            };
            _btnAddNode.Click += (_, _) => _graph.AddNode();

            _btnRecenter = new HudButton
            {
                Text = "Recenter",
                Top = 12,
                Width = 96,
                Height = 32
            };
            _btnRecenter.Click += (_, _) => _graph.CenterView();

            _btnMinimize = new HudButton
            {
                Text = "—",
                Width = 38,
                Height = 28,
                Top = 14
            };
            _btnMinimize.Click += (_, _) => WindowState = FormWindowState.Minimized;

            _btnMaximize = new HudButton
            {
                Text = "□",
                Width = 38,
                Height = 28,
                Top = 14
            };
            _btnMaximize.Click += (_, _) => ToggleMaximize();

            _btnClose = new HudButton
            {
                Text = "×",
                Width = 38,
                Height = 28,
                Top = 14
            };
            _btnClose.Click += (_, _) => Close();

            _titleBar.Controls.Add(_lblTitle);
            _titleBar.Controls.Add(_btnAddNode);
            _titleBar.Controls.Add(_btnRecenter);
            _titleBar.Controls.Add(_btnMinimize);
            _titleBar.Controls.Add(_btnMaximize);
            _titleBar.Controls.Add(_btnClose);

            Controls.Add(_titleBar);
        }

        private void BuildRightPane()
        {
            _rightPane = new Panel
            {
                BackColor = Theme.CardBackAlt
            };

            _lblPropHeader = new Label
            {
                Text = "Properties",
                Left = 18,
                Top = 18,
                AutoSize = true,
                ForeColor = Theme.Text,
                Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold)
            };

            _lblNodeName = Theme.MakeLabel("Node Name", 18, 58, true, true);
            _txtNodeName = new TextBox
            {
                Left = 18,
                Top = 80,
                Width = 270
            };
            Theme.ApplyTextBoxStyle(_txtNodeName);
            _txtNodeName.TextChanged += NodeName_TextChanged;

            _lblNotes = Theme.MakeLabel("Notes", 18, 124, true, true);
            _txtNotes = new TextBox
            {
                Left = 18,
                Top = 146,
                Width = 270,
                Height = 220,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            Theme.ApplyTextBoxStyle(_txtNotes);
            _txtNotes.TextChanged += Notes_TextChanged;

            _lblColor = Theme.MakeLabel("Node Color", 18, 384, true, true);

            _pnlColorPreview = new Panel
            {
                Left = 18,
                Top = 408,
                Width = 44,
                Height = 24,
                BackColor = Theme.CardBack
            };
            _pnlColorPreview.Paint += (_, e) =>
            {
                using var pen = new Pen(Theme.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, _pnlColorPreview.Width - 1, _pnlColorPreview.Height - 1);
            };

            _btnPickColor = new HudButton
            {
                Text = "Pick Color",
                Left = 74,
                Top = 403,
                Width = 110,
                Height = 32
            };
            _btnPickColor.Click += (_, _) => PickNodeColor();

            _rightPane.Controls.Add(_lblPropHeader);
            _rightPane.Controls.Add(_lblNodeName);
            _rightPane.Controls.Add(_txtNodeName);
            _rightPane.Controls.Add(_lblNotes);
            _rightPane.Controls.Add(_txtNotes);
            _rightPane.Controls.Add(_lblColor);
            _rightPane.Controls.Add(_pnlColorPreview);
            _rightPane.Controls.Add(_btnPickColor);

            Controls.Add(_rightPane);
        }

        private void BuildGraphCanvas()
        {
            _graph = new GraphCanvas();
            _graph.SelectedNodeChanged += Graph_SelectedNodeChanged;
            Controls.Add(_graph);
        }

        private void BuildFooterButtons()
        {
            _btnSave = new HudButton
            {
                Text = "Save Layout",
                Width = 140,
                Height = 32
            };
            _btnSave.Click += (_, _) => SaveLayout();

            _btnLoad = new HudButton
            {
                Text = "Load Layout",
                Width = 140,
                Height = 32
            };
            _btnLoad.Click += (_, _) => LoadLayout();

            _btnZoomOut = new HudButton
            {
                Text = "-",
                Width = 44,
                Height = 32
            };
            _btnZoomOut.Click += (_, _) => _graph.ZoomOut();

            _btnZoomIn = new HudButton
            {
                Text = "+",
                Width = 44,
                Height = 32
            };
            _btnZoomIn.Click += (_, _) => _graph.ZoomIn();

            _btnClearGraph = new Button
            {
                Text = "Clear Graph",
                Width = 110,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.CardBackAlt,
                ForeColor = Color.FromArgb(220, 90, 90),
                Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnClearGraph.FlatAppearance.BorderColor = Theme.Border;
            _btnClearGraph.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 25, 25);
            _btnClearGraph.FlatAppearance.MouseDownBackColor = Color.FromArgb(55, 30, 30);
            _btnClearGraph.Click += (_, _) => ConfirmClearGraph();

            Controls.Add(_btnSave);
            Controls.Add(_btnLoad);
            Controls.Add(_btnZoomOut);
            Controls.Add(_btnZoomIn);
            Controls.Add(_btnClearGraph);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LayoutCustomUi();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutCustomUi();
        }

        private void LayoutCustomUi()
        {
            if (_titleBar == null || _graph == null || _rightPane == null ||
                _btnSave == null || _btnLoad == null || _btnZoomOut == null ||
                _btnZoomIn == null || _btnClearGraph == null ||
                _btnAddNode == null || _btnRecenter == null ||
                _btnMinimize == null || _btnMaximize == null || _btnClose == null)
            {
                return;
            }

            int contentLeft = OuterMargin;
            int contentTop = OuterMargin;
            int contentWidth = ClientSize.Width - (OuterMargin * 2);
            int contentHeight = ClientSize.Height - (OuterMargin * 2);

            if (contentWidth <= 0 || contentHeight <= 0)
                return;

            _titleBar.SetBounds(
                contentLeft,
                contentTop,
                contentWidth,
                TitleBarHeight);

            _btnAddNode.Left = 255;
            _btnRecenter.Left = 375;

            _btnMinimize.Left = _titleBar.Width - 126;
            _btnMaximize.Left = _titleBar.Width - 84;
            _btnClose.Left = _titleBar.Width - 42;

            int bodyTop = contentTop + TitleBarHeight;
            int footerTop = ClientSize.Height - OuterMargin - 46;
            int bodyHeight = footerTop - bodyTop - 10;

            _rightPane.SetBounds(
                ClientSize.Width - OuterMargin - RightPaneWidth,
                bodyTop,
                RightPaneWidth,
                ClientSize.Height - bodyTop - OuterMargin);

            int graphWidth = _rightPane.Left - contentLeft;
            int graphHeight = bodyHeight;

            _graph.SetBounds(
                contentLeft,
                bodyTop,
                Math.Max(100, graphWidth),
                Math.Max(100, graphHeight));

            _btnSave.Left = contentLeft + 10;
            _btnSave.Top = footerTop;

            _btnLoad.Left = _btnSave.Right + 8;
            _btnLoad.Top = footerTop;

            int graphCenterX = _graph.Left + (_graph.Width / 2);

            _btnZoomOut.Left = graphCenterX - 48;
            _btnZoomOut.Top = footerTop;

            _btnZoomIn.Left = graphCenterX + 4;
            _btnZoomIn.Top = footerTop;

            _btnClearGraph.Left = _rightPane.Left - 118;
            _btnClearGraph.Top = footerTop;
        }

        private void Graph_SelectedNodeChanged(GraphNode? node)
        {
            _currentNode = node;
            _updatingProperties = true;

            bool hasNode = node != null;

            _txtNodeName.Enabled = hasNode;
            _txtNotes.Enabled = hasNode;
            _btnPickColor.Enabled = hasNode;

            if (node == null)
            {
                _txtNodeName.Text = string.Empty;
                _txtNotes.Text = string.Empty;
                _pnlColorPreview.BackColor = Theme.CardBack;
            }
            else
            {
                _txtNodeName.Text = node.Title;
                _txtNotes.Text = node.Notes;
                _pnlColorPreview.BackColor = node.NodeColor;
            }

            _pnlColorPreview.Invalidate();
            _updatingProperties = false;
        }

        private void NodeName_TextChanged(object? sender, EventArgs e)
        {
            if (_updatingProperties || _currentNode == null)
                return;

            _currentNode.Title = _txtNodeName.Text;
            _graph.RefreshSelectedNode();
        }

        private void Notes_TextChanged(object? sender, EventArgs e)
        {
            if (_updatingProperties || _currentNode == null)
                return;

            _currentNode.Notes = _txtNotes.Text;
        }

        private void PickNodeColor()
        {
            if (_currentNode == null)
                return;

            using var dialog = new ColorDialog
            {
                FullOpen = true,
                AnyColor = true,
                SolidColorOnly = false,
                Color = _currentNode.NodeColor
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _currentNode.NodeColor = dialog.Color;
                _pnlColorPreview.BackColor = dialog.Color;
                _pnlColorPreview.Invalidate();
                _graph.RefreshSelectedNode();
            }
        }

        private void SaveLayout()
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "PewPlanner Graph (*.pew)|*.pew",
                DefaultExt = "pew",
                AddExtension = true,
                FileName = "graph.pew"
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
                _graph.SaveToFile(dialog.FileName);
        }

        private void LoadLayout()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "PewPlanner Graph (*.pew)|*.pew",
                DefaultExt = "pew"
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
                _graph.LoadFromFile(dialog.FileName);
        }

        private void ConfirmClearGraph()
        {
            var result = MessageBox.Show(
                this,
                "Clear the entire graph?\n\nThis will remove all nodes and connections.",
                "Clear Graph",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
                _graph.ClearGraph();
        }

        private void ToggleMaximize()
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        }

        private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _dragStart = new Point(e.X, e.Y);
        }

        private void TitleBar_MouseMove(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || WindowState == FormWindowState.Maximized)
                return;

            Left += e.X - _dragStart.X;
            Top += e.Y - _dragStart.Y;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTCLIENT = 1;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;

            if (m.Msg == WM_NCHITTEST && WindowState == FormWindowState.Normal)
            {
                base.WndProc(ref m);

                if ((int)m.Result == HTCLIENT)
                {
                    Point p = PointToClient(GetMousePointFromLParam(m.LParam));

                    bool left = p.X >= 0 && p.X < ResizeBorderSize;
                    bool right = p.X <= ClientSize.Width && p.X > ClientSize.Width - ResizeBorderSize;
                    bool top = p.Y >= 0 && p.Y < ResizeBorderSize;
                    bool bottom = p.Y <= ClientSize.Height && p.Y > ClientSize.Height - ResizeBorderSize;

                    if (left && top)
                        m.Result = (IntPtr)HTTOPLEFT;
                    else if (right && top)
                        m.Result = (IntPtr)HTTOPRIGHT;
                    else if (left && bottom)
                        m.Result = (IntPtr)HTBOTTOMLEFT;
                    else if (right && bottom)
                        m.Result = (IntPtr)HTBOTTOMRIGHT;
                    else if (left)
                        m.Result = (IntPtr)HTLEFT;
                    else if (right)
                        m.Result = (IntPtr)HTRIGHT;
                    else if (top)
                        m.Result = (IntPtr)HTTOP;
                    else if (bottom)
                        m.Result = (IntPtr)HTBOTTOM;
                }

                return;
            }

            base.WndProc(ref m);
        }

        private static Point GetMousePointFromLParam(IntPtr lParam)
        {
            int value = lParam.ToInt32();
            int x = (short)(value & 0xFFFF);
            int y = (short)((value >> 16) & 0xFFFF);
            return new Point(x, y);
        }
    }
}
using System;
using System.Drawing;
using System.Windows.Forms;
using PewPlanner.Models;
using PewPlanner.UI;

namespace PewPlanner
{
    public class MainForm : Form
    {
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
                Left = 0,
                Top = 0,
                Width = ClientSize.Width,
                Height = 56,
                BackColor = Theme.TitleBar,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
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
                Left = 255,
                Top = 12,
                Width = 110,
                Height = 32,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _btnAddNode.Click += (_, _) => _graph.AddNode();

            _btnRecenter = new HudButton
            {
                Text = "Recenter",
                Left = 375,
                Top = 12,
                Width = 96,
                Height = 32,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _btnRecenter.Click += (_, _) => _graph.CenterView();

            _btnMinimize = new HudButton
            {
                Text = "—",
                Width = 38,
                Height = 28,
                Left = ClientSize.Width - 126,
                Top = 14,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnMinimize.Click += (_, _) => WindowState = FormWindowState.Minimized;

            _btnMaximize = new HudButton
            {
                Text = "□",
                Width = 38,
                Height = 28,
                Left = ClientSize.Width - 84,
                Top = 14,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnMaximize.Click += (_, _) => ToggleMaximize();

            _btnClose = new HudButton
            {
                Text = "×",
                Width = 38,
                Height = 28,
                Left = ClientSize.Width - 42,
                Top = 14,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
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
                Width = 310,
                Left = ClientSize.Width - 310,
                Top = 56,
                Height = ClientSize.Height - 56,
                BackColor = Theme.CardBackAlt,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
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
            _graph = new GraphCanvas
            {
                Left = 0,
                Top = 56,
                Width = ClientSize.Width - _rightPane.Width,
                Height = ClientSize.Height - 112,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            _graph.SelectedNodeChanged += Graph_SelectedNodeChanged;

            Controls.Add(_graph);
        }

        private void BuildFooterButtons()
        {
            _btnSave = new HudButton
            {
                Text = "Save Layout",
                Left = 18,
                Top = ClientSize.Height - 46,
                Width = 140,
                Height = 32,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            _btnSave.Click += (_, _) => SaveLayout();

            _btnLoad = new HudButton
            {
                Text = "Load Layout",
                Left = 166,
                Top = ClientSize.Height - 46,
                Width = 140,
                Height = 32,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            _btnLoad.Click += (_, _) => LoadLayout();

            _btnZoomOut = new HudButton
            {
                Text = "-",
                Width = 44,
                Height = 32,
                Top = ClientSize.Height - 46,
                Left = (ClientSize.Width - _rightPane.Width) / 2 - 48,
                Anchor = AnchorStyles.Bottom
            };
            _btnZoomOut.Click += (_, _) => _graph.ZoomOut();

            _btnZoomIn = new HudButton
            {
                Text = "+",
                Width = 44,
                Height = 32,
                Top = ClientSize.Height - 46,
                Left = (ClientSize.Width - _rightPane.Width) / 2 + 4,
                Anchor = AnchorStyles.Bottom
            };
            _btnZoomIn.Click += (_, _) => _graph.ZoomIn();

            _btnClearGraph = new Button
            {
                Text = "Clear Graph",
                Width = 110,
                Height = 32,
                Left = ClientSize.Width - _rightPane.Width - 128,
                Top = ClientSize.Height - 46,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
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
    }
}
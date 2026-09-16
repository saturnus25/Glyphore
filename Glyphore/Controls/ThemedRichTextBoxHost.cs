using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Glyphore;

internal sealed class ThemedRichTextBoxHost : UserControl
{
    private const int EmGetScrollPos = 0x04DD;
    private const int EmSetScrollPos = 0x04DE;

    private readonly TableLayoutPanel _root = new();
    private readonly GlyphScrollBar _vertical = new();
    private readonly GlyphScrollBar _horizontal = new();
    private readonly Panel _corner = new();
    private bool _syncing;

    internal RichTextBox Editor { get; } = new();

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowHorizontalScrollBar { get; set; } = true;

    public ThemedRichTextBoxHost()
    {
        BackColor = Theme.Input;
        Margin = Padding.Empty;
        Padding = Padding.Empty;

        _root.Dock = DockStyle.Fill;
        _root.Margin = Padding.Empty;
        _root.Padding = Padding.Empty;
        _root.ColumnCount = 2;
        _root.RowCount = 2;
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0));
        _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
        _root.BackColor = Theme.Input;

        Editor.Dock = DockStyle.Fill;
        Editor.Margin = Padding.Empty;
        Editor.BorderStyle = BorderStyle.None;
        Editor.ScrollBars = RichTextBoxScrollBars.None;
        Editor.WordWrap = false;
        Editor.BackColor = Theme.Input;
        Editor.ForeColor = Theme.Text;
        Editor.DetectUrls = false;

        _vertical.Dock = DockStyle.Fill;
        _vertical.Margin = Padding.Empty;
        _vertical.BackColor = Theme.Input;
        _vertical.Orientation = Orientation.Vertical;
        _vertical.Visible = false;
        _vertical.ValueChanged += (_, _) => SetScrollPosition(_horizontal.Value, _vertical.Value);

        _horizontal.Dock = DockStyle.Fill;
        _horizontal.Margin = Padding.Empty;
        _horizontal.BackColor = Theme.Input;
        _horizontal.Orientation = Orientation.Horizontal;
        _horizontal.Visible = false;
        _horizontal.ValueChanged += (_, _) => SetScrollPosition(_horizontal.Value, _vertical.Value);

        _corner.Dock = DockStyle.Fill;
        _corner.Margin = Padding.Empty;
        _corner.BackColor = Theme.Input;
        _corner.Visible = false;

        _root.Controls.Add(Editor, 0, 0);
        _root.Controls.Add(_vertical, 1, 0);
        _root.Controls.Add(_horizontal, 0, 1);
        _root.Controls.Add(_corner, 1, 1);
        Controls.Add(_root);

        Editor.TextChanged += (_, _) => BeginRefreshMetrics();
        Editor.SizeChanged += (_, _) => BeginRefreshMetrics();
        Editor.FontChanged += (_, _) => BeginRefreshMetrics();
        Editor.VScroll += (_, _) => SyncBarsFromEditor();
        Editor.HScroll += (_, _) => SyncBarsFromEditor();
        Editor.SelectionChanged += (_, _) => SyncBarsFromEditor();
        Editor.MouseWheel += (_, _) => BeginInvoke((Action)SyncBarsFromEditor);
        SizeChanged += (_, _) => BeginRefreshMetrics();
    }

    internal void RefreshScrollBars() => RefreshMetrics();

    private void BeginRefreshMetrics()
    {
        if (IsDisposed || Disposing) return;
        if (IsHandleCreated)
        {
            BeginInvoke((Action)RefreshMetrics);
            return;
        }
        RefreshMetrics();
    }

    private void RefreshMetrics()
    {
        if (_syncing || IsDisposed || Editor.IsDisposed) return;
        _syncing = true;
        try
        {
            int viewportWidth = Math.Max(1, Editor.ClientSize.Width);
            int viewportHeight = Math.Max(1, Editor.ClientSize.Height);
            string[] lines = Editor.Lines.Length == 0 ? [string.Empty] : Editor.Lines;

            int maxWidth = 0;
            int visualLineCount = 0;
            foreach (string line in lines)
            {
                int width = TextRenderer.MeasureText(
                    line.Length == 0 ? " " : line,
                    Editor.Font,
                    Size.Empty,
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width;
                maxWidth = Math.Max(maxWidth, width);
                visualLineCount += Editor.WordWrap
                    ? Math.Max(1, (int)Math.Ceiling(width / (double)Math.Max(1, viewportWidth - ScaleMetric(8))))
                    : 1;
            }

            int contentWidth = Editor.WordWrap
                ? viewportWidth
                : Math.Max(viewportWidth, maxWidth + ScaleMetric(12));
            int contentHeight = Math.Max(viewportHeight, visualLineCount * Math.Max(1, Editor.Font.Height) + ScaleMetric(10));

            bool verticalVisible = contentHeight > viewportHeight + ScaleMetric(2);
            bool horizontalVisible = ShowHorizontalScrollBar && contentWidth > viewportWidth + ScaleMetric(2);
            int barThickness = ScaleMetric(13);

            _vertical.Visible = verticalVisible;
            _horizontal.Visible = horizontalVisible;
            _corner.Visible = verticalVisible && horizontalVisible;
            _root.ColumnStyles[1].Width = verticalVisible ? barThickness : 0;
            _root.RowStyles[1].Height = horizontalVisible ? barThickness : 0;
            _root.PerformLayout();

            viewportWidth = Math.Max(1, Editor.ClientSize.Width);
            viewportHeight = Math.Max(1, Editor.ClientSize.Height);
            _vertical.SetMetrics(contentHeight, viewportHeight);
            _horizontal.SetMetrics(contentWidth, viewportWidth);
        }
        finally
        {
            _syncing = false;
        }
        SyncBarsFromEditor();
    }

    private void SyncBarsFromEditor()
    {
        if (_syncing || !Editor.IsHandleCreated) return;
        NativePoint point = default;
        SendMessage(Editor.Handle, EmGetScrollPos, IntPtr.Zero, ref point);

        _syncing = true;
        try
        {
            _horizontal.Value = point.X;
            _vertical.Value = point.Y;
        }
        finally
        {
            _syncing = false;
        }
    }

    private void SetScrollPosition(int x, int y)
    {
        if (_syncing || !Editor.IsHandleCreated) return;
        NativePoint point = new() { X = Math.Max(0, x), Y = Math.Max(0, y) };
        SendMessage(Editor.Handle, EmSetScrollPos, IntPtr.Zero, ref point);
        Editor.Invalidate();
        if (IsHandleCreated && !IsDisposed && !Disposing)
            BeginInvoke((Action)SyncBarsFromEditor);
    }

    private int ScaleMetric(int logical)
    {
        int dpi = IsHandleCreated ? Math.Max(96, DeviceDpi) : 96;
        return Math.Max(1, (int)Math.Round(logical * dpi / 96.0));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref NativePoint lParam);
}

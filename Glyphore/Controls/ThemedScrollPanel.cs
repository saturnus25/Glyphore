namespace Glyphore;

internal sealed class ThemedScrollPanel : UserControl, IGlyphWheelScrollHost
{
    private readonly TableLayoutPanel _root = new();
    private readonly Panel _viewport = new();
    private readonly Panel _surface = new();
    private readonly GlyphScrollBar _vertical = new();
    private Control? _content;
    private bool _updating;

    public ThemedScrollPanel()
    {
        BackColor = Theme.Panel;
        TabStop = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

        _root.Dock = DockStyle.Fill;
        _root.Margin = Padding.Empty;
        _root.Padding = Padding.Empty;
        _root.ColumnCount = 2;
        _root.RowCount = 1;
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0));
        _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _root.BackColor = Theme.Panel;

        _viewport.Dock = DockStyle.Fill;
        _viewport.Margin = Padding.Empty;
        _viewport.Padding = Padding.Empty;
        _viewport.BackColor = Theme.Panel;
        _viewport.TabStop = true;
        _viewport.AutoScroll = false;

        _surface.Location = Point.Empty;
        _surface.Margin = Padding.Empty;
        _surface.Padding = Padding.Empty;
        _surface.BackColor = Theme.Panel;

        _vertical.Dock = DockStyle.Fill;
        _vertical.Margin = Padding.Empty;
        _vertical.Orientation = Orientation.Vertical;
        _vertical.Visible = false;
        _vertical.ValueChanged += (_, _) => ApplyScrollPosition();

        _viewport.Controls.Add(_surface);
        _root.Controls.Add(_viewport, 0, 0);
        _root.Controls.Add(_vertical, 1, 0);
        Controls.Add(_root);

        _viewport.SizeChanged += (_, _) => RefreshScrollLayout();
        SizeChanged += (_, _) => RefreshScrollLayout();
        _viewport.MouseWheel += (_, e) => ScrollByWheel(e.Delta);
        MouseWheel += (_, e) => ScrollByWheel(e.Delta);
        BackColorChanged += (_, _) => ApplyBackground();
        ApplyBackground();
    }

    private void ApplyBackground()
    {
        _root.BackColor = BackColor;
        _viewport.BackColor = BackColor;
        _surface.BackColor = BackColor;
        _vertical.BackColor = BackColor;
    }

    internal int ScrollOffset => _vertical.Value;
    internal bool ScrollBarVisible => _vertical.Visible;
    internal int WheelStepPixels = 34;

    public void SetContent(Control content)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (ReferenceEquals(_content, content)) return;

        _surface.Controls.Clear();
        _content = content;
        content.Dock = DockStyle.Top;
        content.Margin = Padding.Empty;
        _surface.Controls.Add(content);
        WireWheelRouting(content);
        content.SizeChanged += ContentSizeChanged;
        content.Layout += ContentLayout;
        RefreshScrollLayout();
    }

    public bool ScrollByWheel(int delta)
    {
        if (!_vertical.Visible || delta == 0) return false;

        int lines = SystemInformation.MouseWheelScrollLines;
        if (lines == 0) return false;
        int logicalStep = lines < 0
            ? Math.Max(1, _viewport.ClientSize.Height)
            : Math.Max(16, WheelStepPixels);
        int pixels = (int)Math.Round(delta / 120.0 * logicalStep);
        if (pixels == 0) pixels = Math.Sign(delta) * Math.Max(1, logicalStep / 4);
        _vertical.Value -= pixels;
        return true;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        Keys key = keyData & Keys.KeyCode;
        if (_vertical.Visible)
        {
            if (key == Keys.PageDown)
            {
                _vertical.Page(1);
                return true;
            }
            if (key == Keys.PageUp)
            {
                _vertical.Page(-1);
                return true;
            }
            if (key == Keys.Home && (keyData & Keys.Control) != 0)
            {
                _vertical.Value = 0;
                return true;
            }
            if (key == Keys.End && (keyData & Keys.Control) != 0)
            {
                _vertical.Value = _vertical.MaximumOffset;
                return true;
            }
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void ContentSizeChanged(object? sender, EventArgs e) => RefreshScrollLayout();
    private void ContentLayout(object? sender, LayoutEventArgs e) => RefreshScrollLayout();

    private void RefreshScrollLayout()
    {
        if (_updating || _content is null || _viewport.ClientSize.Width <= 0 || _viewport.ClientSize.Height <= 0)
            return;

        _updating = true;
        try
        {
            UpdateOnce();
            bool needBar = _surface.Height > _viewport.ClientSize.Height;
            int barWidth = needBar ? ScaleMetric(13) : 0;
            if (_vertical.Visible != needBar || Math.Abs(_root.ColumnStyles[1].Width - barWidth) > 0.1f)
            {
                _vertical.Visible = needBar;
                _root.ColumnStyles[1].Width = barWidth;
                _root.PerformLayout();
                UpdateOnce();
            }

            _vertical.SetMetrics(_surface.Height, _viewport.ClientSize.Height);
            ApplyScrollPosition();
        }
        finally
        {
            _updating = false;
        }
    }

    private void UpdateOnce()
    {
        if (_content is null) return;

        int width = Math.Max(1, _viewport.ClientSize.Width);
        _surface.Width = width;
        _content.Width = width;
        _content.PerformLayout();

        Size preferred = _content.GetPreferredSize(new Size(width, 0));
        int contentHeight = Math.Max(_content.Height, preferred.Height);
        if (!_content.AutoSize && _content.Height < contentHeight)
            _content.Height = contentHeight;

        _surface.Height = Math.Max(contentHeight, _content.Bottom);
    }

    private void ApplyScrollPosition()
    {
        int max = Math.Max(0, _surface.Height - _viewport.ClientSize.Height);
        int y = Math.Clamp(_vertical.Value, 0, max);
        if (_surface.Top != -y) _surface.Top = -y;
        if (_surface.Left != 0) _surface.Left = 0;
    }

    private void WireWheelRouting(Control control)
    {
        control.MouseWheel += ChildMouseWheel;
        control.ControlAdded += ChildControlAdded;
        foreach (Control child in control.Controls)
            WireWheelRouting(child);
    }

    private void ChildMouseWheel(object? sender, MouseEventArgs e)
    {
        // Controls with their own scrolling consume the wheel themselves. Routing the same
        // event to this container made a ListBox move AND the whole inspector move at once.
        if (sender is GlyphSlider or SafeComboBox or GlyphNumericUpDown or ListBox or TextBoxBase or DataGridView) return;
        ScrollByWheel(e.Delta);
    }

    private void ChildControlAdded(object? sender, ControlEventArgs e)
    {
        if (e.Control is Control child)
            WireWheelRouting(child);
    }

    private int ScaleMetric(int logical)
    {
        int dpi = IsHandleCreated ? Math.Max(96, DeviceDpi) : 96;
        return Math.Max(1, (int)Math.Round(logical * dpi / 96.0));
    }
}

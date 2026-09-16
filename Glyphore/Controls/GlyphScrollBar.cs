using System.ComponentModel;

namespace Glyphore;

internal sealed class GlyphScrollBar : Control
{
    private System.Windows.Forms.Orientation _orientation = System.Windows.Forms.Orientation.Vertical;
    private int _contentLength;
    private int _viewportLength;
    private int _value;
    private bool _dragging;
    private bool _hovered;
    private int _dragOffset;

    public event EventHandler? ValueChanged;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public System.Windows.Forms.Orientation Orientation
    {
        get => _orientation;
        set
        {
            if (_orientation == value) return;
            _orientation = value;
            Cursor = Cursors.Default;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Value
    {
        get => _value;
        set
        {
            int next = Math.Clamp(value, 0, MaximumOffset);
            if (_value == next) return;
            _value = next;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int MaximumOffset => Math.Max(0, _contentLength - _viewportLength);

    public GlyphScrollBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        BackColor = Theme.Panel;
        TabStop = false;
        Cursor = Cursors.Default;
    }

    public void SetMetrics(int contentLength, int viewportLength)
    {
        _contentLength = Math.Max(0, contentLength);
        _viewportLength = Math.Max(0, viewportLength);
        int clamped = Math.Clamp(_value, 0, MaximumOffset);
        if (clamped != _value)
        {
            _value = clamped;
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
        Invalidate();
    }

    public void Page(int direction)
    {
        if (direction == 0) return;
        int amount = Math.Max(1, (int)Math.Round(_viewportLength * 0.85));
        Value += Math.Sign(direction) * amount;
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (MaximumOffset > 0 && e.Delta != 0)
        {
            int lines = SystemInformation.MouseWheelScrollLines;
            if (lines < 0)
            {
                Page(e.Delta > 0 ? -1 : 1);
                return;
            }
            if (lines == 0) return;
            int step = Math.Max(18, Font.Height * Math.Max(1, lines));
            int pixels = (int)Math.Round(e.Delta / 120.0 * step);
            if (pixels == 0) pixels = Math.Sign(e.Delta) * Math.Max(1, step / 4);
            Value -= pixels;
            return;
        }
        base.OnMouseWheel(e);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hovered = false;
        if (!_dragging) Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left || MaximumOffset <= 0) return;

        Rectangle thumb = GetThumbRectangle();
        int position = _orientation == System.Windows.Forms.Orientation.Vertical ? e.Y : e.X;
        int thumbStart = _orientation == System.Windows.Forms.Orientation.Vertical ? thumb.Top : thumb.Left;
        int thumbEnd = _orientation == System.Windows.Forms.Orientation.Vertical ? thumb.Bottom : thumb.Right;

        if (position >= thumbStart && position <= thumbEnd)
        {
            _dragging = true;
            _dragOffset = position - thumbStart;
            Capture = true;
            Invalidate();
            return;
        }

        Page(position < thumbStart ? -1 : 1);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_dragging || MaximumOffset <= 0) return;

        int trackLength = _orientation == System.Windows.Forms.Orientation.Vertical ? Height : Width;
        Rectangle thumb = GetThumbRectangle();
        int thumbLength = _orientation == System.Windows.Forms.Orientation.Vertical ? thumb.Height : thumb.Width;
        int available = Math.Max(1, trackLength - thumbLength);
        int position = (_orientation == System.Windows.Forms.Orientation.Vertical ? e.Y : e.X) - _dragOffset;
        double t = Math.Clamp(position / (double)available, 0.0, 1.0);
        Value = (int)Math.Round(t * MaximumOffset);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left) return;
        _dragging = false;
        Capture = false;
        Invalidate();
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);
        if (!Capture && _dragging)
        {
            _dragging = false;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);

        Rectangle track = ClientRectangle;
        if (track.Width <= 0 || track.Height <= 0) return;

        using var trackBrush = new SolidBrush(Theme.PanelRaised);
        e.Graphics.FillRectangle(trackBrush, track);

        if (MaximumOffset <= 0) return;

        Rectangle thumb = GetThumbRectangle();
        Color thumbColor = _dragging
            ? Theme.AccentPressed
            : _hovered ? Theme.Accent : Theme.BorderHot;
        using var thumbBrush = new SolidBrush(thumbColor);
        using var path = Theme.RoundedRect(thumb, Math.Max(2, ScaleMetric(4)));
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.FillPath(thumbBrush, path);
    }

    private Rectangle GetThumbRectangle()
    {
        int trackLength = _orientation == System.Windows.Forms.Orientation.Vertical ? Height : Width;
        int crossLength = _orientation == System.Windows.Forms.Orientation.Vertical ? Width : Height;
        if (trackLength <= 0 || crossLength <= 0 || _contentLength <= 0)
            return Rectangle.Empty;

        double visibleRatio = Math.Clamp(_viewportLength / (double)Math.Max(1, _contentLength), 0.0, 1.0);
        int minThumb = ScaleMetric(26);
        int thumbLength = Math.Clamp((int)Math.Round(trackLength * visibleRatio), Math.Min(minThumb, trackLength), trackLength);
        int available = Math.Max(0, trackLength - thumbLength);
        int start = MaximumOffset <= 0 ? 0 : (int)Math.Round(available * (_value / (double)MaximumOffset));
        int inset = Math.Min(ScaleMetric(2), Math.Max(0, crossLength / 3));

        return _orientation == System.Windows.Forms.Orientation.Vertical
            ? new Rectangle(inset, start, Math.Max(1, crossLength - inset * 2), Math.Max(1, thumbLength))
            : new Rectangle(start, inset, Math.Max(1, thumbLength), Math.Max(1, crossLength - inset * 2));
    }

    private int ScaleMetric(int logical)
    {
        int dpi = IsHandleCreated ? Math.Max(96, DeviceDpi) : 96;
        return Math.Max(1, (int)Math.Round(logical * dpi / 96.0));
    }
}

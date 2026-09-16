namespace Glyphore;

internal sealed class GlyphSlider : Control
{
    private int _value;
    private bool _dragging;

    public event EventHandler? ValueChanged;

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int Minimum { get; set; }

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int Maximum { get; set; } = 1000;


    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool MouseWheelAdjustsValue { get; set; } = false;

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int Value
    {
        get => _value;
        set
        {
            int next = Math.Clamp(value, Minimum, Maximum);
            if (_value == next) return;
            _value = next;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public GlyphSlider()
    {
        Height = 28;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        Cursor = Cursors.Hand;
        BackColor = Theme.Panel;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int left = 7;
        int right = Math.Max(left + 1, Width - 7);
        int cy = Height / 2;
        int trackHeight = 4;
        float t = Maximum == Minimum ? 0f : (Value - Minimum) / (float)(Maximum - Minimum);
        int thumbX = left + (int)Math.Round((right - left) * t);

        using var track = new SolidBrush(Theme.Track);
        using var fill = new SolidBrush(Theme.Accent);
        using var thumb = new SolidBrush(_dragging ? Theme.AccentHot : Theme.Accent);
        using var thumbBorder = new Pen(Theme.AccentHot, 1f);

        e.Graphics.FillRectangle(track, left, cy - trackHeight / 2, right - left, trackHeight);
        e.Graphics.FillRectangle(fill, left, cy - trackHeight / 2, Math.Max(0, thumbX - left), trackHeight);
        e.Graphics.FillEllipse(thumb, thumbX - 6, cy - 6, 12, 12);
        e.Graphics.DrawEllipse(thumbBorder, thumbX - 6, cy - 6, 12, 12);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        _dragging = true;
        Capture = true;
        SetFromMouse(e.X);
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragging) SetFromMouse(e.X);
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
        if (!Capture)
        {
            _dragging = false;
            Invalidate();
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (!MouseWheelAdjustsValue && MouseWheelRouting.Route(this, e.Delta))
            return;

        if (MouseWheelAdjustsValue)
        {
            int step = Math.Max(1, (Maximum - Minimum) / 100);
            Value += Math.Sign(e.Delta) * step;
            return;
        }

        base.OnMouseWheel(e);
    }

    private void SetFromMouse(int x)
    {
        int left = 7;
        int right = Math.Max(left + 1, Width - 7);
        double t = Math.Clamp((x - left) / (double)(right - left), 0, 1);
        Value = Minimum + (int)Math.Round(t * (Maximum - Minimum));
    }
}

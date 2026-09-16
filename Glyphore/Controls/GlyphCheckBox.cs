namespace Glyphore;

internal sealed class GlyphCheckBox : CheckBox
{
    private bool _hover;

    public GlyphCheckBox()
    {
        AutoSize = false;
        FlatStyle = FlatStyle.Flat;
        UseVisualStyleBackColor = false;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnMouseEnter(EventArgs eventargs) { _hover = true; Invalidate(); base.OnMouseEnter(eventargs); }
    protected override void OnMouseLeave(EventArgs eventargs) { _hover = false; Invalidate(); base.OnMouseLeave(eventargs); }
    protected override void OnCheckedChanged(EventArgs e) { Invalidate(); base.OnCheckedChanged(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int boxSize = Math.Min(18, Math.Max(14, Height - 6));
        var boxRect = new Rectangle(1, Math.Max(1, (Height - boxSize) / 2), boxSize, boxSize);
        Color border = Focused || _hover ? Theme.AccentHot : Color.FromArgb(205, 211, 220);
        Color fill = Checked ? Theme.Accent : (_hover ? Theme.PanelHover : Theme.Input);
        if (!Enabled)
        {
            border = Theme.Border;
            fill = Theme.PanelRaised;
        }

        using (var fillBrush = new SolidBrush(fill)) e.Graphics.FillRectangle(fillBrush, boxRect);
        using (var borderPen = new Pen(border, Checked ? 2f : 1.4f)) e.Graphics.DrawRectangle(borderPen, boxRect);

        if (Checked)
        {
            using var tickPen = new Pen(Theme.BrandSurface, 2.4f)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            };
            e.Graphics.DrawLines(tickPen, new[]
            {
                new PointF(boxRect.Left + 4, boxRect.Top + boxSize * .53f),
                new PointF(boxRect.Left + boxSize * .43f, boxRect.Bottom - 4),
                new PointF(boxRect.Right - 3, boxRect.Top + 4)
            });
        }

        var textRect = new Rectangle(boxRect.Right + 8, 0, Math.Max(1, Width - boxRect.Right - 8), Height);
        TextRenderer.DrawText(e.Graphics, Text, Font, textRect, Enabled ? ForeColor : Theme.Muted,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

        if (Focused)
        {
            var focus = textRect;
            focus.Inflate(-1, -3);
            ControlPaint.DrawFocusRectangle(e.Graphics, focus, ForeColor, BackColor);
        }
    }
}

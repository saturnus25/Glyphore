namespace Glyphore;

internal sealed class ThemedGroupBox : GroupBox
{
    public ThemedGroupBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Padding = new Padding(12, 24, 12, 10);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.Clear(BackColor);

        var borderRect = new Rectangle(1, 9, Math.Max(1, Width - 3), Math.Max(1, Height - 11));
        using var path = Theme.RoundedRect(borderRect, 8);
        using var fill = new SolidBrush(Theme.Panel);
        using var border = new Pen(Theme.Border, 1f);
        e.Graphics.FillPath(fill, path);
        e.Graphics.DrawPath(border, path);

        SizeF textSize = e.Graphics.MeasureString(Text, Font);
        var titleRect = new RectangleF(12, 0, textSize.Width + 14, 20);
        using var titleBg = new SolidBrush(Theme.Bg);
        using var titleBrush = new SolidBrush(Theme.AccentText);
        e.Graphics.FillRectangle(titleBg, titleRect);
        e.Graphics.DrawString(Text, Font, titleBrush, 18, 2);
    }

}

namespace Glyphore;

internal sealed class BrandMarkControl : Control
{
    private readonly Image? _brandImage;
    private readonly Icon? _appIcon;

    public BrandMarkControl()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        Size = new Size(44, 44);
        BackColor = Color.Transparent;

        try
        {
            using Stream? stream = typeof(BrandMarkControl).Assembly.GetManifestResourceStream("Glyphore.BrandIcon.png");
            if (stream is not null)
            {
                using Image source = Image.FromStream(stream, useEmbeddedColorManagement: true, validateImageData: true);
                _brandImage = new Bitmap(source);
            }
        }
        catch
        {
            _brandImage = null;
        }

        if (_brandImage is null)
        {
            try { _appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { _appIcon = null; }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

        int side = Math.Max(1, Math.Min(Width, Height) - 2);
        var rect = new Rectangle((Width - side) / 2, (Height - side) / 2, side, side);
        if (_brandImage is not null)
        {
            e.Graphics.DrawImage(_brandImage, rect);
            return;
        }
        if (_appIcon is not null)
        {
            e.Graphics.DrawIcon(_appIcon, rect);
            return;
        }

        using var bg = new SolidBrush(Theme.BrandSurface);
        using var accent = new Pen(Theme.Accent, 3.2f);
        using var path = Theme.RoundedRect(rect, Math.Max(4, rect.Width / 5));
        e.Graphics.FillPath(bg, path);
        e.Graphics.DrawPath(accent, path);
        using var font = new Font("Segoe UI", Math.Max(9f, Height * .45f), FontStyle.Bold, GraphicsUnit.Pixel);
        TextRenderer.DrawText(e.Graphics, "G", font, rect, Theme.AccentText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _brandImage?.Dispose();
            _appIcon?.Dispose();
        }
        base.Dispose(disposing);
    }
}

using System.ComponentModel;

namespace Glyphore;

/// <summary>
/// Common Glyphoré top-level window using the standard Windows non-client frame.
/// Glyphoré owns only the client content; Windows owns the title bar, caption buttons,
/// resize frame, snap, maximize/restore behavior, monitor transitions and DPI handling.
/// </summary>
internal class GlyphoreWindow : Form
{
    private bool _resizable = true;

    /// <summary>
    /// Window-specific UI is hosted here. The native Windows frame/title bar is non-client
    /// area, so this panel can simply fill the complete client area without offsets.
    /// </summary>
    public Panel ContentPanel { get; } = new();

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Resizable
    {
        get => _resizable;
        set
        {
            if (_resizable == value) return;
            _resizable = value;
            ApplyBorderStyle();
        }
    }

    public GlyphoreWindow()
    {
        BackColor = Theme.Bg;
        ForeColor = Theme.Text;
        Font = new Font("Segoe UI", 9f);
        AutoScaleMode = AutoScaleMode.Dpi;
        KeyPreview = true;
        DoubleBuffered = true;

        // Use the real Windows frame. No client-drawn caption, no WM_NCCALCSIZE override,
        // no synthetic resize hit-testing and no DWM border decoration.
        ApplyBorderStyle();

        ContentPanel.Dock = DockStyle.Fill;
        ContentPanel.BackColor = Theme.Bg;
        ContentPanel.Margin = Padding.Empty;
        ContentPanel.Padding = Padding.Empty;
        base.Controls.Add(ContentPanel);
    }

    private void ApplyBorderStyle()
    {
        FormBorderStyle desired = _resizable
            ? FormBorderStyle.Sizable
            : FormBorderStyle.FixedSingle;

        if (FormBorderStyle != desired)
            FormBorderStyle = desired;
    }
}

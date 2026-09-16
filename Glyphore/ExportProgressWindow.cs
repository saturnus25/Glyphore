namespace Glyphore;

internal sealed class ExportProgressWindow : GlyphoreWindow
{
    private readonly Label _stage = new();
    private readonly Label _detail = new();
    private readonly Label _percent = new();
    private readonly Panel _track = new();
    private readonly Panel _fill = new();
    private int _progress;

    public ExportProgressWindow()
    {
        Text = Localization.English ? "Exporting · Glyphoré" : "Exportando · Glyphoré";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        ControlBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(500, 142);
        BackColor = Theme.Panel;
        ForeColor = Theme.Text;
        Padding = Padding.Empty;

        _stage.SetBounds(18, 16, 390, 24);
        _stage.Font = new Font(Font, FontStyle.Bold);
        _stage.ForeColor = Theme.Text;

        _percent.SetBounds(412, 16, 70, 24);
        _percent.TextAlign = ContentAlignment.MiddleRight;
        _percent.ForeColor = Theme.AccentText;
        _percent.Font = new Font(Font, FontStyle.Bold);

        _detail.SetBounds(18, 45, 464, 22);
        _detail.ForeColor = Theme.Muted;

        _track.SetBounds(18, 82, 464, 18);
        _track.BackColor = Theme.Track;
        _track.Padding = new Padding(1);
        _fill.BackColor = Theme.Accent;
        _fill.SetBounds(1, 1, 0, 16);
        _track.Controls.Add(_fill);

        ContentPanel.BackColor = Theme.Panel;
        ContentPanel.Controls.AddRange([_stage, _percent, _detail, _track]);
        SetProgress(0,
            Localization.English ? "Preparing export…" : "Preparando exportación…",
            Localization.English ? "Capturing the current scene state." : "Capturando el estado actual de la escena.");
    }

    public void SetProgress(int percent, string stage, string detail = "")
    {
        _progress = Math.Clamp(percent, 0, 100);
        _stage.Text = stage;
        _detail.Text = detail;
        _percent.Text = $"{_progress}%";
        ResizeFill();
        _track.Invalidate();
        _stage.Update();
        _detail.Update();
        _percent.Update();
        _track.Update();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ResizeFill();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ResizeFill();
    }

    private void ResizeFill()
    {
        int width = Math.Max(0, (int)Math.Round((_track.ClientSize.Width - 2) * (_progress / 100.0)));
        _fill.SetBounds(1, 1, width, Math.Max(1, _track.ClientSize.Height - 2));
    }
}

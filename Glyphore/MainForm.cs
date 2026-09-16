using System.Diagnostics;

namespace Glyphore;

internal sealed partial class MainForm : GlyphoreWindow
{
    private readonly AppData _data = AppData.Load();
    private readonly EffectSettings _settings = new();
    private readonly FlowLayoutPanel _left = new();
    private readonly GlPreviewControl _preview = new();
    private readonly Label _status = new();
    private readonly RichTextBox _importView = new();
    private readonly System.Windows.Forms.Timer _importTimer = new() { Interval = 5 };
    private readonly Stopwatch _importClock = new();
    private List<string> _importFrames = [];
    private double _importFps = 20;
    private int _importIndex = -1;
    private readonly SafeComboBox _language = new();
    private readonly SafeComboBox _effect = new();
    private readonly SafeComboBox _preset = new();
    private readonly SafeComboBox _charsetPreset = new();
    private readonly SafeComboBox _palette = new();
    private readonly SafeComboBox _shape = new();
    private readonly TextBox _charset = new();
    private readonly GlyphNumericUpDown _width = new(), _height = new(), _fps = new(), _duration = new(), _seed = new(), _glyphSize = new();
    private readonly GlyphCheckBox _invert = new(), _color = new(), _exportCredit = new();
    private readonly Button _exportButton = new GlyphButton();
    private readonly Panel _generalParams = new(), _specificParams = new(), _paletteStops = new();
    private readonly Dictionary<string, ParameterRow> _paramRows = new(StringComparer.OrdinalIgnoreCase);
    private bool _applying;
    private bool _exporting;
    private readonly Button _pauseButton = new GlyphButton();
    private readonly ToolTip _tips = new() { InitialDelay = 500, ReshowDelay = 100, AutoPopDelay = 10000, ShowAlways = true };
    private Icon? _windowIcon;
    private readonly DetachedWindowManager _detachedWindows;

    public MainForm()
    {
        _detachedWindows = new DetachedWindowManager(this);
        _discordPresence = new DiscordRichPresenceService(_preferences.DiscordRichPresenceEnabled);
        _discordActivityFilter = new GlyphoreActivityMessageFilter(_discordPresence);

        Text = "Glyphoré 6.0.0";
        Width = 1640;
        Height = 980;
        MinimumSize = new Size(1240, 760);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 9f);
        BackColor = Theme.Bg;
        ForeColor = Theme.Text;
        StartPosition = FormStartPosition.CenterScreen;
        ShowIcon = true;
        _windowIcon = LoadApplicationIcon();
        if (_windowIcon is not null) Icon = _windowIcon;
        FormClosed += (_, _) =>
        {
            _detachedWindows.Dispose();
            _windowIcon?.Dispose();
        };

        BuildUi();
        PopulateData();
        ApplyPreset(_preset.Items.Count > 0 ? _preset.Items[0]!.ToString()! : "");
        InitializeSceneFromCurrentSettings();
        InitializeHistory();

        _preview.Settings = _settings;
        _preview.Scene = _scene;
        _preview.TargetFps = _settings.Fps;
        _preview.FrameStats += (fps, ms, gpu) => BeginInvoke((Action)(() =>
        {
            string renderer = ShortRendererName(gpu);
            bool hasCamera = Camera3D.TryGetSpec(_settings.Effect, out _);
            string cameraHint = hasCamera
                ? (Localization.English ? " · drag: orbit · wheel: zoom" : " · arrastra: cámara · rueda: zoom")
                : "";
            string layerInfo = _sceneReady ? $" · {_scene.Layers.Count}L" : "";
            _status.Text = $"{_settings.Effect} / {_settings.Preset} · {_settings.Width}×{_settings.Height} · " +
                           $"{fps:0.0}/{_settings.Fps} FPS · {ms:0.00} ms · {renderer}{layerInfo}{cameraHint}";
            string details = Localization.English
                ? $"Direct OpenGL preview\nGPU: {gpu}\nASCII grid: {_settings.Width}×{_settings.Height}\nPreview: {_preview.Width}×{_preview.Height} px\nTarget: {_settings.Fps} FPS"
                : $"Preview OpenGL directa\nGPU: {gpu}\nRejilla ASCII: {_settings.Width}×{_settings.Height}\nPreview: {_preview.Width}×{_preview.Height} px\nObjetivo: {_settings.Fps} FPS";
            if (hasCamera) details += Localization.English
                ? "\nCamera: drag to orbit, mouse wheel to zoom"
                : "\nCámara: arrastra para moverla, rueda para zoom";
            if (_tips.GetToolTip(_status) != details) _tips.SetToolTip(_status, details);
        }));
        _preview.CameraChanged += HandleCameraChanged;
        _preview.MaskEdited += HandlePreviewMaskEdited;
        _preview.MaskDeleteRequested += HandlePreviewMaskDelete;

        InitializeDiscordPresence();
        UpdateDiscordPresenceContext();
    }

    private static Icon? LoadApplicationIcon()
    {
        try
        {
            using Stream? stream = typeof(MainForm).Assembly.GetManifestResourceStream("Glyphore.AppIcon.ico");
            if (stream is not null)
            {
                using var source = new Icon(stream);
                return (Icon)source.Clone();
            }

            return Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
            return null;
        }
    }

    private static string ShortRendererName(string rendererInfo)
    {
        if (string.IsNullOrWhiteSpace(rendererInfo)) return "GPU";
        if (rendererInfo.StartsWith("OpenGL error", StringComparison.OrdinalIgnoreCase) ||
            rendererInfo.StartsWith("Render error", StringComparison.OrdinalIgnoreCase))
            return rendererInfo;

        string renderer = rendererInfo.Split('·', 2)[0].Trim();
        int suffix = renderer.IndexOf("/PCIe", StringComparison.OrdinalIgnoreCase);
        if (suffix >= 0) renderer = renderer[..suffix].Trim();
        renderer = renderer.Replace("NVIDIA GeForce ", "", StringComparison.OrdinalIgnoreCase);
        renderer = renderer.Replace("AMD Radeon ", "Radeon ", StringComparison.OrdinalIgnoreCase);
        return renderer.Length <= 28 ? renderer : renderer[..25].TrimEnd() + "…";
    }
}

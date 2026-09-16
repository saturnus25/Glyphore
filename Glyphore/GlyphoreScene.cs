using System.Text.Json.Serialization;

namespace Glyphore;

internal sealed class GlyphoreScene
{
    public const int CurrentFormatVersion = 9;
    public const int MaxLayers = 128;

    public int FormatVersion { get; set; } = CurrentFormatVersion;
    public string Name { get; set; } = "Untitled";
    public int Width { get; set; } = 178;
    public int Height { get; set; } = 50;
    public int Fps { get; set; } = 30;
    public double Duration { get; set; } = 6.0;
    public string CharsetName { get; set; } = "Classic";
    public string Charset { get; set; } = " .:-=+*#%@";
    public string PaletteName { get; set; } = "Plasma";
    public List<string> PaletteStops { get; set; } = new() { "#090122", "#3d0f7d", "#9c179e", "#e84a8a", "#ffad5a", "#fff0b6" };
    public string BackgroundColor { get; set; } = "#000000";
    public bool ColorEnabled { get; set; } = true;
    public bool IncludeExportCredit { get; set; } = true;
    public bool Invert { get; set; }
    public double GlyphDisplayScale { get; set; } = 1.90;
    public double Gamma { get; set; } = 1.0;
    public Guid? ActiveLayerId { get; set; }
    public bool RespectLayerOrder { get; set; } = true;
    public SceneTransform Transform { get; set; } = new();
    public ScenePostProcess PostProcess { get; set; } = new();
    public List<SceneEffectLayer> Layers { get; set; } = [];

    private int _commonRevision;

    [JsonIgnore]
    internal int CommonRevision => System.Threading.Volatile.Read(ref _commonRevision);

    [JsonIgnore]
    internal object SyncRoot { get; } = new();

    [JsonIgnore]
    public SceneEffectLayer? ActiveLayer => ActiveLayerId is Guid id
        ? Layers.FirstOrDefault(layer => layer.Id == id)
        : null;

    public static GlyphoreScene FromSettings(EffectSettings settings)
    {
        var scene = new GlyphoreScene();
        scene.CaptureCommonFrom(settings);
        var layer = SceneEffectLayer.FromSettings(settings);
        scene.Layers.Add(layer);
        scene.ActiveLayerId = layer.Id;
        return scene;
    }

    public void CaptureCommonFrom(EffectSettings settings)
    {
        lock (SyncRoot)
        {
            double gamma = settings.Get("gamma");
            bool changed =
                Width != settings.Width ||
                Height != settings.Height ||
                Fps != settings.Fps ||
                Duration != settings.Duration ||
                !CharsetName.Equals(settings.CharsetName, StringComparison.Ordinal) ||
                !Charset.Equals(settings.Charset, StringComparison.Ordinal) ||
                !PaletteName.Equals(settings.PaletteName, StringComparison.Ordinal) ||
                !PaletteStops.SequenceEqual(settings.PaletteStops, StringComparer.OrdinalIgnoreCase) ||
                ColorEnabled != settings.ColorEnabled ||
                IncludeExportCredit != settings.IncludeExportCredit ||
                Invert != settings.Invert ||
                GlyphDisplayScale != settings.GlyphDisplayScale ||
                Gamma != gamma;

            Width = settings.Width;
            Height = settings.Height;
            Fps = settings.Fps;
            Duration = settings.Duration;
            CharsetName = settings.CharsetName;
            Charset = settings.Charset;
            PaletteName = settings.PaletteName;
            PaletteStops = new List<string>(settings.PaletteStops);
            ColorEnabled = settings.ColorEnabled;
            IncludeExportCredit = settings.IncludeExportCredit;
            Invert = settings.Invert;
            GlyphDisplayScale = settings.GlyphDisplayScale;
            Gamma = gamma;

            if (changed) unchecked { _commonRevision++; }
        }
    }

    public EffectSettings CreateSettings(SceneEffectLayer layer)
        => CreateSettings(layer, out _, out _);

    internal EffectSettings CreateSettings(
        SceneEffectLayer layer,
        out int commonRevision,
        out int layerRevision)
    {
        // Always capture the settings and the revisions under the same locks. Without
        // this, the render thread can build an old snapshot, observe a newer revision
        // afterwards, and cache the old effect forever as if it were current.
        lock (SyncRoot)
        lock (layer.SyncRoot)
        {
            var settings = new EffectSettings
            {
                Width = Width,
                Height = Height,
                Fps = Fps,
                Duration = Duration,
                Seed = layer.Seed,
                Effect = layer.Effect,
                Preset = layer.Preset,
                CharsetName = layer.CharsetName,
                Charset = string.IsNullOrEmpty(layer.Charset) ? " " : layer.Charset,
                PaletteName = layer.PaletteName,
                PaletteStops = new List<string>(layer.PaletteStops),
                ColorEnabled = ColorEnabled,
                IncludeExportCredit = IncludeExportCredit,
                Invert = Invert,
                GlyphDisplayScale = GlyphDisplayScale,
                ShapeMode = layer.ShapeMode
            };

            settings.V.Clear();
            foreach (var pair in ParameterCatalog.Defaults)
                settings.V[pair.Key] = pair.Value;
            foreach (var pair in layer.Values)
                settings.V[pair.Key] = pair.Value;
            settings.Set("gamma", Gamma);

            commonRevision = _commonRevision;
            layerRevision = layer.SettingsRevision;
            return settings;
        }
    }

    public SceneEffectLayer EnsureActiveLayer(EffectSettings fallback)
    {
        var active = ActiveLayer;
        if (active is not null) return active;

        if (Layers.Count == 0)
            Layers.Add(SceneEffectLayer.FromSettings(fallback));

        ActiveLayerId = Layers[0].Id;
        return Layers[0];
    }

    public GlyphoreScene Clone()
    {
        return new GlyphoreScene
        {
            FormatVersion = FormatVersion,
            Name = Name,
            Width = Width,
            Height = Height,
            Fps = Fps,
            Duration = Duration,
            CharsetName = CharsetName,
            Charset = Charset,
            PaletteName = PaletteName,
            PaletteStops = new List<string>(PaletteStops),
            BackgroundColor = BackgroundColor,
            ColorEnabled = ColorEnabled,
            IncludeExportCredit = IncludeExportCredit,
            Invert = Invert,
            GlyphDisplayScale = GlyphDisplayScale,
            Gamma = Gamma,
            ActiveLayerId = ActiveLayerId,
            RespectLayerOrder = RespectLayerOrder,
            Transform = Transform.Clone(),
            PostProcess = PostProcess.Clone(),
            Layers = Layers.Select(layer => layer.Clone()).ToList(),
            _commonRevision = CommonRevision
        };
    }
}

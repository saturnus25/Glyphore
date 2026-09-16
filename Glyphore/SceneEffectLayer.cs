using System.Text.Json.Serialization;

namespace Glyphore;

internal sealed class SceneEffectLayer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Effect";
    public bool Visible { get; set; } = true;
    public double Opacity { get; set; } = 1.0;
    public LayerBlendMode BlendMode { get; set; } = LayerBlendMode.Normal;
    public int Seed { get; set; } = 1337;
    public string Effect { get; set; } = "Plasma";
    public string Preset { get; set; } = "Classic Plasma";
    // Remembers which preset a customized layer started from. Preset may become Custom,
    // but SourcePreset survives save/load so the editor never loses that reference.
    public string SourcePreset { get; set; } = string.Empty;
    public string ShapeMode { get; set; } = "Square";
    public string CharsetName { get; set; } = "Classic";
    public string Charset { get; set; } = " .:-=+*#%@";
    public string PaletteName { get; set; } = "Plasma";
    public List<string> PaletteStops { get; set; } = new() { "#090122", "#3d0f7d", "#9c179e", "#e84a8a", "#ffad5a", "#fff0b6" };
    public Dictionary<string, double> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<SceneLayerMask> Masks { get; set; } = [];

    // ASCII Title Studio metadata. Numeric styling/animation controls live in Values so they
    // participate in presets and the ordinary parameter pipeline.
    public string TitleText { get; set; } = "GLYPHORÉ";
    public string TitlePrefab { get; set; } = "System Font";
    public string TitleFont { get; set; } = "Consolas";
    public bool TitleBold { get; set; } = true;
    public bool TitleItalic { get; set; }
    public bool TitleAnimate { get; set; } = true;

    private int _settingsRevision;

    [JsonIgnore]
    internal int SettingsRevision => System.Threading.Volatile.Read(ref _settingsRevision);

    [JsonIgnore]
    internal object SyncRoot { get; } = new();

    public static SceneEffectLayer FromSettings(EffectSettings settings, string? name = null)
    {
        var layer = new SceneEffectLayer
        {
            Name = string.IsNullOrWhiteSpace(name) ? settings.Effect : name,
            Seed = settings.Seed,
            Effect = settings.Effect,
            Preset = settings.Preset,
            SourcePreset = settings.Preset.Equals("Custom", StringComparison.OrdinalIgnoreCase) ? string.Empty : settings.Preset,
            ShapeMode = settings.ShapeMode,
            CharsetName = settings.CharsetName,
            Charset = string.IsNullOrEmpty(settings.Charset) ? " " : settings.Charset,
            PaletteName = settings.PaletteName,
            PaletteStops = new List<string>(settings.PaletteStops)
        };
        layer.CaptureFrom(settings);
        return layer;
    }

    public void CaptureFrom(EffectSettings settings)
    {
        // The OpenGL scheduler reads layer state from a different thread. Keep the
        // render snapshot atomic so a palette-only update cannot be paired with stale
        // effect/preset values and then cached under the new revision.
        var values = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in settings.V)
        {
            // Gamma belongs to the final character mapping, not to one procedural layer.
            if (!pair.Key.Equals("gamma", StringComparison.OrdinalIgnoreCase))
                values[pair.Key] = pair.Value;
        }

        lock (SyncRoot)
        {
            Seed = settings.Seed;
            Effect = settings.Effect;
            Preset = settings.Preset;
            if (!settings.Preset.Equals("Custom", StringComparison.OrdinalIgnoreCase))
                SourcePreset = settings.Preset;
            ShapeMode = settings.ShapeMode;
            CharsetName = settings.CharsetName;
            Charset = string.IsNullOrEmpty(settings.Charset) ? " " : settings.Charset;
            PaletteName = settings.PaletteName;
            PaletteStops = new List<string>(settings.PaletteStops);
            Values = values;
            unchecked { _settingsRevision++; }
        }
    }

    public SceneEffectLayer Clone()
    {
        return new SceneEffectLayer
        {
            Id = Id,
            Name = Name,
            Visible = Visible,
            Opacity = Opacity,
            BlendMode = BlendMode,
            Seed = Seed,
            Effect = Effect,
            Preset = Preset,
            SourcePreset = SourcePreset,
            ShapeMode = ShapeMode,
            CharsetName = CharsetName,
            Charset = Charset,
            PaletteName = PaletteName,
            PaletteStops = new List<string>(PaletteStops),
            Values = new Dictionary<string, double>(Values, StringComparer.OrdinalIgnoreCase),
            Masks = Masks.Select(mask => mask.Clone()).ToList(),
            TitleText = TitleText,
            TitlePrefab = TitlePrefab,
            TitleFont = TitleFont,
            TitleBold = TitleBold,
            TitleItalic = TitleItalic,
            TitleAnimate = TitleAnimate,
            _settingsRevision = SettingsRevision
        };
    }

    public SceneEffectLayer Duplicate(string name)
    {
        var clone = Clone();
        clone.Id = Guid.NewGuid();
        clone.Name = name;
        return clone;
    }

    public void Touch()
    {
        lock (SyncRoot)
            unchecked { _settingsRevision++; }
    }

    public void SetPalette(string name, IEnumerable<string> stops)
    {
        var nextStops = stops.Take(8).ToList();
        if (nextStops.Count < 2) nextStops = ["#cccccc", "#ffffff"];
        lock (SyncRoot)
        {
            PaletteName = string.IsNullOrWhiteSpace(name) ? "Custom" : name;
            PaletteStops = nextStops;
            unchecked { _settingsRevision++; }
        }
    }

    public void SetCharset(string name, string charset)
    {
        lock (SyncRoot)
        {
            CharsetName = string.IsNullOrWhiteSpace(name) ? "Custom" : name;
            Charset = string.IsNullOrEmpty(charset) ? " " : charset;
            unchecked { _settingsRevision++; }
        }
    }

    public override string ToString() => $"{(Visible ? "●" : "○")} {Name}";
}

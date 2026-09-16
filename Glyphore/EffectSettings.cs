namespace Glyphore;

internal sealed class EffectSettings
{
    public int Width { get; set; } = 178;
    public int Height { get; set; } = 50;
    public int Fps { get; set; } = 30;
    public double Duration { get; set; } = 6.0;
    public int Seed { get; set; } = 1337;
    public string Effect { get; set; } = "Plasma";
    public string Preset { get; set; } = "Classic Plasma";
    public string CharsetName { get; set; } = "Classic";
    public string Charset { get; set; } = " .:-=+*#%@";
    public string PaletteName { get; set; } = "Plasma";
    public List<string> PaletteStops { get; set; } = new() { "#090122", "#3d0f7d", "#9c179e", "#e84a8a", "#ffad5a", "#fff0b6" };
    public bool ColorEnabled { get; set; } = true;
    public bool IncludeExportCredit { get; set; } = true;
    public bool Invert { get; set; }
    // Preview-only glyph magnification. 1.15 closely matches the compact cell fill
    // users expect from Windows Terminal / PowerShell / cmd while remaining adjustable.
    public double GlyphDisplayScale { get; set; } = 1.90;
    public string ShapeMode { get; set; } = "Square";

    public Dictionary<string, double> V { get; } = new(StringComparer.OrdinalIgnoreCase);

    public EffectSettings() => ResetValues();

    public void ResetValues()
    {
        V.Clear();
        foreach (var kv in ParameterCatalog.Defaults) V[kv.Key] = kv.Value;
    }

    public double Get(string key) => V.TryGetValue(key, out var value) ? value : ParameterCatalog.Defaults.GetValueOrDefault(key, 0.0);
    public float F(string key) => (float)Get(key);
    public void Set(string key, double value) => V[key] = value;

    public void CopyFrom(EffectSettings source)
    {
        Width = source.Width;
        Height = source.Height;
        Fps = source.Fps;
        Duration = source.Duration;
        Seed = source.Seed;
        Effect = source.Effect;
        Preset = source.Preset;
        CharsetName = source.CharsetName;
        Charset = source.Charset;
        PaletteName = source.PaletteName;
        PaletteStops = new List<string>(source.PaletteStops);
        ColorEnabled = source.ColorEnabled;
        IncludeExportCredit = source.IncludeExportCredit;
        Invert = source.Invert;
        GlyphDisplayScale = source.GlyphDisplayScale;
        ShapeMode = source.ShapeMode;
        V.Clear();
        foreach (var kv in source.V) V[kv.Key] = kv.Value;
    }

    public EffectSettings Clone()
    {
        var x = new EffectSettings
        {
            Width = Width, Height = Height, Fps = Fps, Duration = Duration, Seed = Seed,
            Effect = Effect, Preset = Preset, CharsetName = CharsetName, Charset = Charset,
            PaletteName = PaletteName, PaletteStops = new List<string>(PaletteStops),
            ColorEnabled = ColorEnabled, IncludeExportCredit = IncludeExportCredit, Invert = Invert,
            GlyphDisplayScale = GlyphDisplayScale, ShapeMode = ShapeMode
        };
        x.V.Clear();
        foreach (var kv in V) x.V[kv.Key] = kv.Value;
        return x;
    }
}

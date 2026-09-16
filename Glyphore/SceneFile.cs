using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glyphore;

internal static class SceneFile
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static string NormalizeHtmlColor(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        try
        {
            Color color = ColorTranslator.FromHtml(value);
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        catch { return fallback; }
    }

    public static void Save(string path, GlyphoreScene scene)
    {
        if (scene.Layers is null || scene.Layers.Count == 0)
            throw new InvalidDataException("A Glyphoré scene must contain at least one layer.");
        if (scene.Layers.Count > GlyphoreScene.MaxLayers)
            throw new InvalidDataException($"A Glyphoré scene can contain at most {GlyphoreScene.MaxLayers} layers.");

        scene.FormatVersion = GlyphoreScene.CurrentFormatVersion;
        File.WriteAllText(path, JsonSerializer.Serialize(scene, Options), new System.Text.UTF8Encoding(false));
    }

    public static GlyphoreScene Load(string path)
    {
        var info = new FileInfo(path);
        if (info.Length > 16 * 1024 * 1024)
            throw new InvalidDataException("The Glyphoré scene is too large to load safely.");

        GlyphoreScene? scene;
        try
        {
            scene = JsonSerializer.Deserialize<GlyphoreScene>(File.ReadAllText(path), Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("The file is not a valid Glyphoré scene.", ex);
        }

        if (scene is null)
            throw new InvalidDataException("The Glyphoré scene is empty.");
        if (scene.FormatVersion < 1 || scene.FormatVersion > GlyphoreScene.CurrentFormatVersion)
            throw new InvalidDataException($"Unsupported Glyphoré scene format version: {scene.FormatVersion}.");
        int loadedFormatVersion = scene.FormatVersion;
        if (scene.Layers is null || scene.Layers.Count == 0)
            throw new InvalidDataException("The Glyphoré scene does not contain any layers.");
        if (scene.Layers.Count > GlyphoreScene.MaxLayers)
            throw new InvalidDataException($"The Glyphoré scene contains too many layers (maximum {GlyphoreScene.MaxLayers}).");

        scene.Width = Math.Max(1, scene.Width);
        scene.Height = Math.Max(1, scene.Height);
        scene.Fps = Math.Max(1, scene.Fps);
        scene.Duration = Math.Max(0.001, scene.Duration);
        scene.GlyphDisplayScale = Math.Clamp(scene.GlyphDisplayScale, 0.70, 4.00);
        scene.Gamma = Math.Clamp(scene.Gamma, 0.05, 8.0);
        scene.Transform ??= new SceneTransform();
        scene.Transform.Clamp();
        scene.PostProcess ??= new ScenePostProcess();
        scene.PostProcess.Clamp();
        scene.Name = string.IsNullOrWhiteSpace(scene.Name) ? "Untitled" : scene.Name.Trim();
        scene.CharsetName = string.IsNullOrWhiteSpace(scene.CharsetName) ? "Custom" : scene.CharsetName;
        scene.Charset = string.IsNullOrEmpty(scene.Charset) ? " " : scene.Charset;
        scene.PaletteName = string.IsNullOrWhiteSpace(scene.PaletteName) ? "Custom" : scene.PaletteName;
        scene.PaletteStops ??= [];
        if (scene.PaletteStops.Count < 2)
            scene.PaletteStops = ["#cccccc", "#ffffff"];
        scene.BackgroundColor = NormalizeHtmlColor(scene.BackgroundColor, "#000000");

        var usedIds = new HashSet<Guid>();
        foreach (var layer in scene.Layers)
        {
            if (layer.Id == Guid.Empty || !usedIds.Add(layer.Id))
            {
                layer.Id = Guid.NewGuid();
                usedIds.Add(layer.Id);
            }
            layer.Name = string.IsNullOrWhiteSpace(layer.Name) ? layer.Effect : layer.Name.Trim();
            layer.Effect = string.IsNullOrWhiteSpace(layer.Effect) ? "Plasma" : layer.Effect;
            layer.Preset = string.IsNullOrWhiteSpace(layer.Preset) ? "Custom" : layer.Preset;
            layer.SourcePreset = string.IsNullOrWhiteSpace(layer.SourcePreset) || layer.SourcePreset.Equals("Custom", StringComparison.OrdinalIgnoreCase)
                ? (layer.Preset.Equals("Custom", StringComparison.OrdinalIgnoreCase) ? string.Empty : layer.Preset)
                : layer.SourcePreset.Trim();
            layer.ShapeMode = string.IsNullOrWhiteSpace(layer.ShapeMode) ? "Square" : layer.ShapeMode;
            layer.Opacity = Math.Clamp(layer.Opacity, 0.0, 1.0);
            if (loadedFormatVersion < 2)
            {
                layer.PaletteName = scene.PaletteName;
                layer.PaletteStops = new List<string>(scene.PaletteStops);
            }
            if (loadedFormatVersion < 3)
            {
                layer.CharsetName = scene.CharsetName;
                layer.Charset = scene.Charset;
            }
            layer.CharsetName = string.IsNullOrWhiteSpace(layer.CharsetName) ? "Custom" : layer.CharsetName;
            layer.Charset = string.IsNullOrEmpty(layer.Charset) ? " " : layer.Charset;
            layer.TitleText = string.IsNullOrEmpty(layer.TitleText) ? "GLYPHORÉ" : layer.TitleText[..Math.Min(layer.TitleText.Length, 512)];
            if (loadedFormatVersion < 6 && layer.Effect.Equals("ASCII Title", StringComparison.OrdinalIgnoreCase))
            {
                // v5 used Preset for what the Title Studio called a “prefab”. In v6 the
                // two concepts are separate: TitlePrefab defines letter construction, while
                // Preset is a visual/animation style. Existing scenes keep their numeric
                // Values, so mapping Preset to Custom is lossless.
                layer.TitlePrefab = layer.Preset switch
                {
                    "Neon Blocks" => "Classic Block",
                    "Crystal Motion" => "ANSI Shadow",
                    "Retro Foodtruck" => "Slant",
                    "Dot Matrix" => "Dot Matrix",
                    "Cyber Console" => "Wireframe",
                    "Hologram Wire" => "Oldschool Outline",
                    "Ice Glass" => "Modular",
                    "Terminal Banner" => "Classic Block",
                    "Typewriter" => "System Font",
                    "Glitch Sign" => "Newschool Dollar",
                    _ => "System Font"
                };
                if (layer.Preset is "Neon Blocks" or "Crystal Motion" or "Retro Foodtruck" or "Dot Matrix" or "Cyber Console" or "Hologram Wire" or "Ice Glass" or "Terminal Banner" or "Glitch Sign")
                    layer.Preset = "Custom";
            }
            layer.TitlePrefab = string.IsNullOrWhiteSpace(layer.TitlePrefab) ? "System Font" : layer.TitlePrefab.Trim();
            layer.TitlePrefab = layer.TitlePrefab switch
            {
                "Block Grid" => "Classic Block",
                "Outline Grid" => "Oldschool Outline",
                "Slanted Banner" => "Slant",
                "Terminal Wide" => "Classic Block",
                "Retro Modular" => "Modular",
                _ => layer.TitlePrefab
            };
            layer.TitleFont = string.IsNullOrWhiteSpace(layer.TitleFont) ? "Consolas" : layer.TitleFont.Trim();
            layer.PaletteName = string.IsNullOrWhiteSpace(layer.PaletteName) ? "Custom" : layer.PaletteName;
            layer.PaletteStops ??= [];
            if (layer.PaletteStops.Count < 2)
                layer.PaletteStops = ["#cccccc", "#ffffff"];
            else if (layer.PaletteStops.Count > 8)
                layer.PaletteStops = layer.PaletteStops.Take(8).ToList();

            layer.Masks ??= [];
            if (layer.Masks.Count > 8)
                layer.Masks = layer.Masks.Take(8).ToList();
            var maskIds = new HashSet<Guid>();
            foreach (var mask in layer.Masks)
            {
                if (mask.Id == Guid.Empty || !maskIds.Add(mask.Id))
                {
                    mask.Id = Guid.NewGuid();
                    maskIds.Add(mask.Id);
                }
                mask.Name = string.IsNullOrWhiteSpace(mask.Name) ? mask.Type + " Mask" : mask.Name.Trim();
                mask.Clamp();
            }

            layer.Values ??= new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            // Preserve every finite numeric setting from the scene. The renderer simply ignores
            // unknown keys, while discarding them here made customized/future parameters vanish
            // after a save/load round-trip. Keep sane key/count bounds for malformed files.
            layer.Values = layer.Values
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && pair.Key.Length <= 128 && double.IsFinite(pair.Value))
                .Take(4096)
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        }

        if (scene.ActiveLayerId is not Guid activeId || scene.Layers.All(layer => layer.Id != activeId))
            scene.ActiveLayerId = scene.Layers[0].Id;

        scene.FormatVersion = GlyphoreScene.CurrentFormatVersion;
        return scene;
    }
}

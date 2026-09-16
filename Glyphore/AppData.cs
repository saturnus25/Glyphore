using System.Reflection;
using System.Text.Json;

namespace Glyphore;

internal sealed class AppData
{
    public Dictionary<string, Dictionary<string, Dictionary<string, JsonElement>>> Presets { get; private set; } = new();
    public Dictionary<string, List<string>> Palettes { get; private set; } = new();
    public Dictionary<string, string> Charsets { get; private set; } = new();

    public static AppData Load()
    {
        var asm = Assembly.GetExecutingAssembly();
        return new AppData
        {
            Presets = LoadJson<Dictionary<string, Dictionary<string, Dictionary<string, JsonElement>>>>(asm, "Presets.json"),
            Palettes = LoadJson<Dictionary<string, List<string>>>(asm, "Palettes.json"),
            Charsets = LoadJson<Dictionary<string, string>>(asm, "Charsets.json")
        };
    }

    private static T LoadJson<T>(Assembly asm, string suffix) where T : new()
    {
        var name = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        if (name is null) return new T();
        using var stream = asm.GetManifestResourceStream(name)!;
        return JsonSerializer.Deserialize<T>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new T();
    }
}

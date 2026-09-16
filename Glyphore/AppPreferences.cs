using System.Text.Json;

namespace Glyphore;

internal sealed class AppPreferences
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static string PreferencesPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Glyphore",
        "preferences.json");

    public bool DiscordRichPresenceEnabled { get; set; } = true;

    public static AppPreferences Load()
    {
        try
        {
            string path = PreferencesPath;
            if (!File.Exists(path)) return new AppPreferences();
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppPreferences>(json, JsonOptions) ?? new AppPreferences();
        }
        catch
        {
            // Preferences are convenience state. A malformed/locked file must never prevent startup.
            return new AppPreferences();
        }
    }

    public void Save()
    {
        try
        {
            string path = PreferencesPath;
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);

            string temp = path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(this, JsonOptions), new System.Text.UTF8Encoding(false));
            File.Move(temp, path, overwrite: true);
        }
        catch
        {
            // A read-only profile should not turn a preference toggle into an application error.
        }
    }
}

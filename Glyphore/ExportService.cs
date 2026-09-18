using System.Text;

namespace Glyphore;

internal sealed record ExportFrame(string Text, byte[]? Rgb24 = null, byte[]? Alpha8 = null, RasterFrame? Raster = null);

internal sealed record TextExportOptions(bool PseudoTransparency = false, Color TerminalBackground = default)
{
    public static TextExportOptions Default { get; } = new(false, Color.Black);
}

internal static partial class ExportService
{
    private const string GeneratorName = "Glyphoré 6.0.2";
    private const int FileBufferSize = 1 << 16;
    private const int TextBufferSize = 1 << 14;
    private const int StreamingFlushInterval = 32;
    private static readonly UTF8Encoding Utf8NoBom = new(false);

    private enum TextExportFormat
    {
        Text,
        PowerShell,
        Html,
        Json,
        CSharp,
        Ansi
    }

    private static TextExportFormat ResolveFormat(string path)
        => Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".ps1" => TextExportFormat.PowerShell,
            ".html" or ".htm" => TextExportFormat.Html,
            ".json" => TextExportFormat.Json,
            ".cs" => TextExportFormat.CSharp,
            ".ans" => TextExportFormat.Ansi,
            _ => TextExportFormat.Text
        };

    private static bool ShouldFlushFrame(int frameIndex, int frameCount)
        => ((frameIndex + 1) % StreamingFlushInterval) == 0 || frameIndex == frameCount - 1;

    internal static int GetEffectiveFps(int requestedFps)
        => Math.Max(1, requestedFps);

    public static bool SupportsPseudoTransparency(string path)
        => ResolveFormat(path) is TextExportFormat.PowerShell or TextExportFormat.CSharp or TextExportFormat.Ansi;

    public static async Task SaveGeneratedAsync(
        string path,
        EffectSettings settings,
        int frameCount,
        Func<int, ExportFrame> captureFrame,
        TextExportOptions? options = null,
        Action<int, int>? progress = null)
    {
        options ??= TextExportOptions.Default;
        frameCount = Math.Max(1, frameCount);
        switch (ResolveFormat(path))
        {
            case TextExportFormat.PowerShell:
                await SavePowerShellGeneratedAsync(path, settings, frameCount, captureFrame, options, progress);
                break;
            case TextExportFormat.Html:
                await SaveHtmlGeneratedAsync(path, settings, frameCount, captureFrame, progress);
                break;
            case TextExportFormat.Json:
                await SaveJsonGeneratedAsync(path, settings, frameCount, captureFrame, progress);
                break;
            case TextExportFormat.CSharp:
                await SaveCSharpGeneratedAsync(path, settings, frameCount, captureFrame, options, progress);
                break;
            case TextExportFormat.Ansi:
                await SaveAnsiGeneratedAsync(path, settings, frameCount, captureFrame, options, progress);
                break;
            default:
                await SaveTextGeneratedAsync(path, captureFrame, progress);
                break;
        }
    }

    public static bool ContainsNonOpaqueAlpha(IEnumerable<ExportFrame> frames)
        => frames.Any(frame => frame.Alpha8 is { Length: > 0 } alpha && alpha.Any(value => value < 255));

    public static bool SupportsPartialAlpha(string path)
        => ResolveFormat(path) is TextExportFormat.Html or TextExportFormat.Json;

    public static string PartialAlphaCompatibilityMessage(string path, bool english)
    {
        string format = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".ps1" => "PowerShell / ANSI",
            ".cs" => "C# console / ANSI",
            ".ans" => "ANSI",
            ".txt" => english ? "plain text" : "texto plano",
            _ => english ? "this text format" : "este formato de texto"
        };

        return english
            ? $"{format} cannot represent per-character alpha. Soft masks, fades and partial transparency will not be preserved. Enable pseudo-transparency against the correct terminal background, or use HTML/raster/video for real alpha."
            : $"{format} no puede representar alpha por carácter. Las máscaras suaves, fades y transparencias parciales no se conservarán. Activa la pseudo-transparencia contra el fondo correcto del terminal, o usa HTML/raster/vídeo para alpha real.";
    }

    public static void Save(string path, EffectSettings settings, List<ExportFrame> frames)
    {
        switch (ResolveFormat(path))
        {
            case TextExportFormat.PowerShell:
                SavePowerShell(path, settings, frames);
                break;
            case TextExportFormat.Html:
                SaveHtml(path, settings, frames);
                break;
            case TextExportFormat.Json:
                SaveJson(path, settings, frames);
                break;
            case TextExportFormat.CSharp:
                SaveCSharp(path, settings, frames);
                break;
            case TextExportFormat.Ansi:
                SaveAnsi(path, settings, frames);
                break;
            default:
                SaveText(path, frames);
                break;
        }
    }
}

using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace Glyphore;

internal static partial class ExportService
{
    private static string Pack(List<string> frames)
    {
        byte[] raw = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(frames));
        using var output = new MemoryStream();
        using (var gz = new GZipStream(output, CompressionLevel.SmallestSize, true))
            gz.Write(raw);
        return Convert.ToBase64String(output.ToArray());
    }

    private static string ColorizeAnsi(ExportFrame frame, EffectSettings settings)
        => ColorizeAnsi(frame, settings, TextExportOptions.Default);

    private static string ColorizeAnsi(ExportFrame frame, EffectSettings settings, TextExportOptions options)
    {
        if (frame.Rgb24 is { Length: > 0 } rgb)
            return ColorizeAnsiExact(frame.Text, rgb, frame.Alpha8, options);
        return ColorizeAnsiFromPalette(frame.Text, settings);
    }

    private static string ColorizeAnsiExact(string frame, byte[] rgb24, byte[]? alpha8, TextExportOptions options)
    {
        var sb = new StringBuilder();
        int lastR = -1, lastG = -1, lastB = -1, cell = 0;
        Color background = options.TerminalBackground.IsEmpty ? Color.Black : options.TerminalBackground;
        foreach (var rune in frame.EnumerateRunes())
        {
            if (rune.Value == '\n')
            {
                sb.Append("\x1b[0m\n");
                lastR = lastG = lastB = -1;
                continue;
            }
            if (rune.Value == '\r')
            {
                sb.Append('\r');
                continue;
            }

            int offset = cell * 3;
            if (offset + 2 < rgb24.Length)
            {
                int r = rgb24[offset], g = rgb24[offset + 1], b = rgb24[offset + 2];
                if (options.PseudoTransparency && alpha8 is { Length: > 0 } && cell < alpha8.Length)
                {
                    int a = alpha8[cell];
                    r = (r * a + background.R * (255 - a) + 127) / 255;
                    g = (g * a + background.G * (255 - a) + 127) / 255;
                    b = (b * a + background.B * (255 - a) + 127) / 255;
                }
                if (r != lastR || g != lastG || b != lastB)
                {
                    sb.Append($"\x1b[38;2;{r};{g};{b}m");
                    lastR = r; lastG = g; lastB = b;
                }
            }
            sb.Append(rune.ToString());
            cell++;
        }
        sb.Append("\x1b[0m");
        return sb.ToString();
    }

    private static string ColorizeAnsiFromPalette(string frame, EffectSettings settings)
    {
        var stops = InterpolatePalette(settings.PaletteStops, 24);
        var runes = settings.Charset.EnumerateRunes().ToArray();
        var map = new Dictionary<string, int>();
        for (int i = 0; i < runes.Length; i++) map[runes[i].ToString()] = i;

        var sb = new StringBuilder();
        int last = -1;
        foreach (var rune in frame.EnumerateRunes())
        {
            if (rune.Value == '\n')
            {
                sb.Append("\x1b[0m\n");
                last = -1;
                continue;
            }
            if (rune.Value == '\r')
            {
                sb.Append('\r');
                continue;
            }

            int runeIndex = map.GetValueOrDefault(
                rune.ToString(),
                rune.Value == ' ' ? 0 : Math.Max(0, runes.Length * 3 / 4));
            double value = runeIndex / (double)Math.Max(1, runes.Length - 1);
            int level = Math.Clamp((int)Math.Round(value * (stops.Count - 1)), 0, stops.Count - 1);

            if (level != last)
            {
                var color = stops[level];
                sb.Append($"\x1b[38;2;{color.R};{color.G};{color.B}m");
                last = level;
            }
            sb.Append(rune.ToString());
        }

        sb.Append("\x1b[0m");
        return sb.ToString();
    }

    private static string ColorizeHtml(ExportFrame frame, EffectSettings settings)
    {
        bool exactColor = settings.ColorEnabled && frame.Rgb24 is { Length: > 0 };
        bool exactAlpha = frame.Alpha8 is { Length: > 0 };

        if (exactColor || exactAlpha)
            return ColorizeHtmlExact(frame.Text, exactColor ? frame.Rgb24 : null, exactAlpha ? frame.Alpha8 : null);
        if (settings.ColorEnabled)
            return ColorizeHtmlFromPalette(frame.Text, settings);
        return System.Net.WebUtility.HtmlEncode(frame.Text) ?? string.Empty;
    }

    private static string ColorizeHtmlExact(string frame, byte[]? rgb24, byte[]? alpha8)
    {
        var sb = new StringBuilder();
        var buffer = new StringBuilder();
        int lastPacked = -1;
        int lastAlpha = -1;
        int cell = 0;

        void Flush()
        {
            if (buffer.Length == 0) return;
            string encoded = System.Net.WebUtility.HtmlEncode(buffer.ToString());
            if (lastPacked >= 0 && lastAlpha >= 0)
            {
                int r = (lastPacked >> 16) & 255, g = (lastPacked >> 8) & 255, b = lastPacked & 255;
                string opacity = (lastAlpha / 255.0).ToString("0.###", CultureInfo.InvariantCulture);
                sb.Append($"<span style=\"color:#{r:x2}{g:x2}{b:x2};opacity:{opacity}\">{encoded}</span>");
            }
            else if (lastPacked >= 0)
            {
                int r = (lastPacked >> 16) & 255, g = (lastPacked >> 8) & 255, b = lastPacked & 255;
                sb.Append($"<span style=\"color:#{r:x2}{g:x2}{b:x2}\">{encoded}</span>");
            }
            else if (lastAlpha >= 0)
            {
                string opacity = (lastAlpha / 255.0).ToString("0.###", CultureInfo.InvariantCulture);
                sb.Append($"<span style=\"opacity:{opacity}\">{encoded}</span>");
            }
            else
            {
                sb.Append(encoded);
            }
            buffer.Clear();
        }

        foreach (var rune in frame.EnumerateRunes())
        {
            if (rune.Value == '\n')
            {
                Flush();
                sb.Append('\n');
                lastPacked = -1;
                lastAlpha = -1;
                continue;
            }
            if (rune.Value == '\r') continue;

            int rgbOffset = cell * 3;
            int packed = rgb24 is not null && rgbOffset + 2 < rgb24.Length
                ? (rgb24[rgbOffset] << 16) | (rgb24[rgbOffset + 1] << 8) | rgb24[rgbOffset + 2]
                : -1;
            int alpha = alpha8 is not null && cell < alpha8.Length ? alpha8[cell] : -1;

            if (buffer.Length == 0)
            {
                lastPacked = packed;
                lastAlpha = alpha;
            }
            else if (packed != lastPacked || alpha != lastAlpha)
            {
                Flush();
                lastPacked = packed;
                lastAlpha = alpha;
            }

            buffer.Append(rune.ToString());
            cell++;
        }

        Flush();
        return sb.ToString();
    }

    private static string ColorizeHtmlFromPalette(string frame, EffectSettings settings)
    {
        var stops = InterpolatePalette(settings.PaletteStops, 16);
        var runes = settings.Charset.EnumerateRunes().ToArray();
        var map = new Dictionary<string, int>();
        for (int i = 0; i < runes.Length; i++) map[runes[i].ToString()] = i;

        var sb = new StringBuilder();
        var buffer = new StringBuilder();
        int last = -1;

        void Flush()
        {
            if (buffer.Length == 0) return;
            var color = stops[Math.Max(0, last)];
            sb.Append($"<span style=\"color:#{color.R:x2}{color.G:x2}{color.B:x2}\">{System.Net.WebUtility.HtmlEncode(buffer.ToString())}</span>");
            buffer.Clear();
        }

        foreach (var rune in frame.EnumerateRunes())
        {
            if (rune.Value == '\n')
            {
                Flush();
                sb.Append('\n');
                last = -1;
                continue;
            }
            if (rune.Value == '\r') continue;

            int runeIndex = map.GetValueOrDefault(
                rune.ToString(),
                rune.Value == ' ' ? 0 : Math.Max(0, runes.Length * 3 / 4));
            int level = Math.Clamp(
                (int)Math.Round(runeIndex / (double)Math.Max(1, runes.Length - 1) * (stops.Count - 1)),
                0,
                stops.Count - 1);

            if (last < 0) last = level;
            else if (level != last)
            {
                Flush();
                last = level;
            }
            buffer.Append(rune.ToString());
        }

        Flush();
        return sb.ToString();
    }

    private static List<Color> InterpolatePalette(List<string> source, int count)
    {
        var input = source.Count >= 2
            ? source.Select(ColorUtil.ParseHtmlOrWhite).ToList()
            : new List<Color> { Color.Gray, Color.White };
        var output = new List<Color>(count);

        for (int i = 0; i < count; i++)
        {
            double position = i / (double)Math.Max(1, count - 1) * (input.Count - 1);
            int a = Math.Min(input.Count - 2, (int)Math.Floor(position));
            double t = position - a;
            var x = input[a];
            var y = input[a + 1];
            output.Add(Color.FromArgb(
                (int)Math.Round(x.R + (y.R - x.R) * t),
                (int)Math.Round(x.G + (y.G - x.G) * t),
                (int)Math.Round(x.B + (y.B - x.B) * t)));
        }

        return output;
    }
}

using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace Glyphore;

internal enum RasterExportStage
{
    WritingFrames,
    Encoding
}

internal readonly record struct RasterExportProgress(RasterExportStage Stage, int Current, int Total);

internal static class RasterAnimationExporter
{
    private static IReadOnlyList<RasterExportProfile>? _cachedProfiles;

    public static IReadOnlyList<RasterExportProfile> GetAvailableProfiles(bool refresh = false)
    {
        if (!refresh && _cachedProfiles is not null) return _cachedProfiles;

        var profiles = new List<RasterExportProfile>
        {
            new(
                "png-sequence",
                "PNG Sequence (Alpha)",
                ".png",
                true,
                "Un PNG transparente por fotograma.",
                false)
        };

        string? ffmpeg = FindFfmpeg();
        if (ffmpeg is null)
        {
            _cachedProfiles = profiles;
            return profiles;
        }

        string encoders = RunProbe(ffmpeg, "-hide_banner -encoders");
        string muxers = RunProbe(ffmpeg, "-hide_banner -muxers");
        string pixelFormats = RunProbe(ffmpeg, "-hide_banner -pix_fmts");

        bool Encoder(string name) => HasListedName(encoders, name);
        bool Muxer(string name) => HasListedName(muxers, name);
        bool Pixel(string name) => HasListedName(pixelFormats, name);
        bool EncoderSupports(string encoder, string pixelFormat)
        {
            if (!Encoder(encoder) || !Pixel(pixelFormat)) return false;
            string help = RunProbe(ffmpeg, $"-hide_banner -h encoder={encoder}");
            return ContainsToken(help, pixelFormat);
        }

        if (Encoder("libx264") && Muxer("mp4"))
            profiles.Insert(0, new("mp4-h264", "MP4 H.264", ".mp4", false, "Vídeo compatible sin canal alpha.", true));

        if (Encoder("gif") && Muxer("gif"))
            profiles.Add(new("gif", "Animated GIF", ".gif", false, "GIF animado; transparencia limitada.", true));

        if (Encoder("libvpx-vp9") && Muxer("webm"))
        {
            profiles.Add(new("webm-vp9", "WebM VP9", ".webm", false, "Vídeo VP9 comprimido.", true));
            if (EncoderSupports("libvpx-vp9", "yuva420p"))
                profiles.Add(new("webm-vp9-alpha", "WebM VP9 Alpha", ".webm", true, "Vídeo comprimido con transparencia.", true));
        }

        if (Muxer("mov") && EncoderSupports("prores_ks", "yuva444p10le"))
            profiles.Add(new("mov-prores4444", "MOV ProRes 4444 Alpha", ".mov", true, "Alta calidad y canal alpha; archivos grandes.", true));

        if (Muxer("mov") && EncoderSupports("qtrle", "argb"))
            profiles.Add(new("mov-qtrle", "MOV QuickTime Animation Alpha", ".mov", true, "QuickTime Animation sin pérdida con alpha; archivos grandes.", true));

        if (Muxer("apng") && EncoderSupports("apng", "rgba"))
            profiles.Add(new("apng", "APNG Alpha", ".png", true, "PNG animado con transparencia.", true));

        _cachedProfiles = profiles;
        return profiles;
    }

    public static async Task SaveAsync(
        string path,
        EffectSettings settings,
        IReadOnlyList<ExportFrame> frames,
        RasterExportOptions options,
        Action<int>? progress = null,
        Action<RasterExportProgress>? detailedProgress = null)
    {
        if (frames.Count == 0) throw new InvalidOperationException("No hay fotogramas para exportar.");

        if (options.Profile.Id == "png-sequence")
        {
            await SavePngSequenceAsync(path, settings, frames, progress, detailedProgress);
            return;
        }

        string ffmpeg = FindFfmpeg() ?? throw new InvalidOperationException(
            "FFmpeg no está disponible. Coloca ffmpeg.exe junto a Glyphore.exe o añádelo al PATH.");

        string temp = Path.Combine(Path.GetTempPath(), "GlyphoreExport_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            for (int i = 0; i < frames.Count; i++)
            {
                string framePath = Path.Combine(temp, $"frame_{i:000000}.png");
                SaveFramePng(framePath, settings, frames[i]);
                if ((i & 3) == 0 || i == frames.Count - 1)
                {
                    progress?.Invoke(i + 1);
                    detailedProgress?.Invoke(new RasterExportProgress(RasterExportStage.WritingFrames, i + 1, frames.Count));
                    await Task.Yield();
                }
            }

            string input = Path.Combine(temp, "frame_%06d.png");
            string args = BuildFfmpegArguments(options.Profile.Id, settings.Fps, input, path);
            var psi = new ProcessStartInfo(ffmpeg, args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            detailedProgress?.Invoke(new RasterExportProgress(RasterExportStage.Encoding, 0, frames.Count));
            using var process = Process.Start(psi) ?? throw new InvalidOperationException("No se pudo iniciar FFmpeg.");
            Task<string> stderrTask = process.StandardError.ReadToEndAsync();
            while (await process.StandardOutput.ReadLineAsync() is string line)
            {
                if (!line.StartsWith("frame=", StringComparison.OrdinalIgnoreCase)) continue;
                if (!int.TryParse(line.AsSpan(6).Trim(), out int encoded)) continue;
                detailedProgress?.Invoke(new RasterExportProgress(
                    RasterExportStage.Encoding,
                    Math.Clamp(encoded, 0, frames.Count),
                    frames.Count));
            }
            await process.WaitForExitAsync();
            string stderr = await stderrTask;
            if (process.ExitCode != 0)
                throw new InvalidOperationException("FFmpeg falló al exportar:\n" + stderr.Trim());
            detailedProgress?.Invoke(new RasterExportProgress(RasterExportStage.Encoding, frames.Count, frames.Count));
        }
        finally
        {
            try { Directory.Delete(temp, true); } catch { }
        }
    }

    public static async Task SaveGeneratedAsync(
        string path,
        EffectSettings settings,
        int frameCount,
        Func<int, byte[]?, RasterFrame> captureFrame,
        RasterExportOptions options,
        Action<RasterExportProgress>? detailedProgress = null)
    {
        if (frameCount <= 0) throw new InvalidOperationException("No hay fotogramas para exportar.");
        ArgumentNullException.ThrowIfNull(captureFrame);

        if (options.Profile.Id == "png-sequence")
        {
            string parent = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory;
            string stem = Path.GetFileNameWithoutExtension(path);
            string directory = Path.Combine(parent, stem + "_frames");
            Directory.CreateDirectory(directory);
            byte[]? reusablePngRgba = null;
            for (int i = 0; i < frameCount; i++)
            {
                RasterFrame frame = captureFrame(i, i == 0 ? null : reusablePngRgba);
                reusablePngRgba = frame.Rgba32;
                SaveRgbaPng(Path.Combine(directory, $"frame_{i:000000}.png"), frame);
                detailedProgress?.Invoke(new RasterExportProgress(RasterExportStage.WritingFrames, i + 1, frameCount));
                // Give WinForms a scheduling point every frame. A 4K/1080p capture can itself take
                // several milliseconds, so waiting four frames is enough to make the window look hung.
                await Task.Yield();
            }
            return;
        }

        string ffmpeg = FindFfmpeg() ?? throw new InvalidOperationException(
            "FFmpeg no está disponible. Coloca ffmpeg.exe junto a Glyphore.exe o añádelo al PATH.");

        RasterFrame first = captureFrame(0, null);
        byte[] reusableRgba = first.Rgba32;
        string args = BuildFfmpegRawArguments(options.Profile.Id, settings.Fps, first.Width, first.Height, path);
        var psi = new ProcessStartInfo(ffmpeg, args)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("No se pudo iniciar FFmpeg.");
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();
        // Drain FFmpeg's progress pipe concurrently. The frame-writing callback already reports
        // user-visible progress on the UI thread; draining stdout prevents the encoder from
        // blocking if its progress pipe fills.
        Task stdoutTask = Task.Run(async () =>
        {
            while (await process.StandardOutput.ReadLineAsync().ConfigureAwait(false) is not null) { }
        });

        Exception? writeFailure = null;
        try
        {
            Stream input = process.StandardInput.BaseStream;
            for (int i = 0; i < frameCount; i++)
            {
                RasterFrame frame = i == 0 ? first : captureFrame(i, reusableRgba);
                reusableRgba = frame.Rgba32;
                if (frame.Width != first.Width || frame.Height != first.Height)
                    throw new InvalidOperationException("La resolución raster cambió durante la exportación.");

                await input.WriteAsync(frame.Rgba32.AsMemory()).ConfigureAwait(true);
                detailedProgress?.Invoke(new RasterExportProgress(RasterExportStage.WritingFrames, i + 1, frameCount));
                await Task.Yield();
            }
            await input.FlushAsync().ConfigureAwait(true);
            process.StandardInput.Close();
        }
        catch (Exception ex)
        {
            writeFailure = ex;
            try { process.StandardInput.Close(); } catch { }
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch { }
        }

        await process.WaitForExitAsync().ConfigureAwait(true);
        await stdoutTask.ConfigureAwait(true);
        string stderr = await stderrTask.ConfigureAwait(true);

        if (writeFailure is not null)
            throw new InvalidOperationException(
                "La exportación raster se interrumpió." +
                (string.IsNullOrWhiteSpace(stderr) ? string.Empty : "\n" + stderr.Trim()), writeFailure);
        if (process.ExitCode != 0)
            throw new InvalidOperationException("FFmpeg falló al exportar:\n" + stderr.Trim());

        detailedProgress?.Invoke(new RasterExportProgress(
            RasterExportStage.Encoding,
            frameCount,
            frameCount));
    }

    private static string BuildFfmpegRawArguments(string profileId, int fps, int width, int height, string path)
    {
        string common = $"-y -hide_banner -loglevel error -progress pipe:1 -nostats -f rawvideo -pix_fmt rgba -s:v {width}x{height} -r {Math.Max(1, fps)} -i pipe:0";
        return profileId switch
        {
            "gif" => $"{common} -vf \"split[s0][s1];[s0]palettegen=max_colors=256[p];[s1][p]paletteuse=dither=sierra2_4a\" -loop 0 \"{path}\"",
            "webm-vp9" => $"{common} -c:v libvpx-vp9 -pix_fmt yuv420p -crf 28 -b:v 0 \"{path}\"",
            "webm-vp9-alpha" => $"{common} -c:v libvpx-vp9 -pix_fmt yuva420p -auto-alt-ref 0 -crf 28 -b:v 0 \"{path}\"",
            "mov-prores4444" => $"{common} -c:v prores_ks -profile:v 4 -pix_fmt yuva444p10le -vendor apl0 \"{path}\"",
            "mov-qtrle" => $"{common} -c:v qtrle -pix_fmt argb \"{path}\"",
            "apng" => $"{common} -c:v apng -pix_fmt rgba -plays 0 -f apng \"{path}\"",
            _ => $"{common} -c:v libx264 -pix_fmt yuv420p -movflags +faststart \"{path}\""
        };
    }

    private static string BuildFfmpegArguments(string profileId, int fps, string input, string path)
    {
        string common = $"-y -hide_banner -loglevel error -progress pipe:1 -nostats -framerate {Math.Max(1, fps)} -i \"{input}\"";
        return profileId switch
        {
            "gif" => $"{common} -vf \"split[s0][s1];[s0]palettegen=max_colors=256[p];[s1][p]paletteuse=dither=sierra2_4a\" -loop 0 \"{path}\"",
            "webm-vp9" => $"{common} -c:v libvpx-vp9 -pix_fmt yuv420p -crf 28 -b:v 0 \"{path}\"",
            "webm-vp9-alpha" => $"{common} -c:v libvpx-vp9 -pix_fmt yuva420p -auto-alt-ref 0 -crf 28 -b:v 0 \"{path}\"",
            "mov-prores4444" => $"{common} -c:v prores_ks -profile:v 4 -pix_fmt yuva444p10le -vendor apl0 \"{path}\"",
            "mov-qtrle" => $"{common} -c:v qtrle -pix_fmt argb \"{path}\"",
            "apng" => $"{common} -c:v apng -pix_fmt rgba -plays 0 -f apng \"{path}\"",
            _ => $"{common} -c:v libx264 -pix_fmt yuv420p -movflags +faststart \"{path}\""
        };
    }

    private static async Task SavePngSequenceAsync(
        string path,
        EffectSettings settings,
        IReadOnlyList<ExportFrame> frames,
        Action<int>? progress,
        Action<RasterExportProgress>? detailedProgress)
    {
        string parent = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory;
        string stem = Path.GetFileNameWithoutExtension(path);
        string directory = Path.Combine(parent, stem + "_frames");
        Directory.CreateDirectory(directory);

        for (int i = 0; i < frames.Count; i++)
        {
            SaveFramePng(Path.Combine(directory, $"frame_{i:000000}.png"), settings, frames[i]);
            if ((i & 3) == 0 || i == frames.Count - 1)
            {
                progress?.Invoke(i + 1);
                detailedProgress?.Invoke(new RasterExportProgress(RasterExportStage.WritingFrames, i + 1, frames.Count));
                await Task.Yield();
            }
        }
    }

    private static void SaveFramePng(string path, EffectSettings settings, ExportFrame frame)
    {
        if (frame.Raster is { } raster)
        {
            SaveRgbaPng(path, raster);
            return;
        }

        using Bitmap bitmap = RenderLegacyFrame(settings, frame);
        bitmap.Save(path, ImageFormat.Png);
    }

    private static void SaveRgbaPng(string path, RasterFrame frame)
    {
        if (frame.Rgba32.Length != checked(frame.Width * frame.Height * 4))
            throw new InvalidDataException("El frame RGBA tiene un tamaño inválido.");

        using var bitmap = new Bitmap(frame.Width, frame.Height, PixelFormat.Format32bppArgb);
        var rect = new Rectangle(0, 0, frame.Width, frame.Height);
        BitmapData data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            int dstStride = Math.Abs(data.Stride);
            byte[] row = new byte[frame.Width * 4];
            for (int y = 0; y < frame.Height; y++)
            {
                int src = y * frame.Width * 4;
                for (int x = 0; x < frame.Width; x++)
                {
                    int si = src + x * 4;
                    int di = x * 4;
                    row[di] = frame.Rgba32[si + 2];     // B
                    row[di + 1] = frame.Rgba32[si + 1]; // G
                    row[di + 2] = frame.Rgba32[si];     // R
                    row[di + 3] = frame.Rgba32[si + 3]; // A
                }
                Marshal.Copy(row, 0, data.Scan0 + y * data.Stride, row.Length);
            }
        }
        finally { bitmap.UnlockBits(data); }
        bitmap.Save(path, ImageFormat.Png);
    }

    private static Bitmap RenderLegacyFrame(EffectSettings settings, ExportFrame frame)
    {
        string[] lines = frame.Text.Replace("\r", string.Empty).Split('\n');
        int cols = Math.Max(1, settings.Width);
        int rows = Math.Max(1, settings.Height);
        int cellH = Math.Clamp(900 / rows, 10, 28);
        int cellW = Math.Max(5, (int)Math.Round(cellH * .55));
        int width = cols * cellW;
        int height = rows * cellH;
        var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(Color.Black);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
        using var font = new Font("Consolas", Math.Max(6f, cellH * .72f), FontStyle.Regular, GraphicsUnit.Pixel);
        using var format = new StringFormat(StringFormat.GenericTypographic) { FormatFlags = StringFormatFlags.MeasureTrailingSpaces };
        byte[]? rgb = frame.Rgb24;

        for (int y = 0; y < rows; y++)
        {
            string line = y < lines.Length ? lines[y] : string.Empty;
            for (int x = 0; x < cols; x++)
            {
                char ch = x < line.Length ? line[x] : ' ';
                if (ch == ' ') continue;
                Color color = Color.White;
                int ci = (y * cols + x) * 3;
                if (rgb is { Length: > 0 } && ci + 2 < rgb.Length)
                    color = Color.FromArgb(rgb[ci], rgb[ci + 1], rgb[ci + 2]);
                using var brush = new SolidBrush(color);
                g.DrawString(ch.ToString(), font, brush, new PointF(x * cellW, y * cellH), format);
            }
        }
        return bitmap;
    }

    public static string? FindFfmpeg()
    {
        string local = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe");
        if (File.Exists(local)) return local;
        string? path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path)) return null;
        foreach (string part in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                string candidate = Path.Combine(part.Trim('"'), "ffmpeg.exe");
                if (File.Exists(candidate)) return candidate;
            }
            catch { }
        }
        return null;
    }

    private static string RunProbe(string ffmpeg, string arguments)
    {
        try
        {
            var psi = new ProcessStartInfo(ffmpeg, arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            using var process = Process.Start(psi);
            if (process is null) return string.Empty;
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(5000))
            {
                try { process.Kill(true); } catch { }
                return string.Empty;
            }
            return stdout + "\n" + stderr;
        }
        catch { return string.Empty; }
    }

    private static bool HasListedName(string source, string name)
    {
        foreach (string line in source.Replace("\r", string.Empty).Split('\n'))
        {
            string[] parts = line.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;
            foreach (string listed in parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                if (listed.Equals(name, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private static bool ContainsToken(string source, string token)
        => source.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
}

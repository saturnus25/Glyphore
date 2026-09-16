using System.Diagnostics;
using System.Drawing.Imaging;

namespace Glyphore;

internal static class TransparentExportSmokeTest
{
    public static void Run()
    {
        using var host = new Form
        {
            Text = "Glyphoré alpha export smoke host",
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000),
            ClientSize = new Size(960, 540),
            ShowInTaskbar = false
        };
        using var preview = new GlPreviewControl
        {
            Dock = DockStyle.Fill,
            PreviewViewMode = PreviewViewMode.Fit,
            PreviewBackgroundColor = Color.Magenta,
            PreviewBackgroundMode = PreviewBackgroundMode.Solid,
            TargetFps = 30
        };
        host.Controls.Add(preview);
        host.Show();
        Application.DoEvents();

        var settings = new EffectSettings
        {
            Effect = "ASCII Title",
            Width = 96,
            Height = 36,
            Fps = 30,
            Duration = 1.0,
            CharsetName = "Smoke ramp",
            Charset = " .:-=+*#%@"
        };
        GlyphoreScene scene = GlyphoreScene.FromSettings(settings);
        scene.BackgroundColor = "#123456";
        SceneEffectLayer layer = scene.Layers[0];
        layer.Effect = "ASCII Title";
        layer.TitleText = "ALPHA";
        layer.TitlePrefab = "FIGlet · Standard";
        layer.TitleAnimate = false;
        layer.Values["title_glow"] = 1.25;
        layer.Values["title_shadow"] = 1.0;
        layer.Values["title_shadow_x"] = 2.0;
        layer.Values["title_shadow_y"] = 1.0;
        layer.Values["title_fade_mode"] = 1.0;
        layer.Values["title_fade_progress"] = .66;
        layer.Values["title_fade_softness"] = .12;
        layer.Masks.Add(new SceneLayerMask
        {
            Name = "Alpha smoke radial gradient",
            Type = SceneMaskType.RadialGradient,
            X = .5,
            Y = .5,
            Width = .86,
            Height = .72,
            GradientSoftness = .9,
            Strength = 1.0
        });
        layer.Touch();

        preview.Scene = scene;
        preview.Settings = scene.CreateSettings(layer);
        Application.DoEvents();

        ExportFrame magentaFrame = preview.CaptureExportFrame(.42, includeRaster: true, RasterBackgroundMode.Transparent, Color.Magenta);
        RasterFrame magentaRaster = magentaFrame.Raster ?? throw new InvalidOperationException("Transparent capture did not return an RGBA raster frame.");
        ValidateAlphaRange(magentaRaster, "OpenGL RGBA readback");

        // Preview background is an editor-only surface. A transparent export must be bit-identical
        // when the preview background changes underneath it.
        preview.PreviewBackgroundColor = Color.White;
        preview.PreviewBackgroundMode = PreviewBackgroundMode.Checkerboard;
        Application.DoEvents();
        ExportFrame checkerFrame = preview.CaptureExportFrame(.42, includeRaster: true, RasterBackgroundMode.Transparent, Color.Lime);
        RasterFrame checkerRaster = checkerFrame.Raster ?? throw new InvalidOperationException("Checkerboard transparent capture did not return RGBA.");
        if (!magentaRaster.Rgba32.AsSpan().SequenceEqual(checkerRaster.Rgba32))
            throw new InvalidOperationException("Preview Background leaked into the transparent export framebuffer.");

        string temp = Path.Combine(Path.GetTempPath(), "glyphore-alpha-smoke-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            ValidateTextExportAlpha(temp, settings, checkerFrame);

            var png = RasterAnimationExporter.GetAvailableProfiles(refresh: true).First(profile => profile.Id == "png-sequence");
            string pngAnchor = Path.Combine(temp, "alpha-smoke.png");
            RasterAnimationExporter.SaveAsync(
                pngAnchor,
                settings,
                [checkerFrame],
                new RasterExportOptions(png, RasterBackgroundMode.Transparent, Color.Black)).GetAwaiter().GetResult();

            string pngFrame = Path.Combine(temp, "alpha-smoke_frames", "frame_000000.png");
            if (!File.Exists(pngFrame)) throw new InvalidOperationException("PNG Sequence did not emit frame_000000.png.");
            ValidateDecodedBitmapAlpha(pngFrame, "PNG Sequence");

            RasterExportProfile? ffmpegAlpha = RasterAnimationExporter.GetAvailableProfiles()
                .FirstOrDefault(profile => profile.SupportsAlpha && profile.RequiresFfmpeg);
            string? ffmpeg = RasterAnimationExporter.FindFfmpeg();
            if (ffmpegAlpha is not null && ffmpeg is not null)
            {
                string encoded = Path.Combine(temp, "alpha-smoke" + ffmpegAlpha.Extension);
                RasterAnimationExporter.SaveAsync(
                    encoded,
                    settings,
                    [checkerFrame, checkerFrame],
                    new RasterExportOptions(ffmpegAlpha, RasterBackgroundMode.Transparent, Color.Black)).GetAwaiter().GetResult();

                string decoded = Path.Combine(temp, "decoded.png");
                var psi = new ProcessStartInfo(ffmpeg, $"-y -hide_banner -loglevel error -i \"{encoded}\" -frames:v 1 \"{decoded}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };
                using var process = Process.Start(psi) ?? throw new InvalidOperationException("Could not start FFmpeg for alpha reinspection.");
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0 || !File.Exists(decoded))
                    throw new InvalidOperationException($"FFmpeg alpha reinspection failed for {ffmpegAlpha.DisplayName}: {stderr.Trim()}");
                ValidateDecodedBitmapAlpha(decoded, ffmpegAlpha.DisplayName);
            }
        }
        finally
        {
            try { Directory.Delete(temp, true); } catch { }
        }
    }

    private static void ValidateTextExportAlpha(string temp, EffectSettings settings, ExportFrame frame)
    {
        byte[] alpha = frame.Alpha8 ?? throw new InvalidOperationException("Text export capture did not include the A8 cell-alpha plane.");
        if (!alpha.Any(value => value is > 0 and < 255))
            throw new InvalidOperationException("Text export capture did not preserve partial per-cell alpha.");
        if (!ExportService.ContainsNonOpaqueAlpha([frame]))
            throw new InvalidOperationException("Text export alpha compatibility detection did not recognize transparent cells.");
        if (!ExportService.SupportsPartialAlpha("smoke.html") || !ExportService.SupportsPartialAlpha("smoke.json"))
            throw new InvalidOperationException("HTML/JSON should advertise per-cell alpha support.");
        if (ExportService.SupportsPartialAlpha("smoke.ps1") || ExportService.SupportsPartialAlpha("smoke.ans"))
            throw new InvalidOperationException("Terminal text formats incorrectly advertise per-cell alpha support.");
        if (!ExportService.SupportsPseudoTransparency("smoke.ps1") || !ExportService.SupportsPseudoTransparency("smoke.ans") || ExportService.SupportsPseudoTransparency("smoke.html"))
            throw new InvalidOperationException("Pseudo-transparency capability reporting is inconsistent.");

        string psPath = Path.Combine(temp, "pseudo-alpha-smoke.ps1");
        ExportService.SaveGeneratedAsync(
            psPath, settings, 2, _ => frame,
            new TextExportOptions(true, ColorTranslator.FromHtml("#012456"))).GetAwaiter().GetResult();
        var (psFrames, _) = PowerShellImport.Parse(File.ReadAllText(psPath));
        if (psFrames.Count != 2 || !psFrames[0].Contains("\x1b[38;2;", StringComparison.Ordinal))
            throw new InvalidOperationException("Streamed PowerShell pseudo-transparency export was not generated/importable as ANSI frames.");

        string htmlPath = Path.Combine(temp, "alpha-smoke.html");
        ExportService.Save(htmlPath, settings, [frame]);
        string html = File.ReadAllText(htmlPath);
        if (!html.Contains("opacity:", StringComparison.Ordinal))
            throw new InvalidOperationException("HTML export did not emit CSS opacity for per-cell alpha.");

        string jsonPath = Path.Combine(temp, "alpha-smoke.json");
        ExportService.Save(jsonPath, settings, [frame]);
        string json = File.ReadAllText(jsonPath);
        if (!json.Contains("\"alphaEncoding\"", StringComparison.Ordinal) ||
            !json.Contains("\"alphaFrames\"", StringComparison.Ordinal))
            throw new InvalidOperationException("JSON export did not preserve the A8 alpha plane metadata/data.");
    }

    private static void ValidateAlphaRange(RasterFrame frame, string stage)
    {
        byte min = 255, max = 0;
        bool partial = false;
        for (int i = 3; i < frame.Rgba32.Length; i += 4)
        {
            byte alpha = frame.Rgba32[i];
            min = Math.Min(min, alpha);
            max = Math.Max(max, alpha);
            partial |= alpha is > 0 and < 255;
        }
        if (min >= 250) throw new InvalidOperationException($"{stage}: background remained opaque (minimum alpha {min}).");
        if (max <= 32) throw new InvalidOperationException($"{stage}: no opaque/visible title content was rendered (maximum alpha {max}).");
        if (!partial) throw new InvalidOperationException($"{stage}: no partial alpha was produced by fade/mask feather/glow/shadow.");
    }

    private static void ValidateDecodedBitmapAlpha(string path, string stage)
    {
        using var bitmap = new Bitmap(path);
        int min = 255, max = 0;
        bool partial = false;
        for (int y = 0; y < bitmap.Height; y += Math.Max(1, bitmap.Height / 80))
        {
            for (int x = 0; x < bitmap.Width; x += Math.Max(1, bitmap.Width / 120))
            {
                int alpha = bitmap.GetPixel(x, y).A;
                min = Math.Min(min, alpha);
                max = Math.Max(max, alpha);
                partial |= alpha is > 0 and < 255;
            }
        }
        if (min >= 250) throw new InvalidOperationException($"{stage}: decoded output is opaque (minimum alpha {min}).");
        if (max <= 32) throw new InvalidOperationException($"{stage}: decoded output lost visible content (maximum alpha {max}).");
        if (!partial) throw new InvalidOperationException($"{stage}: decoded output has no partial alpha.");
    }
}

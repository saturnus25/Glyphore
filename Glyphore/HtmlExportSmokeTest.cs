using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Glyphore;

internal static class HtmlExportSmokeTest
{
    public static void Run()
    {
        string syntheticPath = Path.Combine(Environment.CurrentDirectory, "html-export-synthetic-smoke.html");
        string staticPath = Path.Combine(Environment.CurrentDirectory, "html-export-static-smoke.html");
        ValidateSerializationAndPlayer(syntheticPath, staticPath);
        ValidateRenderedAnimations(Path.Combine(Environment.CurrentDirectory, "html-export-smoke.html"));
    }

    private static void ValidateSerializationAndPlayer(string animatedPath, string staticPath)
    {
        var settings = new EffectSettings
        {
            Effect = "HTML smoke · Unicode",
            Fps = 0,
            Duration = 3,
            Width = 1,
            Height = 1,
            ColorEnabled = true,
            IncludeExportCredit = true
        };
        ExportFrame[] frames =
        [
            new("é", [255, 0, 0], [255]),
            new("β", [0, 255, 0], [128]),
            new("終", [0, 0, 255], [64])
        ];

        int captures = 0;
        ExportService.SaveGeneratedAsync(
                animatedPath,
                settings,
                frames.Length,
                index =>
                {
                    captures++;
                    return frames[index];
                })
            .GetAwaiter()
            .GetResult();

        if (captures != frames.Length)
            throw new InvalidOperationException($"HTML export captured {captures} frames instead of {frames.Length}.");

        string html = File.ReadAllText(animatedPath);
        AssertContains(html, "const fps=1,screen=", "invalid FPS was not normalized to 1");
        AssertContains(html, "index===renderedFrame", "same-frame DOM update guard is missing");
        AssertContains(html, "Math.max(0,now-start)", "absolute elapsed-time frame calculation is missing");
        AssertContains(html, "if(play)pause();else", "Pause/Play does not preserve the current timeline offset");
        AssertContains(html, "frames.length<=1", "single-frame guard is missing");
        AssertContains(html, "screen.dataset.frameIndex=String(index)", "rendered frame diagnostics are missing");
        AssertContains(html, "color:#ff0000;opacity:1", "first frame RGB/alpha was not serialized");
        AssertContains(html, "color:#00ff00;opacity:0.502", "second frame RGB/alpha was not serialized");
        AssertContains(html, "color:#0000ff;opacity:0.251", "third frame RGB/alpha was not serialized");
        foreach (ExportFrame frame in frames)
            AssertContains(html, JsonSerializer.Serialize(frame.Text), $"Unicode frame '{frame.Text}' was not preserved");
        if (CountOccurrences(html, "{r:") != frames.Length)
            throw new InvalidOperationException("Generated HTML did not serialize exactly three distinct frame objects.");

        int[] timeline =
        [
            ExportService.GetHtmlFrameIndex(0, settings.Fps, frames.Length),
            ExportService.GetHtmlFrameIndex(1001, settings.Fps, frames.Length),
            ExportService.GetHtmlFrameIndex(2001, settings.Fps, frames.Length),
            ExportService.GetHtmlFrameIndex(3001, settings.Fps, frames.Length)
        ];
        if (!timeline.SequenceEqual(new[] { 0, 1, 2, 0 }))
            throw new InvalidOperationException(
                $"HTML timeline did not visit distinct frames at effective 1 FPS: {string.Join(",", timeline)}.");

        // Exercise the buffered compatibility path and its one-frame edge case too. Both paths
        // intentionally share the exact same browser player implementation.
        settings.Fps = -30;
        ExportService.Save(staticPath, settings, [frames[0]]);
        string staticHtml = File.ReadAllText(staticPath);
        AssertContains(staticHtml, "const fps=1,screen=", "buffered HTML FPS was not normalized");
        AssertContains(staticHtml, "const frames=[", "buffered HTML did not emit its frame array");
        if (ExportService.GetHtmlFrameIndex(double.PositiveInfinity, settings.Fps, 1) != 0)
            throw new InvalidOperationException("Static HTML did not remain on frame zero.");
    }

    private static void ValidateRenderedAnimations(string animatedPath)
    {
        using var host = new Form
        {
            Text = "Glyphoré HTML animation smoke host",
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000),
            ClientSize = new Size(960, 540),
            ShowInTaskbar = false
        };
        using var preview = new GlPreviewControl
        {
            Dock = DockStyle.Fill,
            PreviewViewMode = PreviewViewMode.Fit,
            TargetFps = 30
        };
        host.Controls.Add(preview);
        host.Show();
        Application.DoEvents();

        var settings = new EffectSettings
        {
            Effect = "Plasma",
            Preset = "HTML animation smoke",
            Width = 72,
            Height = 24,
            Fps = 30,
            Duration = 6.0,
            CharsetName = "HTML smoke ramp",
            Charset = " .:-=+*#%@",
            ColorEnabled = true,
            IncludeExportCredit = true
        };
        GlyphoreScene scene = GlyphoreScene.FromSettings(settings);
        GlyphoreScene snapshot = scene.Clone();
        preview.Scene = scene;
        preview.Settings = settings;
        Application.DoEvents();

        var renderedFrames = new ExportFrame[180];
        for (int index = 0; index < renderedFrames.Length; index++)
            renderedFrames[index] = preview.CaptureExportFrame(snapshot, index / 30.0);

        int[] sampleIndices = [0, 1, 2, 90, 179];
        ExportFrame[] samples = sampleIndices.Select(index => renderedFrames[index]).ToArray();
        List<string> report = DescribeFrameSamples("Plasma 30 FPS / 6 s", sampleIndices, samples, settings);
        AssertFramesAnimate("Plasma", samples, settings);

        int captures = 0;
        ExportService.SaveGeneratedAsync(
                animatedPath,
                settings,
                180,
                index =>
                {
                    captures++;
                    return renderedFrames[index];
                })
            .GetAwaiter()
            .GetResult();
        if (captures != 180)
            throw new InvalidOperationException($"30 FPS / 6 s HTML captured {captures} frames instead of 180.");

        string html = File.ReadAllText(animatedPath);
        AssertContains(html, "const fps=30,screen=", "30 FPS playback was not serialized");
        if (CountOccurrences(html, "{r:") != 180)
            throw new InvalidOperationException("The rendered 30 FPS / 6 s HTML does not contain exactly 180 frames.");

        ValidateAsciiTitleAnimations(preview, report);
        File.WriteAllLines(
            Path.Combine(Environment.CurrentDirectory, "html-export-smoke-report.txt"),
            report,
            new UTF8Encoding(false));
    }

    private static void ValidateAsciiTitleAnimations(GlPreviewControl preview, List<string> report)
    {
        var settings = new EffectSettings
        {
            Effect = "ASCII Title",
            Preset = "HTML title animation smoke",
            Width = 96,
            Height = 36,
            Fps = 30,
            Duration = 2.0,
            CharsetName = "HTML title ramp",
            Charset = " .:-=+*#%@",
            ColorEnabled = true
        };
        GlyphoreScene scene = GlyphoreScene.FromSettings(settings);
        SceneEffectLayer layer = scene.Layers[0];
        layer.Effect = "ASCII Title";
        layer.TitleText = "GLYPHORÉ STUDIO";
        layer.TitlePrefab = "FIGlet · Standard";
        layer.TitleAnimate = true;

        GlyphoreScene clone = scene.Clone();
        SceneEffectLayer clonedLayer = clone.Layers[0];
        if (!clonedLayer.TitleAnimate || clonedLayer.TitleText != layer.TitleText || clonedLayer.TitlePrefab != layer.TitlePrefab)
            throw new InvalidOperationException("ASCII Title animation metadata did not survive Scene.Clone().");

        (string Name, Action Configure, double[] Times)[] cases =
        [
            ("Wave", () =>
            {
                ResetTitleAnimation(layer);
                layer.Values["title_wave"] = 4.0;
                layer.Values["title_wave_x"] = 2.0;
                layer.Values["title_wave_speed"] = 3.0;
            }, [0.0, .15, .35, .7]),
            ("Shimmer", () =>
            {
                ResetTitleAnimation(layer);
                layer.Values["title_shimmer"] = 3.0;
                layer.Values["title_shimmer_speed"] = 4.0;
                layer.Values["title_shimmer_frequency"] = .5;
                layer.Values["title_shimmer_width"] = .2;
                layer.Values["title_shimmer_phase"] = 0.0;
            }, [0.0, .12, .27, .48, .72]),
            ("Typewriter / Reveal", () =>
            {
                ResetTitleAnimation(layer);
                layer.Values["title_reveal"] = 1.0;
            }, [.03, .2, .6, 1.2, 2.8]),
            ("Glitch", () =>
            {
                ResetTitleAnimation(layer);
                layer.Values["title_glitch"] = 2.0;
            }, [.03, .11, .19, .31, .47])
        ];

        foreach ((string name, Action configure, double[] times) in cases)
        {
            configure();
            layer.Touch();
            GlyphoreScene snapshot = scene.Clone();
            ExportFrame[] frames = times
                .Select(time => preview.CaptureExportFrame(snapshot, time))
                .ToArray();
            report.AddRange(DescribeFrameSamples("ASCII Title · " + name, times, frames, settings));
            AssertFramesAnimate("ASCII Title " + name, frames, settings);
        }

        ResetTitleAnimation(layer);
        layer.Values["title_wave"] = 4.0;
        layer.Values["title_wave_speed"] = 3.0;
        layer.Touch();
        GlyphoreScene rasterSnapshot = scene.Clone();
        RasterFrame raster0 = preview.CaptureRasterExportFrame(
            rasterSnapshot, 0.0, RasterBackgroundMode.SceneBackground);
        RasterFrame raster1 = preview.CaptureRasterExportFrame(
            rasterSnapshot, .35, RasterBackgroundMode.SceneBackground);
        if (HashBytes(raster0.Rgba32) == HashBytes(raster1.Rgba32))
            throw new InvalidOperationException("ASCII Title Wave raster/video sampling remained static while testing HTML sampling.");
    }

    private static void ResetTitleAnimation(SceneEffectLayer layer)
    {
        layer.Values["title_wave"] = 0.0;
        layer.Values["title_wave_x"] = 0.0;
        layer.Values["title_shimmer"] = 0.0;
        layer.Values["title_reveal"] = 0.0;
        layer.Values["title_glitch"] = 0.0;
    }

    private static void AssertFramesAnimate(string context, IReadOnlyList<ExportFrame> frames, EffectSettings settings)
    {
        int textVariants = frames.Select(frame => HashText(frame.Text)).Distinct(StringComparer.Ordinal).Count();
        int rgbVariants = frames.Select(frame => HashBytes(frame.Rgb24)).Distinct(StringComparer.Ordinal).Count();
        int alphaVariants = frames.Select(frame => HashBytes(frame.Alpha8)).Distinct(StringComparer.Ordinal).Count();
        int richVariants = frames
            .Select(frame => HashText(ExportService.GetHtmlRichFrameForTest(frame, settings)))
            .Distinct(StringComparer.Ordinal)
            .Count();
        if (Math.Max(textVariants, Math.Max(rgbVariants, alphaVariants)) <= 1)
            throw new InvalidOperationException($"{context} produced identical Text, RGB and Alpha planes at every sampled time.");
        if (richVariants <= 1)
            throw new InvalidOperationException($"{context} produced different export planes but identical HTML rich frames.");
    }

    private static List<string> DescribeFrameSamples(
        string context,
        IReadOnlyList<int> indices,
        IReadOnlyList<ExportFrame> frames,
        EffectSettings settings)
        => DescribeFrameSamples(context, indices.Select(index => (double)index).ToArray(), frames, settings);

    private static List<string> DescribeFrameSamples(
        string context,
        IReadOnlyList<double> times,
        IReadOnlyList<ExportFrame> frames,
        EffectSettings settings)
    {
        var lines = new List<string> { context, "sample,text_sha256,rgb_sha256,alpha_sha256,html_sha256" };
        for (int i = 0; i < frames.Count; i++)
        {
            ExportFrame frame = frames[i];
            lines.Add(string.Join(",",
                times[i].ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
                HashText(frame.Text),
                HashBytes(frame.Rgb24),
                HashBytes(frame.Alpha8),
                HashText(ExportService.GetHtmlRichFrameForTest(frame, settings))));
        }
        lines.Add(string.Empty);
        return lines;
    }

    private static string HashText(string value)
        => HashBytes(Encoding.UTF8.GetBytes(value));

    private static string HashBytes(byte[]? value)
        => value is { Length: > 0 }
            ? Convert.ToHexString(SHA256.HashData(value))
            : "NONE";

    private static void AssertContains(string value, string expected, string failure)
    {
        if (!value.Contains(expected, StringComparison.Ordinal))
            throw new InvalidOperationException(failure + $" (missing '{expected}').");
    }

    private static int CountOccurrences(string value, string needle)
    {
        int count = 0;
        int offset = 0;
        while ((offset = value.IndexOf(needle, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += needle.Length;
        }
        return count;
    }
}

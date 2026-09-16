using System.Text;

namespace Glyphore;

internal sealed partial class GlPreviewControl
{
    public string CaptureAsciiFrame(double timeSeconds)
    {
        if (!_loaded) return "OpenGL no disponible";
        MakeCurrent();

        if (_scene is { Layers.Count: > 0 } scene)
        {
            SyncAtlasIfNeeded();
            RenderSceneGlyphSelection(scene, timeSeconds);
            return ReadGlyphSelectionAsAscii(scene.Width, scene.Height, scene.Transform);
        }

        double animation = timeSeconds * _settings.Get("speed");
        double temporal = animation * _settings.Get("time_freq");
        RenderIntensity(_settings, (float)animation, (float)temporal);
        return ReadIntensityAsAscii(_settings);
    }

    public ExportFrame CaptureExportFrame(
        double timeSeconds,
        bool includeRaster = false,
        RasterBackgroundMode backgroundMode = RasterBackgroundMode.SceneBackground,
        Color? solidColor = null)
        => CaptureExportFrameCore(_scene, timeSeconds, includeRaster, backgroundMode, solidColor);

    // Export may take seconds or minutes. Render from an immutable scene snapshot rather than
    // consulting the live editor scene on every frame; this guarantees that save/load/export
    // all consume exactly the same layer values, preset provenance and mask/title metadata.
    public ExportFrame CaptureExportFrame(
        GlyphoreScene sceneSnapshot,
        double timeSeconds,
        bool includeRaster = false,
        RasterBackgroundMode backgroundMode = RasterBackgroundMode.SceneBackground,
        Color? solidColor = null)
        => CaptureExportFrameCore(sceneSnapshot, timeSeconds, includeRaster, backgroundMode, solidColor);

    private ExportFrame CaptureExportFrameCore(
        GlyphoreScene? sceneOverride,
        double timeSeconds,
        bool includeRaster,
        RasterBackgroundMode backgroundMode,
        Color? solidColor)
    {
        if (!_loaded) return new ExportFrame("OpenGL no disponible");
        MakeCurrent();

        EffectSettings outputSettings;
        ExportFrame result;
        GlyphoreScene? renderedScene = null;
        bool useCompositedColor = false;
        bool useCompositedGlyph = false;

        if (sceneOverride is { Layers.Count: > 0 } scene)
        {
            renderedScene = scene;
            // Sync resources for the explicit snapshot. SyncAtlasIfNeeded() intentionally follows
            // the live _scene field and would make an export snapshot depend on editor state.
            SyncSceneGlyphResources(scene);
            RenderSceneIntensity(scene, timeSeconds);
            RenderSceneColor(scene, timeSeconds);
            RenderSceneGlyphSelection(scene, timeSeconds);
            outputSettings = SceneOutputSettings(scene);
            result = ReadSceneExportFrame(outputSettings, scene.Transform);
            useCompositedColor = true;
            useCompositedGlyph = true;
        }
        else
        {
            double animation = timeSeconds * _settings.Get("speed");
            double temporal = animation * _settings.Get("time_freq");
            RenderIntensity(_settings, (float)animation, (float)temporal);
            RenderColor(_settings, (float)animation, (float)temporal);
            outputSettings = _settings;
            result = ReadExportFrame(outputSettings);
        }

        if (includeRaster)
        {
            Color background = backgroundMode switch
            {
                RasterBackgroundMode.SolidColor => solidColor ?? Color.Black,
                RasterBackgroundMode.SceneBackground when renderedScene is not null => ColorUtil.ParseHtmlOrWhite(renderedScene.BackgroundColor),
                _ => Color.Black
            };
            bool transparent = backgroundMode == RasterBackgroundMode.Transparent;
            RasterFrame raster = CaptureRasterFromCurrentBuffers(
                outputSettings,
                renderedScene,
                timeSeconds,
                useCompositedColor,
                useCompositedGlyph,
                transparent,
                background);
            result = result with { Raster = raster };
        }

        return result;
    }


    // Raster/video export does not need the textual frame, RGB24 cell colors or A8 cell plane.
    // Capture only the final raster so long videos stay O(one frame) in managed memory.
    public RasterFrame CaptureRasterExportFrame(
        GlyphoreScene sceneSnapshot,
        double timeSeconds,
        RasterBackgroundMode backgroundMode,
        Color? solidColor = null,
        byte[]? reusableRgba = null)
    {
        if (!_loaded) throw new InvalidOperationException("OpenGL no disponible");
        MakeCurrent();
        SyncSceneGlyphResources(sceneSnapshot);
        RenderSceneIntensity(sceneSnapshot, timeSeconds);
        RenderSceneColor(sceneSnapshot, timeSeconds);
        RenderSceneGlyphSelection(sceneSnapshot, timeSeconds);
        EffectSettings outputSettings = SceneOutputSettings(sceneSnapshot);
        Color background = backgroundMode switch
        {
            RasterBackgroundMode.SolidColor => solidColor ?? Color.Black,
            RasterBackgroundMode.SceneBackground => ColorUtil.ParseHtmlOrWhite(sceneSnapshot.BackgroundColor),
            _ => Color.Black
        };
        return CaptureRasterFromCurrentBuffers(
            outputSettings,
            sceneSnapshot,
            timeSeconds,
            useCompositedColor: true,
            useCompositedGlyph: true,
            transparent: backgroundMode == RasterBackgroundMode.Transparent,
            background: background,
            reusableRgba: reusableRgba);
    }

    public string CaptureCurrentAsciiFrame()
    {
        if (!_loaded) return "OpenGL no disponible";
        MakeCurrent();

        if (_scene is { Layers.Count: > 0 } scene)
        {
            AdvanceSceneClocks(scene);
            SyncAtlasIfNeeded();
            RenderSceneGlyphSelection(scene);
            return ReadGlyphSelectionAsAscii(scene.Width, scene.Height, scene.Transform);
        }

        var times = AdvancePreviewClocks();
        RenderIntensity(_settings, (float)times.Animation, (float)times.Temporal);
        return ReadIntensityAsAscii(_settings);
    }

    private unsafe string ReadIntensityAsAscii(EffectSettings outputSettings)
    {
        int w = Math.Max(2, outputSettings.Width);
        int h = Math.Max(2, outputSettings.Height);
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _intensityFbo);
        NativeGl.Finish();
        byte[] pixels = new byte[w * h * 4];
        fixed (byte* p = pixels)
            NativeGl.ReadPixels(0, 0, w, h, NativeGl.GL_RGBA, NativeGl.GL_UNSIGNED_BYTE, (IntPtr)p);
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, 0);

        var runes = outputSettings.Charset.EnumerateRunes().ToArray();
        if (runes.Length == 0) runes = " ".EnumerateRunes().ToArray();
        var sb = new StringBuilder((w + 1) * h);
        double gamma = Math.Max(.05, outputSettings.Get("gamma"));

        for (int y = h - 1; y >= 0; y--)
        {
            for (int x = 0; x < w; x++)
            {
                double v = pixels[(y * w + x) * 4] / 255.0;
                v = Math.Pow(Math.Clamp(v, 0, 1), 1.0 / gamma);
                if (outputSettings.Invert) v = 1 - v;
                int idx = Math.Clamp((int)Math.Round(v * (runes.Length - 1)), 0, runes.Length - 1);
                sb.Append(runes[idx].ToString());
            }
            if (y > 0) sb.Append('\n');
        }

        return sb.ToString();
    }

    private unsafe string ReadGlyphSelectionAsAscii(int width, int height, SceneTransform? transform)
    {
        int w = Math.Max(2, width);
        int h = Math.Max(2, height);
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _glyphSelectFbo);
        NativeGl.Finish();
        byte[] pixels = new byte[w * h * 4];
        fixed (byte* p = pixels)
            NativeGl.ReadPixels(0, 0, w, h, NativeGl.GL_RGBA, NativeGl.GL_UNSIGNED_BYTE, (IntPtr)p);
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, 0);

        var runes = _atlasRamp.EnumerateRunes().ToArray();
        if (runes.Length == 0) runes = " ".EnumerateRunes().ToArray();
        var sb = new StringBuilder((w + 1) * h);
        for (int y = h - 1; y >= 0; y--)
        {
            for (int x = 0; x < w; x++)
            {
                if (!TryMapSceneTransformCell(x, y, w, h, transform, out int sx, out int sy))
                {
                    sb.Append(' ');
                    continue;
                }
                int src = (sy * w + sx) * 4;
                int index = pixels[src] | (pixels[src + 1] << 8) | (pixels[src + 2] << 16);
                index = Math.Clamp(index, 0, runes.Length - 1);
                sb.Append(runes[index].ToString());
            }
            if (y > 0) sb.Append('\n');
        }
        return sb.ToString();
    }

    private unsafe ExportFrame ReadExportFrame(EffectSettings outputSettings)
    {
        int w = Math.Max(2, outputSettings.Width);
        int h = Math.Max(2, outputSettings.Height);
        byte[] intensity = new byte[w * h * 4];
        byte[] color = new byte[w * h * 4];

        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _intensityFbo);
        NativeGl.Finish();
        fixed (byte* p = intensity)
            NativeGl.ReadPixels(0, 0, w, h, NativeGl.GL_RGBA, NativeGl.GL_UNSIGNED_BYTE, (IntPtr)p);

        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _colorFbo);
        NativeGl.Finish();
        fixed (byte* p = color)
            NativeGl.ReadPixels(0, 0, w, h, NativeGl.GL_RGBA, NativeGl.GL_UNSIGNED_BYTE, (IntPtr)p);
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, 0);

        var runes = outputSettings.Charset.EnumerateRunes().ToArray();
        if (runes.Length == 0) runes = " ".EnumerateRunes().ToArray();
        var sb = new StringBuilder((w + 1) * h);
        var rgb24 = new byte[w * h * 3];
        var alpha8 = new byte[w * h];
        double gamma = Math.Max(.05, outputSettings.Get("gamma"));
        int cell = 0;

        for (int y = h - 1; y >= 0; y--)
        {
            for (int x = 0; x < w; x++)
            {
                int src = (y * w + x) * 4;
                double v = intensity[src] / 255.0;
                v = Math.Pow(Math.Clamp(v, 0, 1), 1.0 / gamma);
                if (outputSettings.Invert) v = 1 - v;
                int idx = Math.Clamp((int)Math.Round(v * (runes.Length - 1)), 0, runes.Length - 1);
                sb.Append(runes[idx].ToString());

                int dst = cell * 3;
                rgb24[dst] = color[src];
                rgb24[dst + 1] = color[src + 1];
                rgb24[dst + 2] = color[src + 2];
                alpha8[cell] = color[src + 3];
                cell++;
            }
            if (y > 0) sb.Append('\n');
        }

        return new ExportFrame(sb.ToString(), rgb24, alpha8);
    }

    private unsafe ExportFrame ReadSceneExportFrame(EffectSettings outputSettings, SceneTransform? transform)
    {
        int w = Math.Max(2, outputSettings.Width);
        int h = Math.Max(2, outputSettings.Height);
        byte[] glyph = new byte[w * h * 4];
        byte[] color = new byte[w * h * 4];

        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _glyphSelectFbo);
        NativeGl.Finish();
        fixed (byte* p = glyph)
            NativeGl.ReadPixels(0, 0, w, h, NativeGl.GL_RGBA, NativeGl.GL_UNSIGNED_BYTE, (IntPtr)p);

        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _colorFbo);
        NativeGl.Finish();
        fixed (byte* p = color)
            NativeGl.ReadPixels(0, 0, w, h, NativeGl.GL_RGBA, NativeGl.GL_UNSIGNED_BYTE, (IntPtr)p);
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, 0);

        var runes = _atlasRamp.EnumerateRunes().ToArray();
        if (runes.Length == 0) runes = " ".EnumerateRunes().ToArray();
        var sb = new StringBuilder((w + 1) * h);
        var rgb24 = new byte[w * h * 3];
        var alpha8 = new byte[w * h];
        int cell = 0;

        for (int y = h - 1; y >= 0; y--)
        {
            for (int x = 0; x < w; x++)
            {
                int dst = cell * 3;
                if (!TryMapSceneTransformCell(x, y, w, h, transform, out int sx, out int sy))
                {
                    sb.Append(' ');
                    rgb24[dst] = 0;
                    rgb24[dst + 1] = 0;
                    rgb24[dst + 2] = 0;
                    alpha8[cell] = 0;
                    cell++;
                    continue;
                }

                int src = (sy * w + sx) * 4;
                int index = glyph[src] | (glyph[src + 1] << 8) | (glyph[src + 2] << 16);
                index = Math.Clamp(index, 0, runes.Length - 1);
                sb.Append(runes[index].ToString());

                byte alpha = color[src + 3];
                if (alpha > 0)
                {
                    rgb24[dst] = (byte)Math.Clamp((color[src] * 255 + alpha / 2) / alpha, 0, 255);
                    rgb24[dst + 1] = (byte)Math.Clamp((color[src + 1] * 255 + alpha / 2) / alpha, 0, 255);
                    rgb24[dst + 2] = (byte)Math.Clamp((color[src + 2] * 255 + alpha / 2) / alpha, 0, 255);
                }
                else
                {
                    rgb24[dst] = rgb24[dst + 1] = rgb24[dst + 2] = 0;
                }
                alpha8[cell] = alpha;
                cell++;
            }
            if (y > 0) sb.Append('\n');
        }

        return new ExportFrame(sb.ToString(), rgb24, alpha8);
    }

    private static bool TryMapSceneTransformCell(int x, int y, int width, int height, SceneTransform? transform, out int sourceX, out int sourceY)
    {
        sourceX = x;
        sourceY = y;
        if (transform is null ||
            (Math.Abs(transform.Rotation) < 0.001 && Math.Abs(transform.PerspectiveX) < 0.001 && Math.Abs(transform.PerspectiveY) < 0.001))
            return true;

        double px = ((x + 0.5) / Math.Max(1.0, width)) * 2.0 - 1.0;
        double py = ((y + 0.5) / Math.Max(1.0, height)) * 2.0 - 1.0;
        double aspect = Math.Max(0.10, (width / Math.Max(1.0, height)) * 0.55);
        px *= aspect;

        double angle = -transform.Rotation * Math.PI / 180.0;
        double c = Math.Cos(angle);
        double s = Math.Sin(angle);
        double rx = c * px - s * py;
        double ry = s * px + c * py;

        double perspectiveX = Math.Max(0.20, 1.0 + transform.PerspectiveX * ry);
        double perspectiveY = Math.Max(0.20, 1.0 + transform.PerspectiveY * rx);
        rx /= perspectiveX;
        ry /= perspectiveY;
        rx /= aspect;

        double u = rx * 0.5 + 0.5;
        double v = ry * 0.5 + 0.5;
        if (u < 0.0 || u > 1.0 || v < 0.0 || v > 1.0) return false;

        sourceX = Math.Clamp((int)Math.Floor(u * width), 0, width - 1);
        sourceY = Math.Clamp((int)Math.Floor(v * height), 0, height - 1);
        return true;
    }
}

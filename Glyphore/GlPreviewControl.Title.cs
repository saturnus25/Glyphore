using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.Text;

namespace Glyphore;

internal sealed partial class GlPreviewControl
{
    private sealed class TitleTextureResource
    {
        public uint Texture;
        public string Signature = string.Empty;
    }

    private readonly Dictionary<Guid, TitleTextureResource> _titleTextures = new();

    private unsafe void BindLayerTitleTexture(SceneEffectLayer layer, EffectSettings settings, float titleTime)
    {
        if (!layer.Effect.Equals("ASCII Title", StringComparison.OrdinalIgnoreCase))
        {
            Ui(_captureProgram, "u_title_enabled", 0);
            Ui(_captureProgram, "u_title_animate", 0);
            U1(_captureProgram, "u_title_time", 0f);
            return;
        }

        string text = string.IsNullOrEmpty(layer.TitleText) ? "GLYPHORÉ" : layer.TitleText;
        string fontName = string.IsNullOrWhiteSpace(layer.TitleFont) ? "Consolas" : layer.TitleFont;
        string prefab = string.IsNullOrWhiteSpace(layer.TitlePrefab) ? "System Font" : layer.TitlePrefab;
        bool generatedPrefab = AsciiTitlePrefabGenerator.IsGeneratedPrefab(prefab);
        string signature = string.Join('|',
            settings.Width,
            settings.Height,
            text,
            fontName,
            prefab,
            layer.TitleBold,
            layer.TitleItalic,
            settings.Get("title_letter_spacing").ToString("R", CultureInfo.InvariantCulture),
            settings.Get("title_size").ToString("R", CultureInfo.InvariantCulture),
            settings.Get("title_x").ToString("R", System.Globalization.CultureInfo.InvariantCulture),
            settings.Get("title_y").ToString("R", System.Globalization.CultureInfo.InvariantCulture));

        if (!_titleTextures.TryGetValue(layer.Id, out var resource))
        {
            resource = new TitleTextureResource();
            _titleTextures[layer.Id] = resource;
        }

        if (resource.Texture == 0 || !string.Equals(resource.Signature, signature, StringComparison.Ordinal))
        {
            int w = Math.Max(2, settings.Width);
            int h = Math.Max(2, settings.Height);

            // Keep the source title geometry independent from visual effects. Glow, outline,
            // shadow, wave, rotation and perspective must never silently shrink the letters.
            // Use almost the whole logical title canvas. Position X/Y then traverses the
            // remaining free space all the way to the visible edges instead of being trapped
            // inside a small central safe region.
            const float safeWidth = .92f;
            const float safeHeight = .84f;
            float titleScale = (float)Math.Clamp(settings.Get("title_size"), .15, 2.5);
            int letterSpacing = Math.Max(0, (int)Math.Round(settings.Get("title_letter_spacing")));
            float titleX = (float)Math.Clamp(settings.Get("title_x"), -1.0, 1.0);
            float titleY = (float)Math.Clamp(settings.Get("title_y"), -1.0, 1.0);

            byte[] pixels;

            if (generatedPrefab)
            {
                // FIGlet contributes only occupied-cell geometry. Visible glyph selection is
                // performed later from the layer's selected charset, exactly like other effects.
                pixels = BuildGeneratedPrefabGrid(
                    text,
                    prefab,
                    w,
                    h,
                    safeWidth,
                    safeHeight,
                    titleScale,
                    letterSpacing,
                    titleX,
                    titleY);
            }
            else
            {
                const int renderScale = 2;
                int renderW = checked(w * renderScale);
                int renderH = checked(h * renderScale);
                using var renderBitmap = new Bitmap(renderW, renderH, PixelFormat.Format32bppArgb);
                using (var graphics = Graphics.FromImage(renderBitmap))
                {
                    graphics.Clear(Color.Black);
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                    FontStyle style = FontStyle.Regular;
                    if (layer.TitleBold) style |= FontStyle.Bold;
                    if (layer.TitleItalic) style |= FontStyle.Italic;
                    float nominalPx = Math.Max(2f, renderH * .72f);
                    Font CreateFont(float size)
                    {
                        try { return new Font(fontName, size, style, GraphicsUnit.Pixel); }
                        catch { return new Font("Consolas", size, style, GraphicsUnit.Pixel); }
                    }

                    using var brush = new SolidBrush(Color.White);
                    using var format = new StringFormat(StringFormat.GenericTypographic)
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.None,
                        FormatFlags = StringFormatFlags.NoClip
                    };

                    using Font nominalFont = CreateFont(nominalPx);
                    float nominalTrackingPx = letterSpacing * renderScale;
                    SizeF measured = letterSpacing == 0
                        ? graphics.MeasureString(text, nominalFont, Math.Max(renderW * 4, 4096), format)
                        : MeasureTrackedText(graphics, text, nominalFont, nominalTrackingPx);
                    float targetW = renderW * safeWidth;
                    float targetH = renderH * safeHeight;
                    float baseFit = Math.Min(1f, Math.Min(targetW / Math.Max(1f, measured.Width), targetH / Math.Max(1f, measured.Height)));
                    // title_size=1 means “fit”. Values above 1 deliberately grow beyond that fit
                    // instead of being clamped back to the same size. Letter spacing participates
                    // in measured.Width before this fit, so tracking never bypasses clipping/fit.
                    float finalPx = Math.Max(2f, nominalPx * baseFit * titleScale);
                    Font font = CreateFont(finalPx);

                    using (font)
                    {
                        var rect = new RectangleF(0, 0, renderW, renderH);
                        if (letterSpacing == 0)
                        {
                            // Compatibility path: preserve the original DrawString rendering
                            // exactly when tracking is zero.
                            graphics.DrawString(text, font, brush, rect, format);
                        }
                        else
                        {
                            float scaledTrackingPx = nominalTrackingPx * (finalPx / Math.Max(1f, nominalPx));
                            DrawTrackedText(graphics, text, font, brush, rect, scaledTrackingPx);
                        }
                    }
                }

                // Graphics.DrawString centers typographic metrics, which can still leave the
                // visible ink off-center because of font side-bearings. Recenter the actual
                // rendered pixels before applying the user's X/Y offset.
                Rectangle ink = FindVisibleInkBounds(renderBitmap);
                using var centeredBitmap = new Bitmap(renderW, renderH, PixelFormat.Format32bppArgb);
                using (var centered = Graphics.FromImage(centeredBitmap))
                {
                    centered.Clear(Color.Black);
                    centered.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    int recenterX = ink.Width > 0 ? (int)Math.Round(renderW * .5 - (ink.Left + ink.Width * .5)) : 0;
                    int recenterY = ink.Height > 0 ? (int)Math.Round(renderH * .5 - (ink.Top + ink.Height * .5)) : 0;
                    // X/Y are normalized placement controls: -1 and +1 move the visible ink
                    // to the corresponding canvas edge whenever there is free space.
                    int travelX = ink.Width > 0 ? Math.Max(0, (renderW - ink.Width) / 2) : renderW / 2;
                    int travelY = ink.Height > 0 ? Math.Max(0, (renderH - ink.Height) / 2) : renderH / 2;
                    int shiftX = recenterX + (int)Math.Round(titleX * travelX);
                    int shiftY = recenterY - (int)Math.Round(titleY * travelY);
                    centered.DrawImageUnscaled(renderBitmap, shiftX, shiftY);
                }

                using var bitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                using (var downsample = Graphics.FromImage(bitmap))
                {
                    downsample.Clear(Color.Black);
                    downsample.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    downsample.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    downsample.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    downsample.DrawImage(centeredBitmap, new Rectangle(0, 0, w, h), 0, 0, renderW, renderH, GraphicsUnit.Pixel);
                }

                pixels = new byte[w * h * 4];
                for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    int dst = (y * w + x) * 4;
                    pixels[dst] = color.B;
                    pixels[dst + 1] = color.G;
                    pixels[dst + 2] = color.R;
                    pixels[dst + 3] = color.A;
                }
            }

            if (resource.Texture == 0)
            {
                uint texture;
                NativeGl.GenTextures(1, &texture);
                resource.Texture = texture;
            }
            NativeGl.ActiveTexture(NativeGl.GL_TEXTURE0 + 4);
            NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, resource.Texture);
            NativeGl.PixelStorei(NativeGl.GL_UNPACK_ALIGNMENT, 1);
            NativeGl.TexParameteri(NativeGl.GL_TEXTURE_2D, NativeGl.GL_TEXTURE_MIN_FILTER, generatedPrefab ? (int)NativeGl.GL_NEAREST : (int)NativeGl.GL_LINEAR);
            NativeGl.TexParameteri(NativeGl.GL_TEXTURE_2D, NativeGl.GL_TEXTURE_MAG_FILTER, generatedPrefab ? (int)NativeGl.GL_NEAREST : (int)NativeGl.GL_LINEAR);
            fixed (byte* pixelPtr = pixels)
            {
                NativeGl.TexImage2D(NativeGl.GL_TEXTURE_2D, 0, (int)NativeGl.GL_RGBA8, w, h, 0,
                    NativeGl.GL_BGRA, NativeGl.GL_UNSIGNED_BYTE, (IntPtr)pixelPtr);
            }

            // Generated prefab source symbols never become visible glyphs. The title mask is
            // charset-independent, so changing charset only rebuilds the scene glyph resources.
            resource.Signature = signature;
        }

        NativeGl.ActiveTexture(NativeGl.GL_TEXTURE0 + 4);
        NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, resource.Texture);
        Ui(_captureProgram, "u_title_mask", 4);
        Ui(_captureProgram, "u_title_enabled", 1);
        Ui(_captureProgram, "u_title_animate", layer.TitleAnimate ? 1 : 0);
        U1(_captureProgram, "u_title_time", layer.TitleAnimate ? titleTime : 0f);

    }

    private static SizeF MeasureTrackedText(Graphics graphics, string text, Font font, float trackingPx)
    {
        string[] lines = text.Replace("\r", string.Empty).Split('\n');
        float lineHeight = Math.Max(1f, font.GetHeight(graphics));
        float width = 0f;
        using var format = new StringFormat(StringFormat.GenericTypographic)
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near,
            Trimming = StringTrimming.None,
            FormatFlags = StringFormatFlags.NoClip
        };

        foreach (string line in lines)
            width = Math.Max(width, MeasureTrackedLine(graphics, line, font, format, trackingPx));

        return new SizeF(width, Math.Max(lineHeight, lineHeight * Math.Max(1, lines.Length)));
    }

    private static void DrawTrackedText(Graphics graphics, string text, Font font, Brush brush, RectangleF bounds, float trackingPx)
    {
        string[] lines = text.Replace("\r", string.Empty).Split('\n');
        float lineHeight = Math.Max(1f, font.GetHeight(graphics));
        float totalHeight = lineHeight * Math.Max(1, lines.Length);
        float y = bounds.Top + (bounds.Height - totalHeight) * .5f;
        using var format = new StringFormat(StringFormat.GenericTypographic)
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near,
            Trimming = StringTrimming.None,
            FormatFlags = StringFormatFlags.NoClip
        };

        foreach (string line in lines)
        {
            List<string> elements = EnumerateTextElements(line);
            float lineWidth = MeasureTrackedLine(graphics, elements, font, format, trackingPx);
            float x = bounds.Left + (bounds.Width - lineWidth) * .5f;

            for (int index = 0; index < elements.Count; index++)
            {
                string element = elements[index];
                graphics.DrawString(element, font, brush, new PointF(x, y), format);
                x += MeasureTextElement(graphics, element, font, format);
                if (index != elements.Count - 1) x += trackingPx;
            }
            y += lineHeight;
        }
    }

    private static float MeasureTrackedLine(Graphics graphics, string line, Font font, StringFormat format, float trackingPx)
        => MeasureTrackedLine(graphics, EnumerateTextElements(line), font, format, trackingPx);

    private static float MeasureTrackedLine(Graphics graphics, List<string> elements, Font font, StringFormat format, float trackingPx)
    {
        if (elements.Count == 0) return 0f;
        float width = 0f;
        foreach (string element in elements)
            width += MeasureTextElement(graphics, element, font, format);
        return width + trackingPx * Math.Max(0, elements.Count - 1);
    }

    private static float MeasureTextElement(Graphics graphics, string element, Font font, StringFormat format)
        => graphics.MeasureString(element, font, 4096, format).Width;

    private static List<string> EnumerateTextElements(string text)
    {
        var elements = new List<string>();
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text.Normalize(NormalizationForm.FormC));
        while (enumerator.MoveNext())
            elements.Add(enumerator.GetTextElement());
        return elements;
    }

    private static Rectangle FindVisibleInkBounds(Bitmap bitmap)
    {
        int minX = bitmap.Width;
        int minY = bitmap.Height;
        int maxX = -1;
        int maxY = -1;
        for (int y = 0; y < bitmap.Height; y++)
        for (int x = 0; x < bitmap.Width; x++)
        {
            Color pixel = bitmap.GetPixel(x, y);
            if (pixel.R <= 3 && pixel.G <= 3 && pixel.B <= 3) continue;
            minX = Math.Min(minX, x);
            minY = Math.Min(minY, y);
            maxX = Math.Max(maxX, x);
            maxY = Math.Max(maxY, y);
        }

        return maxX < minX || maxY < minY
            ? Rectangle.Empty
            : Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
    }

    private static byte[] BuildGeneratedPrefabGrid(
        string text,
        string prefab,
        int width,
        int height,
        float safeWidth,
        float safeHeight,
        float titleScale,
        int letterSpacing,
        float titleX,
        float titleY)
    {
        string art = AsciiTitlePrefabGenerator.Generate(text, prefab, letterSpacing).Replace("\r", string.Empty);
        Rune[][] rawLines = art.Split('\n').Select(line => line.EnumerateRunes().ToArray()).ToArray();
        if (rawLines.Length == 0) rawLines = [new[] { new Rune(' ') }];

        // FIGlet fonts often contain asymmetric outer padding. Center the visible glyph bounds,
        // not the font's invisible padding, so title_x/title_y == 0 is visually centered.
        int minInkX = int.MaxValue;
        int maxInkX = -1;
        int minInkY = int.MaxValue;
        int maxInkY = -1;
        for (int y = 0; y < rawLines.Length; y++)
        {
            Rune[] line = rawLines[y];
            bool rowHasInk = false;
            for (int x = 0; x < line.Length; x++)
            {
                if (Rune.IsWhiteSpace(line[x])) continue;
                rowHasInk = true;
                minInkX = Math.Min(minInkX, x);
                maxInkX = Math.Max(maxInkX, x);
            }
            if (rowHasInk)
            {
                minInkY = Math.Min(minInkY, y);
                maxInkY = Math.Max(maxInkY, y);
            }
        }

        Rune[][] lines;
        if (maxInkX >= minInkX && maxInkY >= minInkY)
        {
            int cropWidth = maxInkX - minInkX + 1;
            lines = rawLines
                .Skip(minInkY)
                .Take(maxInkY - minInkY + 1)
                .Select(line => line.Length <= minInkX
                    ? Array.Empty<Rune>()
                    : line.Skip(minInkX).Take(Math.Min(cropWidth, line.Length - minInkX)).ToArray())
                .ToArray();
        }
        else
        {
            lines = rawLines;
        }

        int artWidth = Math.Max(1, lines.Max(line => line.Length));
        int artHeight = Math.Max(1, lines.Length);

        int safeCellsW = Math.Max(1, (int)Math.Floor(width * safeWidth));
        int safeCellsH = Math.Max(1, (int)Math.Floor(height * safeHeight));
        double fitScale = Math.Min(safeCellsW / (double)artWidth, safeCellsH / (double)artHeight);
        // A value of 1 fits the banner. >1 grows it and <1 shrinks it.
        double scale = Math.Clamp(fitScale * titleScale, 0.08, 8.0);
        int drawW = Math.Max(1, (int)Math.Round(artWidth * scale));
        int drawH = Math.Max(1, (int)Math.Round(artHeight * scale));
        int travelX = Math.Max(0, (width - drawW) / 2);
        int travelY = Math.Max(0, (height - drawH) / 2);
        int startX = (width - drawW) / 2 + (int)Math.Round(titleX * travelX);
        int startY = (height - drawH) / 2 - (int)Math.Round(titleY * travelY);

        var mask = new byte[width * height * 4];

        for (int dy = 0; dy < drawH; dy++)
        {
            int sy = Math.Clamp((int)Math.Floor(dy / scale), 0, artHeight - 1);
            Rune[] sourceLine = lines[sy];
            for (int dx = 0; dx < drawW; dx++)
            {
                int sx = Math.Clamp((int)Math.Floor(dx / scale), 0, artWidth - 1);
                if (sx >= sourceLine.Length) continue;
                Rune rune = sourceLine[sx];
                if (rune.Value == ' ' || rune.Value == '\t') continue;

                int x = startX + dx;
                int y = startY + dy;
                if ((uint)x >= (uint)width || (uint)y >= (uint)height) continue;
                int offset = (y * width + x) * 4;
                mask[offset] = 255;
                mask[offset + 1] = 255;
                mask[offset + 2] = 255;
                mask[offset + 3] = 255;

            }
        }

        return mask;
    }


}

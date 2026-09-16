namespace Glyphore;

internal sealed partial class GlPreviewControl
{
    private unsafe void EnsureRasterExportTarget(int width, int height)
    {
        if (_rasterExportTex != 0 && _rasterExportFbo != 0 && _rasterExportW == width && _rasterExportH == height) return;
        if (_rasterExportFbo != 0) NativeGl.DeleteFramebuffers(1, ref _rasterExportFbo);
        if (_rasterExportTex != 0) NativeGl.DeleteTextures(1, ref _rasterExportTex);
        _rasterExportFbo = 0;
        _rasterExportTex = 0;

        uint texture;
        NativeGl.GenTextures(1, &texture);
        _rasterExportTex = texture;
        NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, _rasterExportTex);
        NativeGl.TexParameteri(NativeGl.GL_TEXTURE_2D, NativeGl.GL_TEXTURE_MIN_FILTER, (int)NativeGl.GL_LINEAR);
        NativeGl.TexParameteri(NativeGl.GL_TEXTURE_2D, NativeGl.GL_TEXTURE_MAG_FILTER, (int)NativeGl.GL_LINEAR);
        NativeGl.TexImage2D(NativeGl.GL_TEXTURE_2D, 0, (int)NativeGl.GL_RGBA8, width, height, 0, NativeGl.GL_RGBA, NativeGl.GL_UNSIGNED_BYTE, IntPtr.Zero);

        uint fbo;
        NativeGl.GenFramebuffers(1, &fbo);
        _rasterExportFbo = fbo;
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _rasterExportFbo);
        NativeGl.FramebufferTexture2D(NativeGl.GL_FRAMEBUFFER, NativeGl.GL_COLOR_ATTACHMENT0, NativeGl.GL_TEXTURE_2D, _rasterExportTex, 0);
        if (NativeGl.CheckFramebufferStatus(NativeGl.GL_FRAMEBUFFER) != NativeGl.GL_FRAMEBUFFER_COMPLETE)
            throw new InvalidOperationException("FBO RGBA de exportación incompleto");

        _rasterExportW = width;
        _rasterExportH = height;
    }

    private unsafe RasterFrame CaptureRasterFromCurrentBuffers(
        EffectSettings outputSettings,
        GlyphoreScene? scene,
        double sourceTimeSeconds,
        bool useCompositedColor,
        bool useCompositedGlyph,
        bool transparent,
        Color background,
        byte[]? reusableRgba = null)
    {
        int cols = Math.Max(2, outputSettings.Width);
        int rows = Math.Max(2, outputSettings.Height);
        int cellH = Math.Clamp(900 / rows, 10, 28);
        int cellW = Math.Max(5, (int)Math.Round(cellH * .55));
        int width = checked(cols * cellW);
        int height = checked(rows * cellH);
        EnsureRasterExportTarget(width, height);

        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _rasterExportFbo);
        NativeGl.Viewport(0, 0, width, height);
        NativeGl.glClearColor(0, 0, 0, transparent ? 0f : 1f);
        NativeGl.glClear(NativeGl.GL_COLOR_BUFFER_BIT);
        NativeGl.Disable(NativeGl.GL_BLEND);
        NativeGl.UseProgram(_previewProgram);
        NativeGl.Uniform2i(UniformLocation(_previewProgram, "u_grid"), cols, rows);
        U2(_previewProgram, "u_view", width, height);
        Ui(_previewProgram, "u_preview_mode", (int)PreviewViewMode.Stretch);
        U1(_previewProgram, "u_preview_zoom", 1f);
        Ui(_previewProgram, "u_output_transparent", transparent ? 1 : 0);
        U1(_previewProgram, "u_cell_aspect", .55f);
        SetPreviewGlyphUniforms(outputSettings, useCompositedColor, useCompositedGlyph);
        SetSceneTransformUniforms(_previewProgram, scene?.Transform);
        SetPostProcessUniforms(_previewProgram, scene?.PostProcess);
        U1(_previewProgram, "u_post_time", (float)sourceTimeSeconds);
        U3(_previewProgram, "u_background_color", background.R / 255f, background.G / 255f, background.B / 255f);
        Ui(_previewProgram, "u_preview_background_mode", 0);
        BindEditorMaskUniforms(_previewProgram, enabled: false);

        NativeGl.ActiveTexture(NativeGl.GL_TEXTURE0);
        NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, _atlas);
        Ui(_previewProgram, "u_atlas", 0);
        NativeGl.ActiveTexture(NativeGl.GL_TEXTURE0 + 1);
        NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, _intensityTex);
        Ui(_previewProgram, "u_intensity", 1);
        NativeGl.ActiveTexture(NativeGl.GL_TEXTURE0 + 2);
        NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, _colorTex);
        Ui(_previewProgram, "u_color", 2);
        NativeGl.ActiveTexture(NativeGl.GL_TEXTURE0 + 3);
        NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, _glyphSelectTex);
        Ui(_previewProgram, "u_glyph_select", 3);
        NativeGl.BindVertexArray(_vao);
        NativeGl.DrawArrays(NativeGl.GL_TRIANGLES, 0, 3);
        NativeGl.Finish();

        int rgbaLength = checked(width * height * 4);
        byte[] rgba = reusableRgba is { Length: var length } && length == rgbaLength
            ? reusableRgba
            : new byte[rgbaLength];
        NativeGl.PixelStorei(NativeGl.GL_PACK_ALIGNMENT, 1);
        fixed (byte* p = rgba)
            NativeGl.ReadPixels(0, 0, width, height, NativeGl.GL_RGBA, NativeGl.GL_UNSIGNED_BYTE, (IntPtr)p);
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, 0);

        // glReadPixels is bottom-up. Flip in place so export needs one frame buffer rather than
        // simultaneously retaining bottom-up and top-down copies (another ~8 MiB at 1080p).
        int stride = width * 4;
        byte[] row = new byte[stride];
        for (int y = 0; y < height / 2; y++)
        {
            int top = y * stride;
            int bottom = (height - 1 - y) * stride;
            Buffer.BlockCopy(rgba, top, row, 0, stride);
            Buffer.BlockCopy(rgba, bottom, rgba, top, stride);
            Buffer.BlockCopy(row, 0, rgba, bottom, stride);
        }
        return new RasterFrame(width, height, rgba);
    }
}

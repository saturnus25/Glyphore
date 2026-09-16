using System.Diagnostics;

namespace Glyphore;

internal sealed partial class GlPreviewControl
{
    public double BenchmarkGpu(int frames = 240)
    {
        if (!_loaded) return double.NaN;
        MakeCurrent();
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < frames; i++)
        {
            double sourceTime = i / 120.0;
            EffectSettings outputSettings;
            bool useCompositedColor = false;
            bool useCompositedGlyph = false;

            if (_scene is { Layers.Count: > 0 } scene)
            {
                SyncAtlasIfNeeded();
                RenderSceneIntensity(scene, sourceTime);
                RenderSceneColor(scene, sourceTime);
                RenderSceneGlyphSelection(scene, sourceTime);
                outputSettings = SceneOutputSettings(scene);
                useCompositedColor = true;
                useCompositedGlyph = true;
            }
            else
            {
                double animationTime = sourceTime * _settings.Get("speed");
                RenderIntensity(_settings, (float)animationTime, (float)(animationTime * _settings.Get("time_freq")));
                outputSettings = _settings;
            }

            NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, 0);
            NativeGl.Viewport(0, 0, Math.Max(1, Width), Math.Max(1, Height));
            NativeGl.UseProgram(_previewProgram);
            NativeGl.Uniform2i(UniformLocation(_previewProgram, "u_grid"), Math.Max(2, outputSettings.Width), Math.Max(2, outputSettings.Height));
            U2(_previewProgram, "u_view", Math.Max(1, Width), Math.Max(1, Height));
            Ui(_previewProgram, "u_preview_mode", (int)PreviewViewMode);
            U1(_previewProgram, "u_preview_zoom", (float)PreviewZoom);
            Ui(_previewProgram, "u_output_transparent", 0);
            U1(_previewProgram, "u_cell_aspect", 0.55f);
            SetPreviewGlyphUniforms(outputSettings, useCompositedColor, useCompositedGlyph);
            SetSceneTransformUniforms(_previewProgram, _scene?.Transform);
            SetPostProcessUniforms(_previewProgram, _scene?.PostProcess);
            U1(_previewProgram, "u_post_time", (float)sourceTime);
            U3(_previewProgram, "u_background_color", PreviewBackgroundColor.R / 255f, PreviewBackgroundColor.G / 255f, PreviewBackgroundColor.B / 255f);
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
            NativeGl.DrawArrays(NativeGl.GL_TRIANGLES, 0, 3);
        }

        NativeGl.Finish();
        sw.Stop();
        Invalidate();
        return sw.Elapsed.TotalMilliseconds / frames;
    }
}

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Glyphore;

internal sealed partial class GlPreviewControl
{
    private unsafe void InitializeOpenGl()
    {
        _dc = NativeGl.GetDC(Handle);
        if (_dc == IntPtr.Zero) throw new InvalidOperationException("GetDC falló");
        var pfd = NativeGl.DefaultPfd();
        int pf = NativeGl.ChoosePixelFormat(_dc, ref pfd);
        if (pf == 0 || !NativeGl.SetPixelFormat(_dc, pf, ref pfd)) throw new InvalidOperationException("No se pudo configurar el pixel format OpenGL");

        IntPtr temp = NativeGl.wglCreateContext(_dc);
        if (temp == IntPtr.Zero || !NativeGl.wglMakeCurrent(_dc, temp)) throw new InvalidOperationException("No se pudo crear el contexto OpenGL temporal");

        IntPtr createPtr = wglGetProcAddress("wglCreateContextAttribsARB");
        if (IsValidWglProcAddress(createPtr))
        {
            var create = Marshal.GetDelegateForFunctionPointer<WglCreateContextAttribs>(createPtr);
            int[] attrs = [WGL_CONTEXT_MAJOR_VERSION_ARB, 3, WGL_CONTEXT_MINOR_VERSION_ARB, 3, WGL_CONTEXT_PROFILE_MASK_ARB, WGL_CONTEXT_CORE_PROFILE_BIT_ARB, 0];
            IntPtr modern = create(_dc, IntPtr.Zero, attrs);
            if (modern != IntPtr.Zero)
            {
                NativeGl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                NativeGl.wglDeleteContext(temp);
                temp = modern;
                NativeGl.wglMakeCurrent(_dc, temp);
            }
        }
        _rc = temp;
        NativeGl.Load();
        UniformLocationCache.Clear();

        string vert = ReadResource("fullscreen.vert.glsl");
        string effects = ReadResource("effects.glsl");
        string preview = ReadResource("preview.frag.glsl");
        string intensity = ReadResource("intensity.frag.glsl").Replace("/*__EFFECTS__*/", effects);
        _previewProgram = NativeGl.CompileProgram(vert, preview);
        _captureProgram = NativeGl.CompileProgram(vert, intensity);
        uint vao;
        NativeGl.GenVertexArrays(1, &vao);
        _vao = vao;
        NativeGl.BindVertexArray(_vao);
        BuildGlyphAtlas(_settings.Charset);
        _gpuInfo = GetGlString(GL_RENDERER) + " · OpenGL " + GetGlString(GL_VERSION);
        _loaded = true;
    }

    private static string ReadResource(string suffix)
    {
        var asm = Assembly.GetExecutingAssembly();
        var name = asm.GetManifestResourceNames().First(n => n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        using var sr = new StreamReader(asm.GetManifestResourceStream(name)!);
        return sr.ReadToEnd();
    }

    private static string GetGlString(uint name)
    {
        var p = NativeGl.GetString(name);
        return p == IntPtr.Zero ? "?" : Marshal.PtrToStringAnsi(p) ?? "?";
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        System.Threading.Interlocked.Exchange(ref _paintQueued, 0);
        if (!_loaded)
        {
            e.Graphics.Clear(PreviewBackgroundColor);
            using var b = new SolidBrush(Color.Gainsboro);
            e.Graphics.DrawString(_gpuInfo, Font, b, 12, 12);
            return;
        }
        var sw = Stopwatch.StartNew();
        try
        {
            MakeCurrent();
            SyncAtlasIfNeeded();
            EffectSettings outputSettings;
            bool useCompositedColor = false;
            bool useCompositedGlyph = false;
            if (_scene is { Layers.Count: > 0 } scene)
            {
                AdvanceSceneClocks(scene);
                RenderSceneIntensity(scene);
                RenderSceneColor(scene);
                RenderSceneGlyphSelection(scene);
                outputSettings = SceneOutputSettings(scene);
                useCompositedColor = true;
                useCompositedGlyph = true;
            }
            else
            {
                var previewTimes = AdvancePreviewClocks();
                RenderIntensity(_settings, (float)previewTimes.Animation, (float)previewTimes.Temporal);
                outputSettings = _settings;
            }

            NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, 0);
            NativeGl.Viewport(0, 0, Math.Max(1, Width), Math.Max(1, Height));
            NativeGl.glClearColor(
                PreviewBackgroundColor.R / 255f,
                PreviewBackgroundColor.G / 255f,
                PreviewBackgroundColor.B / 255f,
                1f);
            NativeGl.glClear(NativeGl.GL_COLOR_BUFFER_BIT);
            NativeGl.UseProgram(_previewProgram);
            NativeGl.Uniform2i(UniformLocation(_previewProgram, "u_grid"), Math.Max(2, outputSettings.Width), Math.Max(2, outputSettings.Height));
            U2(_previewProgram, "u_view", Width, Height);
            Ui(_previewProgram, "u_preview_mode", (int)PreviewViewMode);
            U1(_previewProgram, "u_preview_zoom", (float)PreviewZoom);
            Ui(_previewProgram, "u_output_transparent", 0);
            U1(_previewProgram, "u_cell_aspect", 0.55f);
            SetPreviewGlyphUniforms(outputSettings, useCompositedColor, useCompositedGlyph);
            SetSceneTransformUniforms(_previewProgram, _scene?.Transform);
            SetPostProcessUniforms(_previewProgram, _scene?.PostProcess);
            U1(_previewProgram, "u_post_time", (float)CurrentTimeSeconds);
            U3(_previewProgram, "u_background_color", PreviewBackgroundColor.R / 255f, PreviewBackgroundColor.G / 255f, PreviewBackgroundColor.B / 255f);
            BindEditorMaskUniforms(_previewProgram);
            NativeGl.ActiveTexture(NativeGl.GL_TEXTURE0); NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, _atlas); Ui(_previewProgram, "u_atlas", 0);
            NativeGl.ActiveTexture(NativeGl.GL_TEXTURE0+1); NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, _intensityTex); Ui(_previewProgram, "u_intensity", 1);
            NativeGl.ActiveTexture(NativeGl.GL_TEXTURE0+2); NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, _colorTex); Ui(_previewProgram, "u_color", 2);
            NativeGl.ActiveTexture(NativeGl.GL_TEXTURE0+3); NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, _glyphSelectTex); Ui(_previewProgram, "u_glyph_select", 3);
            NativeGl.BindVertexArray(_vao); NativeGl.DrawArrays(NativeGl.GL_TRIANGLES, 0, 3);
            NativeGl.SwapBuffers(_dc);
        }
        catch (Exception ex) { _gpuInfo = "Render error: " + ex.Message; }
        sw.Stop();
        _frames++;
        if (_fpsClock.ElapsedMilliseconds >= 500)
        {
            _actualFps = _frames / _fpsClock.Elapsed.TotalSeconds; _frames = 0; _fpsClock.Restart();
            FrameStats?.Invoke(_actualFps, sw.Elapsed.TotalMilliseconds, _gpuInfo);
        }
    }
}

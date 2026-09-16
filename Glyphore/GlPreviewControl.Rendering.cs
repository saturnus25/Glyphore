using System.Drawing.Imaging;
using System.Text;

namespace Glyphore;

internal sealed partial class GlPreviewControl
{
    private unsafe void EnsureIntensityTarget(int w, int h)
    {
        if (_intensityTex != 0 && _colorTex != 0 && _glyphSelectTex != 0 && _intensityW == w && _intensityH == h) return;
        if (_intensityFbo != 0) NativeGl.DeleteFramebuffers(1, ref _intensityFbo);
        if (_intensityTex != 0) NativeGl.DeleteTextures(1, ref _intensityTex);
        if (_colorFbo != 0) NativeGl.DeleteFramebuffers(1, ref _colorFbo);
        if (_colorTex != 0) NativeGl.DeleteTextures(1, ref _colorTex);
        if (_glyphSelectFbo != 0) NativeGl.DeleteFramebuffers(1, ref _glyphSelectFbo);
        if (_glyphSelectTex != 0) NativeGl.DeleteTextures(1, ref _glyphSelectTex);
        _intensityTex = 0;
        _intensityFbo = 0;
        _colorTex = 0;
        _colorFbo = 0;
        _glyphSelectTex = 0;
        _glyphSelectFbo = 0;

        uint intensityTex;
        NativeGl.GenTextures(1, &intensityTex);
        _intensityTex = intensityTex;
        NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, _intensityTex);
        NativeGl.TexParameteri(NativeGl.GL_TEXTURE_2D, NativeGl.GL_TEXTURE_MIN_FILTER, (int)NativeGl.GL_NEAREST);
        NativeGl.TexParameteri(NativeGl.GL_TEXTURE_2D, NativeGl.GL_TEXTURE_MAG_FILTER, (int)NativeGl.GL_NEAREST);
        NativeGl.TexImage2D(NativeGl.GL_TEXTURE_2D, 0, (int)NativeGl.GL_RGBA8, w, h, 0, NativeGl.GL_RGBA, NativeGl.GL_UNSIGNED_BYTE, IntPtr.Zero);

        uint intensityFbo;
        NativeGl.GenFramebuffers(1, &intensityFbo);
        _intensityFbo = intensityFbo;
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _intensityFbo);
        NativeGl.FramebufferTexture2D(NativeGl.GL_FRAMEBUFFER, NativeGl.GL_COLOR_ATTACHMENT0, NativeGl.GL_TEXTURE_2D, _intensityTex, 0);
        if (NativeGl.CheckFramebufferStatus(NativeGl.GL_FRAMEBUFFER) != NativeGl.GL_FRAMEBUFFER_COMPLETE)
            throw new InvalidOperationException("FBO de intensidad incompleto");

        uint colorTex;
        NativeGl.GenTextures(1, &colorTex);
        _colorTex = colorTex;
        NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, _colorTex);
        NativeGl.TexParameteri(NativeGl.GL_TEXTURE_2D, NativeGl.GL_TEXTURE_MIN_FILTER, (int)NativeGl.GL_NEAREST);
        NativeGl.TexParameteri(NativeGl.GL_TEXTURE_2D, NativeGl.GL_TEXTURE_MAG_FILTER, (int)NativeGl.GL_NEAREST);
        NativeGl.TexImage2D(NativeGl.GL_TEXTURE_2D, 0, (int)NativeGl.GL_RGBA8, w, h, 0, NativeGl.GL_RGBA, NativeGl.GL_UNSIGNED_BYTE, IntPtr.Zero);

        uint colorFbo;
        NativeGl.GenFramebuffers(1, &colorFbo);
        _colorFbo = colorFbo;
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _colorFbo);
        NativeGl.FramebufferTexture2D(NativeGl.GL_FRAMEBUFFER, NativeGl.GL_COLOR_ATTACHMENT0, NativeGl.GL_TEXTURE_2D, _colorTex, 0);
        if (NativeGl.CheckFramebufferStatus(NativeGl.GL_FRAMEBUFFER) != NativeGl.GL_FRAMEBUFFER_COMPLETE)
            throw new InvalidOperationException("FBO de color incompleto");

        uint glyphSelectTex;
        NativeGl.GenTextures(1, &glyphSelectTex);
        _glyphSelectTex = glyphSelectTex;
        NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, _glyphSelectTex);
        NativeGl.TexParameteri(NativeGl.GL_TEXTURE_2D, NativeGl.GL_TEXTURE_MIN_FILTER, (int)NativeGl.GL_NEAREST);
        NativeGl.TexParameteri(NativeGl.GL_TEXTURE_2D, NativeGl.GL_TEXTURE_MAG_FILTER, (int)NativeGl.GL_NEAREST);
        NativeGl.TexImage2D(NativeGl.GL_TEXTURE_2D, 0, (int)NativeGl.GL_RGBA8, w, h, 0, NativeGl.GL_RGBA, NativeGl.GL_UNSIGNED_BYTE, IntPtr.Zero);

        uint glyphSelectFbo;
        NativeGl.GenFramebuffers(1, &glyphSelectFbo);
        _glyphSelectFbo = glyphSelectFbo;
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _glyphSelectFbo);
        NativeGl.FramebufferTexture2D(NativeGl.GL_FRAMEBUFFER, NativeGl.GL_COLOR_ATTACHMENT0, NativeGl.GL_TEXTURE_2D, _glyphSelectTex, 0);
        if (NativeGl.CheckFramebufferStatus(NativeGl.GL_FRAMEBUFFER) != NativeGl.GL_FRAMEBUFFER_COMPLETE)
            throw new InvalidOperationException("FBO de glifos incompleto");

        _intensityW = w;
        _intensityH = h;
    }

    private (double Animation, double Temporal) AdvancePreviewClocks()
    {
        double timeline = CurrentTimeSeconds;
        if (!_previewClockInitialized)
        {
            _previewClockInitialized = true;
            _lastTimelineTime = timeline;
            return (_animationTime, _temporalTime);
        }

        double dt = Math.Clamp(timeline - _lastTimelineTime, 0.0, 0.25);
        _lastTimelineTime = timeline;
        double speed = _settings.Get("speed");
        _animationTime += dt * speed;
        _temporalTime += dt * speed * _settings.Get("time_freq");
        return (_animationTime, _temporalTime);
    }

    private EffectSettings GetLayerRenderSettings(GlyphoreScene scene, SceneEffectLayer layer)
    {
        if (!_layerRenderSettings.TryGetValue(layer.Id, out var cached))
        {
            cached = new LayerRenderSettings();
            _layerRenderSettings[layer.Id] = cached;
        }

        int commonRevision = scene.CommonRevision;
        int layerRevision = layer.SettingsRevision;
        if (cached.CommonRevision != commonRevision || cached.LayerRevision != layerRevision)
        {
            cached.Settings = scene.CreateSettings(layer, out commonRevision, out layerRevision);
            cached.CommonRevision = commonRevision;
            cached.LayerRevision = layerRevision;
        }

        return cached.Settings;
    }

    private void AdvanceSceneClocks(GlyphoreScene scene)
    {
        double timeline = CurrentTimeSeconds;
        if (!_sceneClockInitialized)
        {
            _sceneClockInitialized = true;
            _lastSceneTimelineTime = timeline;
        }

        double dt = Math.Clamp(timeline - _lastSceneTimelineTime, 0.0, 0.25);
        _lastSceneTimelineTime = timeline;

        foreach (var layer in scene.Layers)
        {
            if (!_layerClocks.TryGetValue(layer.Id, out var clock))
            {
                clock = new LayerClockState();
                _layerClocks[layer.Id] = clock;
            }

            var settings = GetLayerRenderSettings(scene, layer);
            double speed = settings.Get("speed");
            clock.Animation += dt * speed;
            clock.Temporal += dt * speed * settings.Get("time_freq");
        }
    }

    private (double Animation, double Temporal) ResolveSceneLayerTime(
        SceneEffectLayer layer,
        EffectSettings settings,
        double? sourceTimeSeconds)
    {
        if (sourceTimeSeconds is double sourceTime)
        {
            double animation = sourceTime * settings.Get("speed");
            return (animation, animation * settings.Get("time_freq"));
        }

        return _layerClocks.TryGetValue(layer.Id, out var clock)
            ? (clock.Animation, clock.Temporal)
            : default;
    }

    private void RenderIntensity(EffectSettings settings, float animationTime, float temporalTime)
    {
        int w = Math.Max(2, settings.Width);
        int h = Math.Max(2, settings.Height);
        EnsureIntensityTarget(w, h);
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _intensityFbo);
        NativeGl.Viewport(0, 0, w, h);
        NativeGl.Disable(NativeGl.GL_BLEND);
        NativeGl.UseProgram(_captureProgram);
        SetCommonUniforms(_captureProgram, settings, animationTime, temporalTime);
        BindLayerMasks(null);
        Ui(_captureProgram, "u_title_enabled", 0);
        Ui(_captureProgram, "u_output_color", 0);
        Ui(_captureProgram, "u_output_glyph", 0);
        U1(_captureProgram, "u_color_gamma", settings.F("gamma"));
        Ui(_captureProgram, "u_color_invert", settings.Invert ? 1 : 0);
        U1(_captureProgram, "u_layer_opacity", 1f);
        Ui(_captureProgram, "u_layer_mode", 0);
        NativeGl.BindVertexArray(_vao);
        NativeGl.DrawArrays(NativeGl.GL_TRIANGLES, 0, 3);
    }

    private void RenderColor(EffectSettings settings, float animationTime, float temporalTime)
    {
        int w = Math.Max(2, settings.Width);
        int h = Math.Max(2, settings.Height);
        EnsureIntensityTarget(w, h);
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _colorFbo);
        NativeGl.Viewport(0, 0, w, h);
        NativeGl.glClearColor(0, 0, 0, 0);
        NativeGl.glClear(NativeGl.GL_COLOR_BUFFER_BIT);
        NativeGl.Disable(NativeGl.GL_BLEND);
        NativeGl.UseProgram(_captureProgram);
        SetCommonUniforms(_captureProgram, settings, animationTime, temporalTime);
        BindLayerMasks(null);
        Ui(_captureProgram, "u_title_enabled", 0);
        Ui(_captureProgram, "u_output_color", 1);
        Ui(_captureProgram, "u_output_glyph", 0);
        U1(_captureProgram, "u_color_gamma", settings.F("gamma"));
        Ui(_captureProgram, "u_color_invert", settings.Invert ? 1 : 0);
        SetPaletteUniforms(_captureProgram, settings.PaletteStops);
        U1(_captureProgram, "u_layer_opacity", 1f);
        Ui(_captureProgram, "u_layer_mode", 0);
        NativeGl.BindVertexArray(_vao);
        NativeGl.DrawArrays(NativeGl.GL_TRIANGLES, 0, 3);
    }

    private void RenderSceneIntensity(GlyphoreScene scene, double? sourceTimeSeconds = null)
        => RenderSceneComposite(scene, colorOutput: false, sourceTimeSeconds);

    private void RenderSceneColor(GlyphoreScene scene, double? sourceTimeSeconds = null)
        => RenderSceneComposite(scene, colorOutput: true, sourceTimeSeconds);

    private void RenderSceneGlyphSelection(GlyphoreScene scene, double? sourceTimeSeconds = null)
    {
        int w = Math.Max(2, scene.Width);
        int h = Math.Max(2, scene.Height);
        EnsureIntensityTarget(w, h);
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, _glyphSelectFbo);
        NativeGl.Viewport(0, 0, w, h);
        NativeGl.glClearColor(0, 0, 0, 0);
        NativeGl.glClear(NativeGl.GL_COLOR_BUFFER_BIT);
        NativeGl.Disable(NativeGl.GL_BLEND);

        NativeGl.UseProgram(_captureProgram);
        NativeGl.BindVertexArray(_vao);
        Ui(_captureProgram, "u_output_color", 0);
        Ui(_captureProgram, "u_output_glyph", 1);
        U1(_captureProgram, "u_color_gamma", (float)scene.Gamma);
        Ui(_captureProgram, "u_color_invert", scene.Invert ? 1 : 0);
        NativeGl.ActiveTexture(NativeGl.GL_TEXTURE0 + 3);
        NativeGl.BindTexture(NativeGl.GL_TEXTURE_2D, _glyphMapTex);
        Ui(_captureProgram, "u_glyph_map", 3);

        // Glyph ownership is deterministic: lower layers are written first, then an upper
        // visible layer replaces the glyph for cells where that layer actually has content.
        // Color/intensity still use their configured blend mode independently.
        for (int i = scene.Layers.Count - 1; i >= 0; i--)
        {
            var layer = scene.Layers[i];
            if (!layer.Visible || layer.Opacity <= 0.0001) continue;
            if (!_layerGlyphRows.TryGetValue(layer.Id, out var glyphRow)) continue;

            var settings = GetLayerRenderSettings(scene, layer);
            var time = ResolveSceneLayerTime(layer, settings, sourceTimeSeconds);
            SetCommonUniforms(_captureProgram, settings, (float)time.Animation, (float)time.Temporal);
            BindLayerMasks(layer);
            BindLayerTitleTexture(layer, settings, ResolveTitleAnimationTime(settings, sourceTimeSeconds));
            U1(_captureProgram, "u_layer_opacity", (float)Math.Clamp(layer.Opacity, 0.0, 1.0));
            Ui(_captureProgram, "u_layer_mode", layer.BlendMode == LayerBlendMode.Additive ? 2 : (scene.RespectLayerOrder ? 0 : 1));
            Ui(_captureProgram, "u_layer_glyph_slot", glyphRow.Slot);
            Ui(_captureProgram, "u_layer_glyph_count", Math.Max(1, glyphRow.Count));
            NativeGl.DrawArrays(NativeGl.GL_TRIANGLES, 0, 3);
        }

        Ui(_captureProgram, "u_output_glyph", 0);
    }

    private void RenderSceneComposite(GlyphoreScene scene, bool colorOutput, double? sourceTimeSeconds)
    {
        int w = Math.Max(2, scene.Width);
        int h = Math.Max(2, scene.Height);
        EnsureIntensityTarget(w, h);
        NativeGl.BindFramebuffer(NativeGl.GL_FRAMEBUFFER, colorOutput ? _colorFbo : _intensityFbo);
        NativeGl.Viewport(0, 0, w, h);
        NativeGl.glClearColor(0, 0, 0, 0);
        NativeGl.glClear(NativeGl.GL_COLOR_BUFFER_BIT);

        var visibleLayers = scene.Layers
            .Where(layer => layer.Visible && layer.Opacity > 0.0001)
            .ToList();

        if (visibleLayers.Count == 0)
        {
            NativeGl.Disable(NativeGl.GL_BLEND);
            return;
        }

        NativeGl.UseProgram(_captureProgram);
        NativeGl.BindVertexArray(_vao);
        Ui(_captureProgram, "u_output_color", colorOutput ? 1 : 0);
        Ui(_captureProgram, "u_output_glyph", 0);
        U1(_captureProgram, "u_color_gamma", (float)scene.Gamma);
        Ui(_captureProgram, "u_color_invert", scene.Invert ? 1 : 0);

        // Preserve the legacy single-layer intensity path so introducing premultiplied RGBA
        // composition does not change glyph selection/brightness for ordinary scenes. Colour
        // output deliberately always goes through blending, including one layer, so its texture
        // has one consistent premultiplied-alpha representation.
        if (!colorOutput && visibleLayers.Count == 1 && visibleLayers[0] is { BlendMode: LayerBlendMode.Normal, Opacity: >= 0.9999 } singleLayer)
        {
            var settings = GetLayerRenderSettings(scene, singleLayer);
            var time = ResolveSceneLayerTime(singleLayer, settings, sourceTimeSeconds);
            NativeGl.Disable(NativeGl.GL_BLEND);
            SetCommonUniforms(_captureProgram, settings, (float)time.Animation, (float)time.Temporal);
            BindLayerMasks(singleLayer);
            BindLayerTitleTexture(singleLayer, settings, ResolveTitleAnimationTime(settings, sourceTimeSeconds));
            U1(_captureProgram, "u_layer_opacity", 1f);
            Ui(_captureProgram, "u_layer_mode", 0);
            NativeGl.DrawArrays(NativeGl.GL_TRIANGLES, 0, 3);
            return;
        }

        NativeGl.Enable(NativeGl.GL_BLEND);

        // The first item in the list is the visual top layer. Draw bottom-to-top so
        // Normal alpha blending can make that ordering deterministic.
        for (int i = scene.Layers.Count - 1; i >= 0; i--)
        {
            var layer = scene.Layers[i];
            if (!layer.Visible || layer.Opacity <= 0.0001) continue;

            switch (layer.BlendMode)
            {
                case LayerBlendMode.Additive:
                    NativeGl.BlendFuncSeparate(NativeGl.GL_SRC_ALPHA, NativeGl.GL_ONE, NativeGl.GL_ONE, NativeGl.GL_ONE);
                    Ui(_captureProgram, "u_layer_mode", 2);
                    break;
                default:
                    NativeGl.BlendFuncSeparate(NativeGl.GL_SRC_ALPHA, NativeGl.GL_ONE_MINUS_SRC_ALPHA, NativeGl.GL_ONE, NativeGl.GL_ONE_MINUS_SRC_ALPHA);
                    Ui(_captureProgram, "u_layer_mode", scene.RespectLayerOrder ? 0 : 1);
                    break;
            }

            var settings = GetLayerRenderSettings(scene, layer);
            var time = ResolveSceneLayerTime(layer, settings, sourceTimeSeconds);
            SetCommonUniforms(_captureProgram, settings, (float)time.Animation, (float)time.Temporal);
            BindLayerMasks(layer);
            BindLayerTitleTexture(layer, settings, ResolveTitleAnimationTime(settings, sourceTimeSeconds));
            if (colorOutput) SetPaletteUniforms(_captureProgram, layer.PaletteStops);
            U1(_captureProgram, "u_layer_opacity", (float)Math.Clamp(layer.Opacity, 0.0, 1.0));
            NativeGl.DrawArrays(NativeGl.GL_TRIANGLES, 0, 3);
        }

        NativeGl.Disable(NativeGl.GL_BLEND);
    }


    private float ResolveTitleAnimationTime(EffectSettings settings, double? sourceTimeSeconds)
    {
        // Title animation deliberately uses an absolute paused-aware clock rather than the
        // per-layer integration cache. This keeps wave/shimmer/glitch moving in detached
        // previews and also makes exported frames deterministic from their requested time.
        double timeline = sourceTimeSeconds ?? CurrentTimeSeconds;
        return (float)(timeline * settings.Get("speed") * settings.Get("time_freq"));
    }

    private EffectSettings SceneOutputSettings(GlyphoreScene scene)
    {
        var layer = scene.ActiveLayer ?? scene.Layers.FirstOrDefault();
        return layer is null ? _settings : GetLayerRenderSettings(scene, layer);
    }

    private void MakeCurrent()
    {
        if (!NativeGl.wglMakeCurrent(_dc, _rc)) throw new InvalidOperationException("wglMakeCurrent falló");
    }

    private void SyncAtlasIfNeeded()
    {
        if (!_loaded && _rc == IntPtr.Zero) return;
        if (_scene is { Layers.Count: > 0 } scene)
        {
            SyncSceneGlyphResources(scene);
            return;
        }

        _sceneGlyphSignature = "";
        _layerGlyphRows.Clear();
        string charset = _settings.Charset;
        var ramp = string.IsNullOrEmpty(charset) ? " " : charset;
        int glyphCount = Math.Max(1, ramp.EnumerateRunes().Count());
        if (ramp == _atlasRamp)
        {
            _atlasGlyphCount = glyphCount;
            return;
        }
        if (_rc != IntPtr.Zero) MakeCurrent();
        BuildGlyphAtlas(ramp);
    }
}

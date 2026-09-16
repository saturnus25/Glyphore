using System.Drawing.Imaging;
using System.Text;

namespace Glyphore;

internal sealed partial class GlPreviewControl
{
    private void SetPreviewGlyphUniforms(EffectSettings outputSettings, bool useCompositedColor, bool useCompositedGlyph)
    {
        U1(_previewProgram, "u_gamma", outputSettings.F("gamma"));
        Ui(_previewProgram, "u_invert", outputSettings.Invert ? 1 : 0);
        Ui(_previewProgram, "u_color_enabled", outputSettings.ColorEnabled ? 1 : 0);
        Ui(_previewProgram, "u_use_composited_color", useCompositedColor ? 1 : 0);
        Ui(_previewProgram, "u_use_composited_glyph", useCompositedGlyph ? 1 : 0);
        U1(_previewProgram, "u_glyph_scale", (float)outputSettings.GlyphDisplayScale);
        Ui(_previewProgram, "u_glyph_count", Math.Max(1, outputSettings.Charset.EnumerateRunes().Count()));
        Ui(_previewProgram, "u_atlas_glyph_count", Math.Max(1, _atlasGlyphCount));
        NativeGl.Uniform2i(UniformLocation(_previewProgram, "u_atlas_grid"), _atlasCols, _atlasRows);
        SetPaletteUniforms(_previewProgram, outputSettings.PaletteStops);
    }

    private static void SetCommonUniforms(uint p, EffectSettings s, float animationTime, float temporalTime)
    {
        Ui(p, "u_effect", EffectRegistry.EffectId(s.Effect));
        Ui(p, "u_seed", s.Seed);
        NativeGl.Uniform2i(UniformLocation(p, "u_grid"), Math.Max(2, s.Width), Math.Max(2, s.Height));
        U1(p, "u_time", animationTime);
        U1(p, "u_temporal_time", temporalTime);
        U1(p, "u_scale", s.F("scale"));
        U1(p, "u_amp", s.F("osc_amp"));
        U1(p, "u_fx", s.F("freq_x"));
        U1(p, "u_fy", s.F("freq_y"));
        U1(p, "u_fd", s.F("freq_diag"));
        U1(p, "u_fr", s.F("freq_radial"));
        U1(p, "u_tf", s.F("time_freq"));
        U1(p, "u_phase", s.F("phase_deg"));
        U1(p, "u_turb", s.F("turbulence"));
        U1(p, "u_warp", s.F("warp"));
        U1(p, "u_dx", s.F("drift_x"));
        U1(p, "u_dy", s.F("drift_y"));
        U1(p, "u_pulse", s.F("pulse"));
        U1(p, "u_density", s.F("density"));
        U1(p, "u_aspect", s.F("aspect"));
        U1(p, "u_iterations", s.F("iterations"));
        foreach (var binding in ShaderUniformBindings.EffectSpecific)
            U1(p, binding.UniformName, s.F(binding.SettingKey));

        Ui(p, "u_shape_mode", EffectRegistry.ShapeId(s.ShapeMode));
    }



    private static void SetSceneTransformUniforms(uint p, SceneTransform? transform)
    {
        U1(p, "u_scene_rotation", transform is null ? 0f : (float)transform.Rotation);
        U1(p, "u_scene_perspective_x", transform is null ? 0f : (float)transform.PerspectiveX);
        U1(p, "u_scene_perspective_y", transform is null ? 0f : (float)transform.PerspectiveY);
    }

    private static void SetPostProcessUniforms(uint p, ScenePostProcess? post)
    {
        bool enabled = post?.Enabled == true;
        Ui(p, "u_post_enabled", enabled ? 1 : 0);
        U1(p, "u_post_exposure", enabled ? (float)post!.Exposure : 0f);
        U1(p, "u_post_contrast", enabled ? (float)post!.Contrast : 1f);
        U1(p, "u_post_saturation", enabled ? (float)post!.Saturation : 1f);
        U1(p, "u_post_bloom", enabled ? (float)post!.Bloom : 0f);
        U1(p, "u_post_bloom_radius", enabled ? (float)post!.BloomRadius : 1f);
        U1(p, "u_post_vignette", enabled ? (float)post!.Vignette : 0f);
        U1(p, "u_post_scanlines", enabled ? (float)post!.Scanlines : 0f);
        U1(p, "u_post_grain", enabled ? (float)post!.Grain : 0f);
        U1(p, "u_post_chromatic", enabled ? (float)post!.ChromaticAberration : 0f);
        U1(p, "u_post_posterize", enabled ? (float)post!.Posterize : 0f);
        U1(p, "u_post_threshold", enabled ? (float)post!.Threshold : 0f);
        U1(p, "u_post_blur", enabled ? (float)post!.Blur : 0f);
        U1(p, "u_post_sharpen", enabled ? (float)post!.Sharpen : 0f);
        U1(p, "u_post_pixelate", enabled ? (float)post!.Pixelate : 1f);
        U1(p, "u_post_dither", enabled ? (float)post!.Dither : 0f);
    }

    private (SceneEffectLayer Layer, SceneLayerMask Mask)? FindActiveMask()
    {
        if (_activeMaskId is not Guid id || _scene is null) return null;
        foreach (var layer in _scene.Layers)
        {
            var mask = layer.Masks.FirstOrDefault(candidate => candidate.Id == id);
            if (mask is not null) return (layer, mask);
        }
        return null;
    }

    private void BindEditorMaskUniforms(uint program, bool enabled = true)
    {
        var active = enabled ? FindActiveMask() : null;
        Ui(program, "u_preview_background_mode", enabled ? (int)PreviewBackgroundMode : 0);
        Ui(program, "u_editor_mask_enabled", active is null ? 0 : 1);
        U1(program, "u_editor_snap_x", enabled ? _maskSnapGuideX : -1f);
        U1(program, "u_editor_snap_y", enabled ? _maskSnapGuideY : -1f);
        Ui(program, "u_editor_rotation_snap_visual", enabled && active is not null && _maskDragMode == MaskDragMode.Rotate && MaskRotationSnapping && (ModifierKeys & (Keys.Control | Keys.Alt)) == Keys.None ? 1 : 0);
        if (active is not { } pair)
        {
            Ui(program, "u_editor_mask_type", 0);
            U3(program, "u_editor_mask_transform", .5f, .5f, 0f);
            U3(program, "u_editor_mask_shape", 1f, 1f, 0f);
            U3(program, "u_editor_mask_extra", 0f, 1f, 0f);
            U1(program, "u_editor_mask_gradient_softness", .5f);
            U1(program, "u_editor_mask_shape_amount", .35f);
            U3(program, "u_editor_mask_shape_detail", 0f, 6f, 5f);
            return;
        }

        var mask = pair.Mask;
        Ui(program, "u_editor_mask_type", (int)mask.Type);
        U3(program, "u_editor_mask_transform", (float)mask.X, (float)mask.Y, (float)mask.Rotation);
        U3(program, "u_editor_mask_shape", (float)mask.Width, (float)mask.Height, (float)mask.Feather);
        U3(program, "u_editor_mask_extra", (float)mask.GradientAngle, (float)mask.NoiseScale, mask.Invert ? 1f : 0f);
        U1(program, "u_editor_mask_gradient_softness", (float)mask.GradientSoftness);
        U1(program, "u_editor_mask_shape_amount", (float)mask.ShapeAmount);
        U3(program, "u_editor_mask_shape_detail", (float)mask.TriangleType, mask.PolygonSides, mask.StarPoints);
    }

    private void BindLayerMasks(SceneEffectLayer? layer)
    {
        var masks = layer?.Masks?.Where(mask => mask.Enabled).Take(8).ToArray() ?? Array.Empty<SceneLayerMask>();
        Ui(_captureProgram, "u_mask_count", masks.Length);
        for (int i = 0; i < 8; i++)
        {
            SceneLayerMask? mask = i < masks.Length ? masks[i] : null;
            U3(_captureProgram, $"u_mask_transform[{i}]",
                mask is null ? 0.5f : (float)mask.X,
                mask is null ? 0.5f : (float)mask.Y,
                mask is null ? 0f : (float)mask.Rotation);
            U3(_captureProgram, $"u_mask_shape[{i}]",
                mask is null ? 1f : (float)mask.Width,
                mask is null ? 1f : (float)mask.Height,
                mask is null ? 0f : (float)mask.Feather);
            U3(_captureProgram, $"u_mask_meta[{i}]",
                mask is null ? 0f : (float)mask.Type,
                mask is null ? 0f : (float)mask.Strength,
                mask?.Invert == true ? 1f : 0f);
            U3(_captureProgram, $"u_mask_extra[{i}]",
                mask is null ? 0f : (float)mask.GradientAngle,
                mask is null ? 1f : (float)mask.NoiseScale,
                mask is null ? 0f : mask.NoiseSeed);
            U3(_captureProgram, $"u_mask_gradient[{i}]",
                mask is null ? .5f : (float)mask.GradientSoftness,
                mask is null ? .35f : (float)mask.ShapeAmount,
                0f);
            U3(_captureProgram, $"u_mask_shape_detail[{i}]",
                mask is null ? 0f : (float)mask.TriangleType,
                mask is null ? 6f : mask.PolygonSides,
                mask is null ? 5f : mask.StarPoints);
        }
    }

    private static void SetPaletteUniforms(uint p, List<string> stops)
    {
        var list = stops.Count >= 2 ? stops.Take(8).ToList() : new List<string> { "#cccccc", "#ffffff" };
        Ui(p, "u_palette_count", list.Count);
        for (int i = 0; i < 8; i++)
        {
            var c = ColorUtil.ParseHtmlOrWhite(list[Math.Min(i, list.Count - 1)]);
            int loc = UniformLocation(p, $"u_palette{i}");
            if (loc >= 0) NativeGl.Uniform3f(loc, c.R / 255f, c.G / 255f, c.B / 255f);
        }
    }

    private static int UniformLocation(uint program, string name)
    {
        var key = (program, name);
        if (UniformLocationCache.TryGetValue(key, out int location)) return location;
        location = NativeGl.GetUniformLocation(program, name);
        UniformLocationCache[key] = location;
        return location;
    }

    private static void U1(uint program, string name, float value)
    {
        int location = UniformLocation(program, name);
        if (location >= 0) NativeGl.Uniform1f(location, value);
    }

    private static void Ui(uint program, string name, int value)
    {
        int location = UniformLocation(program, name);
        if (location >= 0) NativeGl.Uniform1i(location, value);
    }

    private static void U3(uint program, string name, float a, float b, float c)
    {
        int location = UniformLocation(program, name);
        if (location >= 0) NativeGl.Uniform3f(location, a, b, c);
    }

    private static void U2(uint program, string name, float a, float b)
    {
        int location = UniformLocation(program, name);
        if (location >= 0) NativeGl.Uniform2f(location, a, b);
    }
}

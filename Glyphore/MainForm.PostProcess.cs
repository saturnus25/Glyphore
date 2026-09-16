namespace Glyphore;

internal sealed partial class MainForm
{
    private readonly GlyphCheckBox _postEnabled = new();
    private readonly Dictionary<string, ParameterRow> _postRows = new(StringComparer.OrdinalIgnoreCase);

    private static readonly ParamDesc[] PostProcessParameters =
    [
        new("Exposición", "post_exposure", -3, 3, 0, 2, "Aclara u oscurece la imagen final en pasos de exposición."),
        new("Contraste", "post_contrast", 0, 3, 1, 2, "Aumenta o reduce la diferencia entre zonas claras y oscuras."),
        new("Saturación", "post_saturation", 0, 3, 1, 2, "Controla la intensidad de los colores del resultado final."),
        new("Bloom / resplandor", "post_bloom", 0, 3, 0, 2, "Extiende luz y color desde las celdas brillantes."),
        new("Radio bloom", "post_bloom_radius", .25, 4, 1, 2, "Cambia cuánto se extiende el resplandor entre celdas."),
        new("Viñeta", "post_vignette", 0, 2, 0, 2, "Oscurece progresivamente los bordes de la preview."),
        new("Scanlines", "post_scanlines", 0, 1, 0, 2, "Añade líneas horizontales tipo CRT."),
        new("Grano", "post_grain", 0, 1, 0, 2, "Añade ruido fino animado al resultado."),
        new("Aberración RGB", "post_chromatic", 0, 4, 0, 2, "Separa ligeramente los canales de color."),
        new("Posterizar", "post_posterize", 0, 24, 0, 0, "Reduce el número de niveles de color. Cero lo desactiva."),
        new("Threshold", "post_threshold", 0, 1, 0, 2, "Recorta las zonas por debajo de un nivel de brillo. Cero lo desactiva."),
        new("Desenfoque", "post_blur", 0, 1, 0, 2, "Mezcla la celda con sus vecinas para suavizar el resultado."),
        new("Nitidez", "post_sharpen", 0, 2, 0, 2, "Refuerza el contraste local entre celdas."),
        new("Pixelado", "post_pixelate", 1, 8, 1, 0, "Agrupa varias celdas visuales en bloques. Uno lo desactiva."),
        new("Dithering", "post_dither", 0, 1, 0, 2, "Añade un patrón fino para romper bandas de color y dar textura.")
    ];

    private ThemedGroupBox BuildPostProcessGroup()
    {
        var group = Group(Localization.Text("group.postprocess"), PostProcessParameters.Length * 34 + 92);
        group.Tag = "group.postprocess";

        _postEnabled.Text = Localization.Text("check.postprocess");
        _postEnabled.Tag = "check.postprocess";
        Theme.CheckBox(_postEnabled);
        _postEnabled.SetBounds(10, 25, 245, 25);
        _postEnabled.CheckedChanged += (_, _) =>
        {
            if (_applying || !_sceneReady) return;
            _scene.PostProcess.Enabled = _postEnabled.Checked;
            PushSceneOnly();
        };
        group.Controls.Add(_postEnabled);

        var reset = Btn(Localization.Text("button.postreset"), (_, _) => ResetPostProcess());
        reset.Tag = "button.postreset";
        reset.SetBounds(270, 23, 135, 28);
        group.Controls.Add(reset);

        int y = 57;
        foreach (var desc in PostProcessParameters)
        {
            var row = new ParameterRow(desc, PostValue(desc.Key))
            {
                Left = 10,
                Top = y,
                Width = 395
            };
            string key = desc.Key;
            row.ValueChanged += value =>
            {
                if (_applying || !_sceneReady) return;
                SetPostValue(key, value);
                PushSceneOnly();
            };
            _postRows[key] = row;
            group.Controls.Add(row);
            y += 34;
        }

        return group;
    }

    private double PostValue(string key)
    {
        var p = _scene.PostProcess;
        return key switch
        {
            "post_exposure" => p.Exposure,
            "post_contrast" => p.Contrast,
            "post_saturation" => p.Saturation,
            "post_bloom" => p.Bloom,
            "post_bloom_radius" => p.BloomRadius,
            "post_vignette" => p.Vignette,
            "post_scanlines" => p.Scanlines,
            "post_grain" => p.Grain,
            "post_chromatic" => p.ChromaticAberration,
            "post_posterize" => p.Posterize,
            "post_threshold" => p.Threshold,
            "post_blur" => p.Blur,
            "post_sharpen" => p.Sharpen,
            "post_pixelate" => p.Pixelate,
            "post_dither" => p.Dither,
            _ => 0
        };
    }

    private void SetPostValue(string key, double value)
    {
        var p = _scene.PostProcess;
        switch (key)
        {
            case "post_exposure": p.Exposure = value; break;
            case "post_contrast": p.Contrast = value; break;
            case "post_saturation": p.Saturation = value; break;
            case "post_bloom": p.Bloom = value; break;
            case "post_bloom_radius": p.BloomRadius = value; break;
            case "post_vignette": p.Vignette = value; break;
            case "post_scanlines": p.Scanlines = value; break;
            case "post_grain": p.Grain = value; break;
            case "post_chromatic": p.ChromaticAberration = value; break;
            case "post_posterize": p.Posterize = value; break;
            case "post_threshold": p.Threshold = value; break;
            case "post_blur": p.Blur = value; break;
            case "post_sharpen": p.Sharpen = value; break;
            case "post_pixelate": p.Pixelate = value; break;
            case "post_dither": p.Dither = value; break;
        }
        p.Clamp();
    }

    private void SyncPostProcessControls()
    {
        if (!_sceneReady) return;
        _applying = true;
        try
        {
            _postEnabled.Checked = _scene.PostProcess.Enabled;
            foreach (var desc in PostProcessParameters)
                if (_postRows.TryGetValue(desc.Key, out var row)) row.SetValue(PostValue(desc.Key));
        }
        finally { _applying = false; }
    }

    private void ResetPostProcess()
    {
        if (!_sceneReady) return;
        _scene.PostProcess = new ScenePostProcess();
        SyncPostProcessControls();
        PushSceneOnly();
    }
}

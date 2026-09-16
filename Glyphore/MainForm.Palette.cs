namespace Glyphore;

internal sealed partial class MainForm
{
    private void ApplyPalette(string name, bool push = true, bool applyToSelection = true)
    {
        if (!_data.Palettes.TryGetValue(name, out var stops)) return;
        _settings.PaletteName = name;
        _settings.PaletteStops = new List<string>(stops);

        if (_sceneReady)
        {
            var targets = applyToSelection
                ? SelectedLayers()
                : (_scene.ActiveLayer is { } active ? new List<SceneEffectLayer> { active } : []);
            foreach (var layer in targets) layer.SetPalette(name, stops);
        }

        _applying = true;
        _palette.SelectedItem = name;
        _applying = false;
        RebuildPaletteStops();
        if (push) Push();
    }

    private void ApplyCurrentPaletteToSelectedLayers()
    {
        if (!_sceneReady) return;
        foreach (var layer in SelectedLayers())
            layer.SetPalette(_settings.PaletteName, _settings.PaletteStops);
        PushSceneOnly();
    }

    private void ApplyCurrentPaletteToAllLayers()
    {
        if (!_sceneReady) return;
        foreach (var layer in _scene.Layers)
            layer.SetPalette(_settings.PaletteName, _settings.PaletteStops);
        PushSceneOnly();
    }

    private void RebuildPaletteStops()
    {
        _paletteStops.Controls.Clear();
        int x = 2;
        for (int i = 0; i < _settings.PaletteStops.Count; i++)
        {
            int index = i;
            var button = new Button
            {
                Left = x,
                Top = 6,
                Width = 42,
                Height = 42,
                BackColor = ColorUtil.ParseHtmlOrWhite(_settings.PaletteStops[i]),
                FlatStyle = FlatStyle.Flat,
                Tag = index
            };
            button.FlatAppearance.BorderColor = Color.Gray;
            button.Click += (_, _) => EditPaletteStop(index);
            _tips.SetToolTip(
                button,
                Localization.English
                    ? "Click to change this gradient color. The change applies to the selected layer(s)."
                    : "Haz clic para cambiar este color del gradiente. El cambio se aplica a las capas seleccionadas.");
            _paletteStops.Controls.Add(button);
            x += 46;
        }
    }

    private void EditPaletteStop(int index)
    {
        using var dialog = new ColorDialog
        {
            Color = ColorUtil.ParseHtmlOrWhite(_settings.PaletteStops[index]),
            FullOpen = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        EnsureCustomPalette();
        _settings.PaletteStops[index] = ColorTranslator.ToHtml(dialog.Color);
        ApplyCurrentPaletteToSelectedLayers();
        RebuildPaletteStops();
        Push();
    }

    private void AddPaletteStop()
    {
        using var dialog = new ColorDialog { Color = Color.White, FullOpen = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        EnsureCustomPalette();
        _settings.PaletteStops.Add(ColorTranslator.ToHtml(dialog.Color));
        if (_settings.PaletteStops.Count > 8) _settings.PaletteStops = _settings.PaletteStops.Take(8).ToList();
        ApplyCurrentPaletteToSelectedLayers();
        RebuildPaletteStops();
        Push();
    }

    private void EnsureCustomPalette()
    {
        if (_settings.PaletteName.Equals("Custom", StringComparison.OrdinalIgnoreCase)) return;
        _settings.PaletteName = "Custom";
        if (!_palette.Items.Contains("Custom")) _palette.Items.Add("Custom");
        _applying = true;
        _palette.SelectedItem = "Custom";
        _applying = false;
    }

    private void MarkCustom()
    {
        if (_applying) return;
        _settings.Preset = "Custom";
        if (!_preset.Items.Contains("Custom")) _preset.Items.Add("Custom");
        _applying = true;
        _preset.SelectedItem = "Custom";
        _applying = false;
    }

    private void Push()
    {
        SyncSceneFromSettings();
        _preview.Settings = _settings;
        if (_sceneReady) _preview.Scene = _scene;
        _preview.TargetFps = _settings.Fps;
        RecordHistory();
    }
}

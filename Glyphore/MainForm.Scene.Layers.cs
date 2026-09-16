namespace Glyphore;

internal sealed partial class MainForm
{
    private void ApplySceneLanguage()
    {
        int blend = _layerBlend.SelectedIndex;
        _applying = true;
        _layerBlend.Items.Clear();
        _layerBlend.Items.AddRange(Localization.English ? new object[] { "Normal", "Additive" } : new object[] { "Normal", "Aditivo" });
        _layerBlend.SelectedIndex = blend >= 0 ? Math.Min(blend, _layerBlend.Items.Count - 1) : 0;
        _undoButton.Text = Localization.Text("button.undo") + "  Ctrl+Z";
        _redoButton.Text = Localization.Text("button.redo") + "  Ctrl+Y";
        _layerDetachButton.Text = _detachedLayersWindow is { IsDisposed: false }
            ? (Localization.English ? "↙ Dock" : "↙ Acoplar")
            : (Localization.English ? "↗ Detach" : "↗ Desacoplar");
        _previewFollowsSceneBackground.Text = Localization.Text("check.previewscenebackground");
        ApplyMaskLanguage();
        SyncSceneBackgroundControl();
        _applying = false;
    }

    private List<SceneEffectLayer> SelectedLayers(bool fallbackActive = true)
    {
        var result = new List<SceneEffectLayer>();
        var seen = new HashSet<Guid>();
        foreach (var layer in _layers.SelectedItems.OfType<SceneEffectLayer>())
        {
            if (seen.Add(layer.Id)) result.Add(layer);
        }

        if (result.Count == 0 && fallbackActive && _scene.ActiveLayer is { } active)
            result.Add(active);
        return result;
    }

    private void SelectAllLayers()
    {
        if (!_sceneReady) return;
        _applying = true;
        try
        {
            for (int i = 0; i < _layers.Items.Count; i++) _layers.SetSelected(i, true);
        }
        finally { _applying = false; }
        SyncLayerControls();
    }

    private void ClearLayerSelection()
    {
        _applying = true;
        try { _layers.ClearSelected(); }
        finally { _applying = false; }
        SyncLayerControls();
    }

    private void RenameActiveLayer()
    {
        if (!_sceneReady) return;
        var activeMask = ActiveMask();
        SceneEffectLayer? layer = activeMask?.Layer ?? _scene.ActiveLayer;
        if (layer is null) return;
        string currentName = activeMask?.Mask.Name ?? layer.Name;
        bool renamingMask = activeMask is not null;

        using var dialog = new Form
        {
            Text = Localization.English
                ? (renamingMask ? "Rename mask" : "Rename layer")
                : (renamingMask ? "Renombrar máscara" : "Renombrar capa"),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(360, 112),
            BackColor = Theme.Panel,
            ForeColor = Theme.Text
        };
        var input = new TextBox { Left = 12, Top = 15, Width = 336, Text = currentName };
        Theme.TextBox(input);
        var ok = Btn(Localization.English ? "Rename" : "Renombrar", (_, _) => dialog.DialogResult = DialogResult.OK);
        var cancel = Btn(Localization.English ? "Cancel" : "Cancelar", (_, _) => dialog.DialogResult = DialogResult.Cancel);
        ok.SetBounds(174, 62, 84, 30);
        cancel.SetBounds(264, 62, 84, 30);
        dialog.Controls.AddRange([input, ok, cancel]);
        dialog.AcceptButton = ok;
        dialog.CancelButton = cancel;
        dialog.Shown += (_, _) => { input.Focus(); input.SelectAll(); };

        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        string name = input.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;
        if (activeMask is { } pair)
        {
            pair.Mask.Name = name;
            pair.Layer.Touch();
        }
        else
        {
            layer.Name = name;
        }
        RefreshLayerList();
        PushSceneOnly();
    }

    private void InitializeSceneFromCurrentSettings()
    {
        _scene = GlyphoreScene.FromSettings(_settings);
        _activeMaskId = null;
        _preview.ActiveMaskId = null;
        _sceneReady = true;
        _scenePath = null;
        RefreshLayerList();
        SyncLayerControls();
        Push();
    }

    private void SyncSceneFromSettings()
    {
        if (!_sceneReady) return;
        _scene.CaptureCommonFrom(_settings);
        var active = _scene.EnsureActiveLayer(_settings);
        string previousEffect = active.Effect;
        bool autoNamed = active.Name.Equals(previousEffect, StringComparison.OrdinalIgnoreCase);
        active.CaptureFrom(_settings);
        if (autoNamed) active.Name = _settings.Effect;
    }

    private void PushSceneOnly()
    {
        if (!_sceneReady) return;
        _preview.Scene = _scene;
        _preview.Settings = _settings;
        _preview.ActiveMaskId = _activeMaskId;
        _preview.MaskSnapping = _maskSnapping.Checked;
        _preview.MaskRotationSnapping = _maskRotationSnapping.Checked;
        _preview.TargetFps = _scene.Fps;
        if (_preview.Paused) _preview.Invalidate();
        RecordHistory();
    }

    private void RefreshLayerList()
    {
        if (!_sceneReady) return;
        var selectedIds = _layers.SelectedItems.OfType<SceneEffectLayer>().Select(layer => layer.Id).ToHashSet();
        if (_activeMaskId is null && _scene.ActiveLayerId is Guid activeId && !selectedIds.Contains(activeId))
        {
            selectedIds.Clear();
            selectedIds.Add(activeId);
        }

        _applying = true;
        _layers.BeginUpdate();
        try
        {
            _layers.Items.Clear();
            foreach (var layer in _scene.Layers)
            {
                _layers.Items.Add(layer);
                foreach (var mask in layer.Masks) _layers.Items.Add(new MaskListEntry(layer, mask));
            }
            for (int i = 0; i < _layers.Items.Count; i++)
            {
                if (_activeMaskId is Guid maskId && _layers.Items[i] is MaskListEntry entry && entry.Mask.Id == maskId)
                    _layers.SetSelected(i, true);
                else if (_activeMaskId is null && _layers.Items[i] is SceneEffectLayer layer && selectedIds.Contains(layer.Id))
                    _layers.SetSelected(i, true);
            }
        }
        finally
        {
            _layers.EndUpdate();
            _applying = false;
        }
        SyncLayerControls();
    }

    private void SyncLayerControls()
    {
        SyncSceneBackgroundControl();
        if (!_sceneReady || _scene.ActiveLayer is not { } layer) return;
        _applying = true;
        _layerVisible.Checked = layer.Visible;
        _layerOpacity.Value = Math.Clamp((decimal)(layer.Opacity * 100.0), _layerOpacity.Minimum, _layerOpacity.Maximum);
        _layerBlend.SelectedIndex = layer.BlendMode == LayerBlendMode.Additive ? 1 : 0;
        _respectLayerOrder.Checked = _scene.RespectLayerOrder;
        SyncMaskControls();
        _preview.ActiveMaskId = _activeMaskId;
        _preview.MaskSnapping = _maskSnapping.Checked;
        _preview.MaskRotationSnapping = _maskRotationSnapping.Checked;
        _applying = false;
    }

    private void ActivateLayer(Guid id)
    {
        if (!_sceneReady || _scene.ActiveLayerId == id) return;
        ExitImportedMode();
        SyncSceneFromSettings();
        var layer = _scene.Layers.FirstOrDefault(candidate => candidate.Id == id);
        if (layer is null) return;
        _activeMaskId = null;
        _preview.ActiveMaskId = null;
        UpdateDiscordPresenceContext();
        _scene.ActiveLayerId = layer.Id;
        LoadActiveLayerIntoEditor();
    }

    private void LoadActiveLayerIntoEditor(bool refreshCommonControls = false)
    {
        var layer = _scene.EnsureActiveLayer(_settings);
        _settings.CopyFrom(_scene.CreateSettings(layer));

        _applying = true;
        try
        {
            _effect.SelectedItem = _settings.Effect;
            RebuildPresetChoicesForActiveLayer();
            _shape.SelectedItem = _settings.ShapeMode;
            SetNumericValue(_seed, _settings.Seed);
            if (!_charsetPreset.Items.Contains(_settings.CharsetName))
                _charsetPreset.Items.Add(_settings.CharsetName);
            _charsetPreset.SelectedItem = _settings.CharsetName;
            _charset.Text = string.IsNullOrEmpty(_settings.Charset) ? " " : _settings.Charset;
            if (!_palette.Items.Contains(_settings.PaletteName))
                _palette.Items.Add(_settings.PaletteName);
            _palette.SelectedItem = _settings.PaletteName;

            if (refreshCommonControls)
            {
                SetNumericValue(_width, _scene.Width);
                SetNumericValue(_height, _scene.Height);
                SetNumericValue(_fps, _scene.Fps);
                SetNumericValue(_duration, _scene.Duration);
                SetNumericValue(_glyphSize, _scene.GlyphDisplayScale * 100.0);
                _invert.Checked = _scene.Invert;
                _color.Checked = _scene.ColorEnabled;
                _exportCredit.Checked = _scene.IncludeExportCredit;

            }

            foreach (var description in ParameterCatalog.General)
            {
                if (_paramRows.TryGetValue(description.Key, out var row))
                    row.SetValue(_settings.Get(description.Key));
            }
        }
        finally
        {
            _applying = false;
        }

        RebuildSpecific();
        RebuildPaletteStops();
        SyncGlobalTransformControls();
        SyncPostProcessControls();
        UpdateEffectTip();
        RefreshLayerList();
        PushSceneOnly();
    }

    private void RebuildPresetChoicesForActiveLayer()
    {
        _preset.Items.Clear();
        if (_data.Presets.TryGetValue(_settings.Effect, out var group))
            _preset.Items.AddRange(group.Keys.Cast<object>().ToArray());
        if (!_preset.Items.Contains(_settings.Preset))
            _preset.Items.Add(_settings.Preset);
        _preset.SelectedItem = _settings.Preset;
        SceneEffectLayer? active = _scene.ActiveLayer;
        string source = active?.SourcePreset ?? string.Empty;
        _tips.SetToolTip(_preset, _settings.Preset.Equals("Custom", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(source)
            ? (Localization.English ? $"Custom settings · based on preset: {source}" : $"Ajustes personalizados · basado en preset: {source}")
            : (Localization.English ? "Visual/effect preset for the active layer." : "Preset visual/del efecto de la capa activa."));
    }

    private static void SetNumericValue(NumericUpDown numeric, double value)
    {
        decimal converted;
        try { converted = (decimal)value; }
        catch { converted = numeric.Minimum; }
        numeric.Value = Math.Clamp(converted, numeric.Minimum, numeric.Maximum);
    }

    private string UniqueLayerName(string baseName)
    {
        string clean = string.IsNullOrWhiteSpace(baseName) ? "Effect" : baseName.Trim();
        if (_scene.Layers.All(layer => !layer.Name.Equals(clean, StringComparison.OrdinalIgnoreCase))) return clean;
        for (int i = 2; i < 10000; i++)
        {
            string candidate = $"{clean} {i}";
            if (_scene.Layers.All(layer => !layer.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase))) return candidate;
        }
        return $"{clean} {Guid.NewGuid().ToString("N")[..6]}";
    }

    private void AddLayer()
    {
        ExitImportedMode();
        if (!_sceneReady) return;
        if (_scene.Layers.Count >= GlyphoreScene.MaxLayers)
        {
            _status.Text = Localization.English
                ? $"A scene can contain at most {GlyphoreScene.MaxLayers} layers."
                : $"Una escena puede contener como máximo {GlyphoreScene.MaxLayers} capas.";
            return;
        }
        SyncSceneFromSettings();
        var layer = SceneEffectLayer.FromSettings(_settings, UniqueLayerName(_settings.Effect));
        layer.Visible = true;
        layer.Opacity = 1.0;
        layer.BlendMode = LayerBlendMode.Normal;
        _scene.Layers.Insert(0, layer);
        _scene.ActiveLayerId = layer.Id;
        LoadActiveLayerIntoEditor();
    }

    private void DuplicateLayer()
    {
        ExitImportedMode();
        if (!_sceneReady) return;
        var targets = SelectedLayers();
        if (targets.Count == 0) return;
        if (_scene.Layers.Count + targets.Count > GlyphoreScene.MaxLayers)
        {
            _status.Text = Localization.English
                ? $"A scene can contain at most {GlyphoreScene.MaxLayers} layers."
                : $"Una escena puede contener como máximo {GlyphoreScene.MaxLayers} capas.";
            return;
        }

        SyncSceneFromSettings();
        var selectedIds = targets.Select(layer => layer.Id).ToHashSet();
        var clones = new List<SceneEffectLayer>();
        for (int i = _scene.Layers.Count - 1; i >= 0; i--)
        {
            var source = _scene.Layers[i];
            if (!selectedIds.Contains(source.Id)) continue;
            var clone = source.Duplicate(UniqueLayerName(source.Name + " Copy"));
            _scene.Layers.Insert(i, clone);
            _preview.DuplicateLayerClock(source.Id, clone.Id);
            clones.Add(clone);
        }

        clones.Reverse();
        if (clones.Count == 0) return;
        _scene.ActiveLayerId = clones[0].Id;
        LoadActiveLayerIntoEditor();

        var cloneIds = clones.Select(layer => layer.Id).ToHashSet();
        _applying = true;
        try
        {
            _layers.ClearSelected();
            for (int i = 0; i < _layers.Items.Count; i++)
            {
                if (_layers.Items[i] is SceneEffectLayer layer && cloneIds.Contains(layer.Id))
                    _layers.SetSelected(i, true);
            }
        }
        finally { _applying = false; }
        SyncLayerControls();
    }

    private void RemoveLayer()
    {
        ExitImportedMode();
        if (!_sceneReady) return;
        var targets = SelectedLayers();
        if (targets.Count == 0) return;
        if (_scene.Layers.Count - targets.Count < 1)
        {
            _status.Text = Localization.English ? "A scene must keep at least one layer." : "Una escena debe conservar al menos una capa.";
            return;
        }

        int firstIndex = targets.Select(target => _scene.Layers.FindIndex(layer => layer.Id == target.Id)).Where(index => index >= 0).DefaultIfEmpty(0).Min();
        var ids = targets.Select(layer => layer.Id).ToHashSet();
        foreach (var id in ids) _preview.ForgetLayerRuntime(id);
        _scene.Layers.RemoveAll(layer => ids.Contains(layer.Id));
        _activeMaskId = null;
        _preview.ActiveMaskId = null;
        UpdateDiscordPresenceContext();

        int next = Math.Clamp(firstIndex, 0, _scene.Layers.Count - 1);
        _scene.ActiveLayerId = _scene.Layers[next].Id;
        LoadActiveLayerIntoEditor();
    }

    private void MoveLayer(int delta)
    {
        ExitImportedMode();
        if (!_sceneReady || delta == 0) return;
        SyncSceneFromSettings();
        var ids = SelectedLayers().Select(layer => layer.Id).ToHashSet();
        if (ids.Count == 0) return;

        if (delta < 0)
        {
            for (int i = 1; i < _scene.Layers.Count; i++)
            {
                if (ids.Contains(_scene.Layers[i].Id) && !ids.Contains(_scene.Layers[i - 1].Id))
                    (_scene.Layers[i - 1], _scene.Layers[i]) = (_scene.Layers[i], _scene.Layers[i - 1]);
            }
        }
        else
        {
            for (int i = _scene.Layers.Count - 2; i >= 0; i--)
            {
                if (ids.Contains(_scene.Layers[i].Id) && !ids.Contains(_scene.Layers[i + 1].Id))
                    (_scene.Layers[i + 1], _scene.Layers[i]) = (_scene.Layers[i], _scene.Layers[i + 1]);
            }
        }

        RefreshLayerList();
        PushSceneOnly();
    }
}

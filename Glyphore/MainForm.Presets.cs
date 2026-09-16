using System.Text.Json;

namespace Glyphore;

internal sealed partial class MainForm
{
    private void RebuildPresets()
    {
        string? firstPreset = null;
        bool wasApplying = _applying;
        _applying = true;
        try
        {
            _preset.Items.Clear();
            if (_data.Presets.TryGetValue(_settings.Effect, out var group))
                _preset.Items.AddRange(group.Keys.Cast<object>().ToArray());

            if (_preset.Items.Count > 0)
            {
                _preset.SelectedIndex = 0;
                firstPreset = _preset.Text;
            }
        }
        finally
        {
            _applying = wasApplying;
        }

        // Apply exactly once after the combo has been rebuilt. Previously SelectedIndex
        // could apply a preset during the rebuild and the method applied it a second time.
        if (!string.IsNullOrEmpty(firstPreset))
            ApplyPreset(firstPreset);
        else
            RebuildSpecific();
    }

    private void ApplyPreset(string name)
    {
        string effect = _settings.Effect;
        if (string.IsNullOrEmpty(name) ||
            !_data.Presets.TryGetValue(effect, out var group) ||
            !group.TryGetValue(name, out var preset))
            return;

        bool wasApplying = _applying;
        _applying = true;
        try
        {
            _settings.Effect = effect;
            _settings.Preset = name;
            _settings.ResetValues();

            foreach (var item in preset)
            {
                if (item.Value.ValueKind == JsonValueKind.Number && item.Value.TryGetDouble(out double value))
                {
                    _settings.Set(item.Key, value);
                }
                else if (item.Key.Equals("palette", StringComparison.OrdinalIgnoreCase) && item.Value.ValueKind == JsonValueKind.String)
                {
                    _settings.PaletteName = item.Value.GetString() ?? "Monochrome";
                }
                else if (item.Key.Equals("shape_mode", StringComparison.OrdinalIgnoreCase) && item.Value.ValueKind == JsonValueKind.String)
                {
                    _settings.ShapeMode = item.Value.GetString() ?? "Square";
                }
            }

            // Resolve palette data in the editor state only. The active layer is updated once,
            // atomically, by Push() below instead of receiving a palette-only intermediate state.
            if (_data.Palettes.TryGetValue(_settings.PaletteName, out var stops))
                _settings.PaletteStops = new List<string>(stops);

            foreach (var row in _paramRows)
                row.Value.SetValue(_settings.Get(row.Key));

            if (!_palette.Items.Contains(_settings.PaletteName))
                _palette.Items.Add(_settings.PaletteName);
            _palette.SelectedItem = _settings.PaletteName;
            _shape.SelectedItem = _settings.ShapeMode;
        }
        finally
        {
            _applying = wasApplying;
        }

        RebuildSpecific();
        RebuildPaletteStops();
        Push();
    }

    private void RebuildSpecific()
    {
        _specificParams.Controls.Clear();
        var specificKeys = ParameterCatalog.Specific.Values
            .SelectMany(group => group)
            .Select(parameter => parameter.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (string key in _paramRows.Keys.Where(specificKeys.Contains).ToList())
            _paramRows.Remove(key);

        if (!ParameterCatalog.Specific.TryGetValue(_settings.Effect, out var descriptions))
        {
            var label = new Label
            {
                Text = Localization.Text("specific.none"),
                ForeColor = Theme.Muted,
                AutoSize = true,
                Top = 8,
                Left = 2
            };
            _specificParams.Controls.Add(label);
            ResizeSpecificGroup(75);
            return;
        }

        int y = 0;
        foreach (var description in descriptions)
        {
            var row = CreateParamRow(description);
            row.Top = y;
            _specificParams.Controls.Add(row);
            y += 34;
        }

        if (Camera3D.TryGetSpec(_settings.Effect, out _))
        {
            var resetCamera = Btn(Localization.Text("button.resetcamera"), (_, _) => ResetCurrentCamera());
            resetCamera.Tag = "button.resetcamera";
            resetCamera.Left = 0;
            resetCamera.Top = y + 2;
            resetCamera.Width = Math.Max(120, _specificParams.ClientSize.Width);
            resetCamera.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            resetCamera.Height = 28;
            _tips.SetToolTip(resetCamera, Localization.Tip("button.resetcamera"));
            _specificParams.Controls.Add(resetCamera);
            y += 34;
        }

        ResizeSpecificGroup(y + 30);
    }

    private void HandleCameraChanged(string effect, double yaw, double pitch, double distance)
    {
        if (!_settings.Effect.Equals(effect, StringComparison.OrdinalIgnoreCase) ||
            !Camera3D.TryGetSpec(effect, out var spec))
            return;

        _settings.Set(spec.YawKey, yaw);
        _settings.Set(spec.PitchKey, pitch);
        _settings.Set(spec.DistanceKey, distance);

        if (_paramRows.TryGetValue(spec.YawKey, out var yawRow)) yawRow.SetValue(yaw);
        if (_paramRows.TryGetValue(spec.PitchKey, out var pitchRow)) pitchRow.SetValue(pitch);
        if (_paramRows.TryGetValue(spec.DistanceKey, out var distanceRow)) distanceRow.SetValue(distance);
        MarkCustom();
        Push();
    }

    private void ResetCurrentCamera()
    {
        if (!Camera3D.TryGetSpec(_settings.Effect, out var spec)) return;

        Camera3D.Reset(_settings, spec);
        if (_paramRows.TryGetValue(spec.YawKey, out var yawRow)) yawRow.SetValue(_settings.Get(spec.YawKey));
        if (_paramRows.TryGetValue(spec.PitchKey, out var pitchRow)) pitchRow.SetValue(_settings.Get(spec.PitchKey));
        if (_paramRows.TryGetValue(spec.DistanceKey, out var distanceRow)) distanceRow.SetValue(_settings.Get(spec.DistanceKey));
        if (spec.PanXKey is not null && _paramRows.TryGetValue(spec.PanXKey, out var panXRow)) panXRow.SetValue(_settings.Get(spec.PanXKey));
        if (spec.PanYKey is not null && _paramRows.TryGetValue(spec.PanYKey, out var panYRow)) panYRow.SetValue(_settings.Get(spec.PanYKey));
        MarkCustom();
        Push();
    }

    private void ResizeSpecificGroup(int height)
    {
        var group = _left.Controls.Cast<Control>().FirstOrDefault(control => control.Name == "specificGroup");
        if (group is null) return;
        group.Height = Math.Max(65, height);
        _specificParams.Height = group.Height - 28;
    }

    private void AddParamRow(Panel panel, ParamDesc description)
    {
        var row = CreateParamRow(description);
        row.Top = panel.Controls.Count * 34;
        panel.Controls.Add(row);
    }

    private ParameterRow CreateParamRow(ParamDesc description)
    {
        var row = new ParameterRow(description, _settings.Get(description.Key))
        {
            Left = 0,
            Width = 390,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };
        row.ValueChanged += value =>
        {
            _settings.Set(description.Key, value);
            MarkCustom();
            Push();
        };
        _paramRows[description.Key] = row;
        return row;
    }
}

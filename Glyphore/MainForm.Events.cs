namespace Glyphore;

internal sealed partial class MainForm
{
    private void HandleShortcutKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && !e.Shift && e.KeyCode == Keys.Z)
        {
            UndoScene();
            e.SuppressKeyPress = true;
        }
        else if (e.Control && (e.KeyCode == Keys.Y || (e.Shift && e.KeyCode == Keys.Z)))
        {
            RedoScene();
            e.SuppressKeyPress = true;
        }
        else if (_layers.Focused && e.Control && e.KeyCode == Keys.A)
        {
            SelectAllLayers();
            e.SuppressKeyPress = true;
        }
        else if (_layers.Focused && e.KeyCode == Keys.F2)
        {
            RenameActiveLayer();
            e.SuppressKeyPress = true;
        }
        else if (_layers.Focused && e.KeyCode == Keys.Delete)
        {
            RemoveLayer();
            e.SuppressKeyPress = true;
        }
        else if (e.KeyCode == Keys.Space)
        {
            TogglePause();
            e.SuppressKeyPress = true;
        }
        else if (e.KeyCode == Keys.F5)
        {
            NotifyDiscordPreviewing();
            _preview.RestartAnimation();
        }
        else if (e.Control && e.KeyCode == Keys.E)
        {
            _ = ExportDialogAsync();
            e.SuppressKeyPress = true;
        }
        else if (e.Control && e.KeyCode == Keys.S)
        {
            SaveScene();
            e.SuppressKeyPress = true;
        }
        else if (e.Control && e.KeyCode == Keys.O)
        {
            LoadSceneDialog();
            e.SuppressKeyPress = true;
        }
    }

    private void PopulateData()
    {
        _effect.Items.AddRange(_data.Presets.Keys.Cast<object>().ToArray());
        if (_effect.Items.Count > 0) _effect.SelectedIndex = 0;
        UpdateEffectTip();

        _charsetPreset.Items.AddRange(_data.Charsets.Keys.Cast<object>().ToArray());
        if (_charsetPreset.Items.Count > 0) _charsetPreset.SelectedIndex = 0;

        _palette.Items.AddRange(_data.Palettes.Keys.Cast<object>().ToArray());
        if (_palette.Items.Count > 0) _palette.SelectedItem = "Plasma";
    }

    private void WireEvents()
    {
        _language.SelectedIndexChanged += (_, _) =>
        {
            Localization.English = _language.SelectedIndex == 1;
            ApplyLanguage();
        };

        _effect.SelectedIndexChanged += (_, _) =>
        {
            if (_applying) return;
            ExitImportedMode();
            _settings.Effect = _effect.Text;
            UpdateEffectTip();
            RebuildPresets();
            if (_sceneReady) RefreshLayerList();
        };

        _preset.SelectedIndexChanged += (_, _) =>
        {
            if (!_applying && _preset.SelectedItem is not null) ApplyPreset(_preset.Text);
        };

        _charsetPreset.SelectedIndexChanged += (_, _) =>
        {
            if (_applying) return;
            if (!_data.Charsets.TryGetValue(_charsetPreset.Text, out string? ramp)) return;
            _applying = true;
            try { _charset.Text = ramp; }
            finally { _applying = false; }
            _settings.CharsetName = _charsetPreset.Text;
            _settings.Charset = string.IsNullOrEmpty(ramp) ? " " : ramp;
            ApplyCurrentCharsetToSelectedLayers();
            Push();
        };

        _charset.TextChanged += (_, _) =>
        {
            if (_applying) return;
            _settings.Charset = _charset.Text.Length == 0 ? " " : _charset.Text;
            MarkCurrentCharsetCustom();
            ApplyCurrentCharsetToSelectedLayers();
            Push();
        };

        _glyphSize.ValueChanged += (_, _) =>
        {
            _settings.GlyphDisplayScale = (double)_glyphSize.Value / 100.0;
            Push();
        };

        _invert.CheckedChanged += (_, _) =>
        {
            _settings.Invert = _invert.Checked;
            Push();
        };
        _color.CheckedChanged += (_, _) =>
        {
            _settings.ColorEnabled = _color.Checked;
            Push();
        };
        _exportCredit.CheckedChanged += (_, _) =>
        {
            if (_applying) return;
            _settings.IncludeExportCredit = _exportCredit.Checked;
            Push();
        };

        _palette.SelectedIndexChanged += (_, _) =>
        {
            if (_applying) return;
            ApplyPalette(_palette.Text);
        };
        _shape.SelectedIndexChanged += (_, _) =>
        {
            if (_applying) return;
            _settings.ShapeMode = _shape.Text;
            MarkCustom();
            Push();
        };

        _width.ValueChanged += (_, _) =>
        {
            _settings.Width = (int)_width.Value;
            Push();
        };
        _height.ValueChanged += (_, _) =>
        {
            _settings.Height = (int)_height.Value;
            Push();
        };
        _fps.ValueChanged += (_, _) =>
        {
            _settings.Fps = (int)_fps.Value;
            _preview.TargetFps = _settings.Fps;
            Push();
        };
        _duration.ValueChanged += (_, _) =>
        {
            if (_applying) return;
            _settings.Duration = (double)_duration.Value;
            Push();
        };
        _seed.ValueChanged += (_, _) =>
        {
            _settings.Seed = (int)_seed.Value;
            Push();
        };
    }

    private void TogglePause()
    {
        if (_importView.Visible)
        {
            if (_importClock.IsRunning)
            {
                _importClock.Stop();
                _pauseButton.Text = Localization.Text("button.resume");
            }
            else
            {
                _importClock.Start();
                _pauseButton.Text = Localization.Text("button.pause");
            }
            return;
        }

        _preview.TogglePause();
        if (!_preview.Paused) NotifyDiscordPreviewing();
        _pauseButton.Text = _preview.Paused
            ? Localization.Text("button.resume")
            : Localization.Text("button.pause");
    }
}

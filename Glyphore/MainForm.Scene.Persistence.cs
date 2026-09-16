namespace Glyphore;

internal sealed partial class MainForm
{
    private void SaveScene()
    {
        if (!_sceneReady) return;
        SyncSceneFromSettings();

        using var dialog = new SaveFileDialog
        {
            Filter = Localization.English ? "Glyphoré scene|*.glyphore|JSON|*.json" : "Escena Glyphoré|*.glyphore|JSON|*.json",
            DefaultExt = "glyphore",
            AddExtension = true,
            OverwritePrompt = true,
            RestoreDirectory = true,
            FileName = string.IsNullOrWhiteSpace(_scene.Name) || _scene.Name == "Untitled" ? "scene.glyphore" : _scene.Name + ".glyphore"
        };

        if (!string.IsNullOrWhiteSpace(_scenePath))
        {
            string? directory = Path.GetDirectoryName(_scenePath);
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
                dialog.InitialDirectory = directory;
            dialog.FileName = Path.GetFileName(_scenePath);
        }

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            string resolvedPath = dialog.FileName;
            _scene.Name = Path.GetFileNameWithoutExtension(resolvedPath);
            SceneFile.Save(resolvedPath, _scene);
            _scenePath = resolvedPath;
            UpdateSceneWindowTitle();
            _status.Text = Localization.English
                ? $"Scene saved · {Path.GetFileName(resolvedPath)} · {_scene.Layers.Count} layer(s)"
                : $"Escena guardada · {Path.GetFileName(resolvedPath)} · {_scene.Layers.Count} capa(s)";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Glyphoré", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RecoverSourcePresetReferences(GlyphoreScene scene)
    {
        foreach (SceneEffectLayer layer in scene.Layers)
        {
            if (!layer.Preset.Equals("Custom", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(layer.SourcePreset))
                continue;
            if (!_data.Presets.TryGetValue(layer.Effect, out var presets)) continue;

            string? bestName = null;
            double bestScore = 0.0;
            double secondScore = 0.0;
            int bestMatches = 0;
            foreach ((string presetName, Dictionary<string, System.Text.Json.JsonElement> definition) in presets)
            {
                int considered = 0;
                int matches = 0;
                foreach ((string key, System.Text.Json.JsonElement element) in definition)
                {
                    if (element.ValueKind == System.Text.Json.JsonValueKind.Number && element.TryGetDouble(out double expected) &&
                        layer.Values.TryGetValue(key, out double actual))
                    {
                        considered++;
                        double tolerance = 1e-6 * Math.Max(1.0, Math.Abs(expected));
                        if (Math.Abs(actual - expected) <= tolerance) matches++;
                    }
                    else if (key.Equals("palette", StringComparison.OrdinalIgnoreCase) && element.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        considered++;
                        if (string.Equals(layer.PaletteName, element.GetString(), StringComparison.OrdinalIgnoreCase)) matches++;
                    }
                    else if (key.Equals("shape_mode", StringComparison.OrdinalIgnoreCase) && element.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        considered++;
                        if (string.Equals(layer.ShapeMode, element.GetString(), StringComparison.OrdinalIgnoreCase)) matches++;
                    }
                }

                if (considered < 4) continue;
                double score = matches / (double)considered;
                if (score > bestScore)
                {
                    secondScore = bestScore;
                    bestScore = score;
                    bestMatches = matches;
                    bestName = presetName;
                }
                else if (score > secondScore)
                {
                    secondScore = score;
                }
            }

            // Recovery is deliberately conservative: only assign provenance when a customized
            // layer still strongly resembles one unique preset. This recovers legacy v7 scenes
            // such as a modified Meteor Rush without ever rewriting the actual saved parameters.
            if (bestName is not null && bestMatches >= 4 && bestScore >= 0.65 && bestScore - secondScore >= 0.12)
                layer.SourcePreset = bestName;
        }
    }

    private void LoadSceneDialog()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = Localization.English ? "Glyphoré scene|*.glyphore;*.json|All files|*.*" : "Escena Glyphoré|*.glyphore;*.json|Todos|*.*"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var scene = SceneFile.Load(dialog.FileName);
            RecoverSourcePresetReferences(scene);
            foreach (var layer in scene.Layers)
            {
                if (!_data.Presets.ContainsKey(layer.Effect))
                    throw new InvalidDataException($"Unknown effect in scene: {layer.Effect}");
            }

            ExitImportedMode();
            _scene = scene;
            _activeMaskId = null;
            _preview.ActiveMaskId = null;
            UpdateDiscordPresenceContext();
            _sceneReady = true;
            _scenePath = dialog.FileName;
            LoadActiveLayerIntoEditor(refreshCommonControls: true);
            _preview.RestartAnimation();
            UpdateSceneWindowTitle();
            _status.Text = Localization.English
                ? $"Scene loaded · {Path.GetFileName(dialog.FileName)} · {_scene.Layers.Count} layer(s)"
                : $"Escena cargada · {Path.GetFileName(dialog.FileName)} · {_scene.Layers.Count} capa(s)";
            InitializeHistory();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Glyphoré", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateSceneWindowTitle()
    {
        Text = string.IsNullOrWhiteSpace(_scenePath)
            ? "Glyphoré 6.0.0"
            : $"Glyphoré 6.0.0 — {Path.GetFileName(_scenePath)}";
    }
}

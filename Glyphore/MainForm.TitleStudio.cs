using System.Text.Json;

namespace Glyphore;

internal sealed partial class MainForm
{
    private sealed record TitlePrefabDefinition(
        string Name,
        string Font,
        bool Bold,
        bool Italic);

    // A prefab is a text-art font: it transforms the entered text into a multi-line
    // ASCII/Unicode banner (FIGlet-style concept). Palette, glow and animation remain
    // completely separate in the Visual Style/Preset selector.
    private static readonly TitlePrefabDefinition[] TitlePrefabs =
        [new("System Font", "Consolas", true, false),
         .. AsciiTitlePrefabGenerator.Names.Select(name => new TitlePrefabDefinition(name, "Consolas", false, false))];

    private int _titleStudioPreviewHeight = 330;

    private void OpenTitleStudio()
    {
        ExitImportedMode();
        if (!_sceneReady) return;
        if (_detachedWindows.TryActivate("title-studio")) return;

        SceneEffectLayer? activeTitle = _scene.ActiveLayer is { Effect: "ASCII Title" } current ? current : null;
        var sourceValues = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var desc in ParameterCatalog.Specific["ASCII Title"])
            sourceValues[desc.Key] = activeTitle?.Values.GetValueOrDefault(desc.Key, desc.Default) ?? desc.Default;

        TitleStudioShell shell = CreateTitleStudioShell(activeTitle);
        GlyphoreWindow dialog = shell.Dialog;
        ToolTip studioTips = shell.Tips;
        TableLayoutPanel root = shell.Root;
        RichTextBox text = shell.Text;
        SafeComboBox prefab = shell.Prefab;
        SafeComboBox visualStyle = shell.VisualStyle;
        SafeComboBox font = shell.Font;
        SafeComboBox palette = shell.Palette;
        SafeComboBox charset = shell.Charset;
        GlyphCheckBox bold = shell.Bold;
        GlyphCheckBox italic = shell.Italic;
        GlyphCheckBox animate = shell.Animate;

        // LIVE PREVIEW / CONTROLS ------------------------------------------------------------
        // The preview and the parameter editor share a real splitter. This keeps both regions
        // usable at every window size and makes the visible preview area exactly the GL viewport.
        var studioSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterWidth = 6,
            BackColor = Theme.Border,
            Margin = Padding.Empty,
            TabStop = false
        };
        root.Controls.Add(studioSplit, 0, 2);
        const int previewPanelMin = 150;
        const int controlsPanelMin = 220;

        var previewGroup = new ThemedGroupBox
        {
            Text = Localization.English ? "Live preview" : "Vista previa en directo",
            Dock = DockStyle.Fill,
            BackColor = Theme.Panel,
            ForeColor = Theme.Text,
            Margin = new Padding(0, 0, 0, 6)
        };
        var previewLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Theme.Panel,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        previewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        previewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        previewGroup.Controls.Add(previewLayout);
        studioSplit.Panel1.Controls.Add(previewGroup);

        var previewHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Black,
            Margin = new Padding(0, 2, 0, 4),
            Padding = Padding.Empty
        };
        var titlePreview = new GlPreviewControl
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Black,
            PreviewBackgroundColor = Color.Black,
            PreviewViewMode = PreviewViewMode.Fit,
            TargetFps = Math.Min(60, Math.Max(15, _scene.Fps))
        };
        Color previewBackground = Color.Black;
        GlyphoreWindow? detachedPreview = null;
        bool closingDetachedPreview = false;
        var previewDockPlaceholder = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Theme.Muted,
            BackColor = Theme.PanelRaised,
            Visible = false,
            Text = Localization.English
                ? "Live Preview is detached. Use ‘Dock preview’ to bring it back here."
                : "La Vista previa está desacoplada. Usa ‘Acoplar preview’ para traerla de vuelta aquí."
        };
        previewHost.Controls.Add(previewDockPlaceholder);
        previewHost.Controls.Add(titlePreview);
        previewLayout.Controls.Add(previewHost, 0, 0);

        // Build the isolated preview scene before wiring any callbacks that can refresh it.
        // This is important for C# definite-assignment analysis as well as runtime ordering:
        // every event handler below can safely assume that the preview already exists.
        var previewSettings = _settings.Clone();
        previewSettings.Effect = "ASCII Title";
        previewSettings.Preset = "Custom";
        var previewScene = GlyphoreScene.FromSettings(previewSettings);
        previewScene.Width = _scene.Width;
        previewScene.Height = _scene.Height;
        previewScene.Fps = Math.Min(60, Math.Max(15, _scene.Fps));
        previewScene.Duration = _scene.Duration;
        previewScene.PostProcess = new ScenePostProcess();
        previewScene.Transform = new SceneTransform();
        var previewLayer = previewScene.Layers[0];
        previewLayer.Effect = "ASCII Title";
        previewLayer.Name = "Title preview";
        // Masks belong to the real title layer, not to an isolated editor copy. Sharing the
        // same mask objects makes docked and detached Title Studio previews another viewport
        // over the exact same editor state.
        previewLayer.Masks = activeTitle?.Masks ?? [];
        titlePreview.TargetFps = previewScene.Fps;
        titlePreview.Settings = previewSettings;
        titlePreview.Scene = previewScene;
        titlePreview.ActiveMaskId = activeTitle?.Masks.Any(mask => mask.Id == _activeMaskId) == true ? _activeMaskId : null;
        titlePreview.MaskSnapping = _maskSnapping.Checked;
        titlePreview.MaskRotationSnapping = _maskRotationSnapping.Checked;
        titlePreview.MaskEdited += (_, mask, commit) =>
        {
            var owner = _scene.Layers.FirstOrDefault(layer => layer.Masks.Any(candidate => candidate.Id == mask.Id));
            if (owner is null) return;

            // The detached/docked Title Studio preview uses its own lightweight preview layer.
            // The mask objects are shared, so touch the real owner too while dragging: this makes
            // the main preview update in the same frame instead of only after MouseUp.
            owner.Touch();
            if (_activeMaskId == mask.Id)
            {
                if (commit) SyncMaskControls();
                else SyncMaskGeometryControls(mask);
            }
            _preview.Invalidate();
            if (!commit) return;
            PushSceneOnly();
            RecordHistory(force: true);
        };
        titlePreview.MaskDeleteRequested += (_, mask) =>
        {
            if (_activeMaskId == mask.Id) RemoveActiveMask();
        };

        var previewToolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Theme.Panel,
            Padding = new Padding(0, 4, 0, 0),
            Margin = Padding.Empty
        };
        GlyphSlider? dockedPreviewZoom = null;
        Label? dockedPreviewZoomValue = null;
        GlyphSlider? detachedPreviewZoom = null;
        Label? detachedPreviewZoomValue = null;
        SafeComboBox? detachedPreviewMode = null;
        bool syncingPreviewUi = false;

        void SetPreviewBackground(Color color, PreviewBackgroundMode mode)
        {
            previewBackground = color;
            titlePreview.PreviewBackgroundColor = color;
            titlePreview.PreviewBackgroundMode = mode;
            previewHost.BackColor = color;
            if (titlePreview.Parent is Control liveHost) liveHost.BackColor = color;
            titlePreview.Invalidate();
        }

        void SetPreviewZoom(int percent)
        {
            percent = Math.Clamp(percent, 10, 400);
            titlePreview.PreviewZoom = percent / 100.0;
            syncingPreviewUi = true;
            try
            {
                if (dockedPreviewZoom is not null && dockedPreviewZoom.Value != percent)
                    dockedPreviewZoom.Value = percent;
                if (detachedPreviewZoom is not null && detachedPreviewZoom.Value != percent)
                    detachedPreviewZoom.Value = percent;
                if (dockedPreviewZoomValue is not null) dockedPreviewZoomValue.Text = $"{percent}%";
                if (detachedPreviewZoomValue is not null) detachedPreviewZoomValue.Text = $"{percent}%";
            }
            finally { syncingPreviewUi = false; }
            titlePreview.Invalidate();
        }

        void SetPreviewMode(PreviewViewMode mode, SafeComboBox dockedCombo)
        {
            titlePreview.PreviewViewMode = mode;
            string value = mode switch
            {
                PreviewViewMode.Fill => "FILL",
                PreviewViewMode.Stretch => "STRETCH",
                _ => "FIT"
            };
            syncingPreviewUi = true;
            try
            {
                if (!string.Equals(dockedCombo.SelectedItem?.ToString(), value, StringComparison.Ordinal))
                    dockedCombo.SelectedItem = value;
                if (detachedPreviewMode is not null && !string.Equals(detachedPreviewMode.SelectedItem?.ToString(), value, StringComparison.Ordinal))
                    detachedPreviewMode.SelectedItem = value;
            }
            finally { syncingPreviewUi = false; }
            titlePreview.Invalidate();
        }

        Button AddPreviewBackgroundButton(FlowLayoutPanel host, string text, int width, Action action)
        {
            var button = Btn(text, (_, _) => action());
            button.Width = width;
            button.Height = 32;
            button.Margin = new Padding(0, 0, 5, 0);
            host.Controls.Add(button);
            return button;
        }

        var darkBgButton = AddPreviewBackgroundButton(previewToolbar, Localization.English ? "Dark" : "Oscuro", 64,
            () => SetPreviewBackground(Color.FromArgb(18, 18, 20), PreviewBackgroundMode.Solid));
        var lightBgButton = AddPreviewBackgroundButton(previewToolbar, Localization.English ? "Light" : "Claro", 64,
            () => SetPreviewBackground(Color.FromArgb(236, 236, 238), PreviewBackgroundMode.Solid));
        var checkerBgButton = AddPreviewBackgroundButton(previewToolbar, "Checker", 78,
            () => SetPreviewBackground(Color.Black, PreviewBackgroundMode.Checkerboard));
        var bgButton = AddPreviewBackgroundButton(previewToolbar, Localization.English ? "Custom" : "Personalizado", 92, () =>
        {
            using var picker = new ColorDialog { Color = previewBackground, FullOpen = true };
            if (picker.ShowDialog(dialog) != DialogResult.OK) return;
            SetPreviewBackground(picker.Color, PreviewBackgroundMode.Solid);
        });
        bgButton.Margin = new Padding(0, 0, 8, 0);

        // Build previewMode before any callback can detach the preview. DetachPreview
        // reads this control to mirror the current framing mode in the detached window.
        var previewMode = new SafeComboBox
        {
            Width = 104,
            Margin = new Padding(0, 1, 8, 0)
        };
        SetupCombo(previewMode);
        previewMode.Items.AddRange(["FIT", "FILL", "STRETCH"]);
        previewMode.SelectedItem = "FIT";
        previewMode.SelectedIndexChanged += (_, _) =>
        {
            if (syncingPreviewUi) return;
            SetPreviewMode(previewMode.SelectedItem?.ToString() switch
            {
                "FILL" => PreviewViewMode.Fill,
                "STRETCH" => PreviewViewMode.Stretch,
                _ => PreviewViewMode.Fit
            }, previewMode);
        };

        var replayButton = Btn(Localization.English ? "↻ Replay" : "↻ Repetir", (_, _) =>
        {
            NotifyDiscordPreviewing();
            titlePreview.RestartAnimation();
        });
        replayButton.Width = 92;
        replayButton.Height = 32;
        replayButton.Margin = new Padding(0, 0, 8, 0);
        previewToolbar.Controls.Add(replayButton);
        studioTips.SetToolTip(replayButton, Localization.English
            ? "Restarts the Title Studio animation clock so Fade Reveal, shimmer, wave and other animated effects can be previewed again immediately."
            : "Reinicia el reloj de animación del Estudio de títulos para volver a ver al instante Fade Reveal, shimmer, wave y otros efectos animados.");

        var detachButton = Btn(Localization.English ? "↗ Detach preview" : "↗ Desacoplar preview", (_, _) => { });
        detachButton.Click += (_, _) =>
        {
            if (detachedPreview is null) DetachPreview();
            else DockPreview();
        };
        detachButton.Width = 164;
        detachButton.Height = 32;
        detachButton.Margin = new Padding(0, 0, 8, 0);
        previewToolbar.Controls.Add(detachButton);
        previewToolbar.Controls.Add(new Label
        {
            AutoSize = true,
            Text = Localization.English ? "Framing" : "Encuadre",
            ForeColor = Theme.Muted,
            Margin = new Padding(2, 8, 5, 0)
        });
        previewToolbar.Controls.Add(previewMode);
        studioTips.SetToolTip(previewMode, Localization.English
            ? "FIT shows the whole scene with letterboxing when needed. FILL preserves aspect ratio and crops. STRETCH fills without cropping but may distort cell proportions."
            : "FIT muestra toda la escena con bandas si hacen falta. FILL conserva la proporción y recorta. STRETCH ocupa todo sin recortar, pero puede deformar la proporción de las celdas.");

        previewToolbar.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "Zoom",
            ForeColor = Theme.Muted,
            Margin = new Padding(2, 8, 5, 0)
        });
        dockedPreviewZoom = new GlyphSlider
        {
            Width = 120,
            Height = 30,
            Minimum = 10,
            Maximum = 400,
            Value = 100,
            MouseWheelAdjustsValue = false,
            Margin = new Padding(0, 0, 4, 0)
        };
        dockedPreviewZoomValue = new Label
        {
            AutoSize = false,
            Width = 46,
            Height = 30,
            Text = "100%",
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Theme.AccentText,
            Margin = new Padding(0, 0, 5, 0)
        };
        dockedPreviewZoom.ValueChanged += (_, _) =>
        {
            if (!syncingPreviewUi) SetPreviewZoom(dockedPreviewZoom.Value);
        };
        previewToolbar.Controls.Add(dockedPreviewZoom);
        previewToolbar.Controls.Add(dockedPreviewZoomValue);
        studioTips.SetToolTip(dockedPreviewZoom, Localization.English
            ? "Editor-only zoom. Values below 100% let you see the title from farther away without changing its scene size."
            : "Zoom exclusivo del editor. Valores inferiores a 100% permiten ver el título desde más lejos sin cambiar su tamaño dentro de la escena.");

        previewToolbar.Controls.Add(new Label
        {
            AutoSize = true,
            Text = Localization.English ? "Changes appear immediately." : "Los cambios aparecen al instante.",
            ForeColor = Theme.Muted,
            Margin = new Padding(6, 8, 0, 0)
        });
        previewLayout.Controls.Add(previewToolbar, 0, 1);
        studioTips.SetToolTip(bgButton, Localization.English
            ? "Changes only the Live Preview background color. It does not modify the title or the scene."
            : "Cambia únicamente el color de fondo de la vista previa. No modifica el título ni la escena.");
        studioTips.SetToolTip(detachButton, Localization.English
            ? "Moves the same live OpenGL preview into an independent top-level window. The docked preview area collapses to give the title controls more room."
            : "Mueve la misma preview OpenGL en directo a una ventana top-level independiente. El área acoplada se contrae para dejar más espacio a los ajustes del título.");

        // TITLE CONTROLS ---------------------------------------------------------------------
        var controlsGroup = new ThemedGroupBox
        {
            Text = Localization.English ? "Title controls" : "Ajustes del título",
            Dock = DockStyle.Fill,
            BackColor = Theme.Panel,
            ForeColor = Theme.Text,
            Margin = new Padding(0, 0, 0, 4)
        };
        var scroll = new ThemedScrollPanel
        {
            Dock = DockStyle.Fill,
            WheelStepPixels = 22,
            BackColor = Theme.Panel,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            TabStop = true
        };
        controlsGroup.Controls.Add(scroll);
        studioSplit.Panel2.Controls.Add(controlsGroup);

        var settingsStack = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 0,
            BackColor = Theme.Panel,
            Margin = Padding.Empty,
            Padding = new Padding(2)
        };
        settingsStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        scroll.SetContent(settingsStack);

        bool applyingDialog = false;

        var rows = new Dictionary<string, ParameterRow>(StringComparer.OrdinalIgnoreCase);
        foreach (var desc in ParameterCatalog.Specific["ASCII Title"])
        {
            var row = new ParameterRow(desc, sourceValues[desc.Key])
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 1, 6, 1),
                MouseWheelEditsValue = false
            };
            rows[desc.Key] = row;
        }

        var colorSynchronizers = new List<Action>();

        Label Subheading(string text)
        {
            return new Label
            {
                Text = text.ToUpperInvariant(),
                AutoSize = true,
                Dock = DockStyle.Fill,
                ForeColor = Theme.Muted,
                Font = new Font("Segoe UI Semibold", 8f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(8, 8, 8, 2),
                Padding = new Padding(0, 2, 0, 0)
            };
        }

        Control BuildColorEditor(string caption, string redKey, string greenKey, string blueKey, string alphaKey, string tooltipEs, string tooltipEn)
        {
            var editor = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 5,
                RowCount = 1,
                BackColor = Theme.PanelRaised,
                Margin = new Padding(6, 2, 6, 4),
                Padding = Padding.Empty,
                MinimumSize = new Size(0, 38)
            };
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74));

            var label = new Label
            {
                Text = caption,
                Dock = DockStyle.Fill,
                ForeColor = Theme.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = Padding.Empty
            };
            var swatch = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 7, 6, 7),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };
            var choose = Btn(Localization.English ? "Choose color" : "Elegir color", (_, _) => { });
            choose.Dock = DockStyle.Fill;
            choose.Margin = new Padding(0, 3, 8, 3);
            var alphaLabel = new Label
            {
                Text = Localization.English ? "Opacity" : "Opacidad",
                Dock = DockStyle.Fill,
                ForeColor = Theme.Muted,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 0, 5, 0)
            };
            var alpha = new GlyphNumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 100,
                DecimalPlaces = 0,
                Increment = 1,
                Margin = new Padding(0, 5, 0, 5)
            };
            Theme.Numeric(alpha);

            editor.Controls.Add(label, 0, 0);
            editor.Controls.Add(swatch, 1, 0);
            editor.Controls.Add(choose, 2, 0);
            editor.Controls.Add(alphaLabel, 3, 0);
            editor.Controls.Add(alpha, 4, 0);

            bool syncing = false;
            Color CurrentColor()
            {
                int rr = Math.Clamp((int)Math.Round(rows[redKey].Value * 255.0), 0, 255);
                int gg = Math.Clamp((int)Math.Round(rows[greenKey].Value * 255.0), 0, 255);
                int bb = Math.Clamp((int)Math.Round(rows[blueKey].Value * 255.0), 0, 255);
                return Color.FromArgb(rr, gg, bb);
            }

            void SyncColor()
            {
                syncing = true;
                try
                {
                    swatch.BackColor = CurrentColor();
                    alpha.Value = Math.Clamp((decimal)Math.Round(rows[alphaKey].Value * 100.0), alpha.Minimum, alpha.Maximum);
                }
                finally { syncing = false; }
            }

            choose.Click += (_, _) =>
            {
                using var picker = new ColorDialog { Color = CurrentColor(), FullOpen = true };
                if (picker.ShowDialog(dialog) != DialogResult.OK) return;
                applyingDialog = true;
                try
                {
                    rows[redKey].SetValue(picker.Color.R / 255.0);
                    rows[greenKey].SetValue(picker.Color.G / 255.0);
                    rows[blueKey].SetValue(picker.Color.B / 255.0);
                }
                finally { applyingDialog = false; }
                SyncColor();
                MarkStyleCustom();
            };
            swatch.Click += (_, _) => choose.PerformClick();
            alpha.ValueChanged += (_, _) =>
            {
                if (syncing) return;
                applyingDialog = true;
                try { rows[alphaKey].SetValue((double)alpha.Value / 100.0); }
                finally { applyingDialog = false; }
                MarkStyleCustom();
            };

            string tip = Localization.English ? tooltipEn : tooltipEs;
            studioTips.SetToolTip(editor, tip);
            studioTips.SetToolTip(label, tip);
            studioTips.SetToolTip(swatch, tip);
            studioTips.SetToolTip(choose, tip);
            studioTips.SetToolTip(alphaLabel, tip);
            studioTips.SetToolTip(alpha, tip);
            colorSynchronizers.Add(SyncColor);
            SyncColor();
            return editor;
        }

        void AddCategory(string title, params Control[] items)
        {
            var category = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = items.Length + 1,
                BackColor = Theme.PanelRaised,
                Padding = new Padding(6, 5, 6, 7),
                Margin = new Padding(2, 2, 2, 8)
            };
            category.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            category.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            category.Controls.Add(new Label
            {
                Text = title.ToUpperInvariant(),
                Dock = DockStyle.Fill,
                ForeColor = Theme.AccentText,
                Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(6, 0, 6, 2)
            }, 0, 0);

            for (int index = 0; index < items.Length; index++)
            {
                Control item = items[index];
                item.Dock = DockStyle.Fill;
                if (item is ParameterRow) item.Margin = new Padding(6, 1, 6, 1);
                category.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                category.Controls.Add(item, 0, index + 1);
            }

            settingsStack.RowCount++;
            settingsStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            settingsStack.Controls.Add(category, 0, settingsStack.RowCount - 1);
        }

        var outlineColorEditor = BuildColorEditor(
            Localization.English ? "Outline color" : "Color del contorno",
            "title_outline_r", "title_outline_g", "title_outline_b", "title_outline_a",
            "Cambia el color del contorno.",
            "Changes the outline color.");
        var shadowColorEditor = BuildColorEditor(
            Localization.English ? "Shadow color" : "Color de sombra",
            "title_shadow_r", "title_shadow_g", "title_shadow_b", "title_shadow_a",
            "Cambia el color de la sombra.",
            "Changes the shadow color.");
        var extrusionColorEditor = BuildColorEditor(
            Localization.English ? "3D color" : "Color 3D",
            "title_extrude_r", "title_extrude_g", "title_extrude_b", "title_extrude_a",
            "Cambia el color de la extrusión 3D.",
            "Changes the 3D extrusion color.");
        var shimmerColorEditor = BuildColorEditor(
            Localization.English ? "Shimmer color" : "Color shimmer",
            "title_shimmer_r", "title_shimmer_g", "title_shimmer_b", "title_shimmer_a",
            "Cambia el color de la banda de brillo.",
            "Changes the shimmer band color.");

        const double recommendedExtrusionQuality = 16.0;
        var extrusionQualityWarning = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ForeColor = Theme.AccentText,
            Margin = new Padding(8, 0, 8, 5),
            Padding = new Padding(0, 1, 0, 2),
            Text = Localization.English
                ? "⚠ 3D quality above 16 can significantly reduce preview and export performance."
                : "⚠ Una calidad 3D superior a 16 puede reducir bastante el rendimiento de preview y exportación."
        };

        void SyncExtrusionQualityWarning()
        {
            bool show = rows["title_extrude_quality"].Value > recommendedExtrusionQuality;
            if (extrusionQualityWarning.Visible != show) extrusionQualityWarning.Visible = show;
        }

        SyncExtrusionQualityWarning();

        var mixCustomPalette = new GlyphCheckBox
        {
            Text = Localization.English ? "Mix custom colors with palette" : "Mezclar colores personalizados con la paleta",
            Checked = rows["title_mix_custom_palette"].Value >= 0.5,
            AutoSize = true,
            ForeColor = Theme.Text,
            Margin = new Padding(0, 0, 0, 0)
        };
        Theme.CheckBox(mixCustomPalette);
        var mixCustomPaletteRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Theme.PanelRaised,
            Margin = new Padding(8, 4, 8, 5),
            Padding = new Padding(0, 2, 0, 2)
        };
        mixCustomPaletteRow.Controls.Add(mixCustomPalette);
        string mixPaletteTip = Localization.English
            ? "Decides whether the palette influences custom colors."
            : "Decide si la paleta influye en los colores personalizados.";
        studioTips.SetToolTip(mixCustomPalette, mixPaletteTip);
        studioTips.SetToolTip(mixCustomPaletteRow, mixPaletteTip);
        mixCustomPalette.CheckedChanged += (_, _) =>
        {
            if (applyingDialog) return;
            rows["title_mix_custom_palette"].SetValue(mixCustomPalette.Checked ? 1.0 : 0.0);
            MarkStyleCustom();
        };
        colorSynchronizers.Add(() =>
        {
            bool value = rows["title_mix_custom_palette"].Value >= 0.5;
            if (mixCustomPalette.Checked != value) mixCustomPalette.Checked = value;
        });

        AddCategory(Localization.English ? "Typography" : "Tipografía",
            rows["title_size"], rows["title_letter_spacing"]);

        AddCategory(Localization.English ? "Transform" : "Transformación",
            rows["title_x"], rows["title_y"], rows["title_rotation"],
            rows["title_perspective_x"], rows["title_perspective_y"]);

        AddCategory(Localization.English ? "Appearance" : "Apariencia",
            mixCustomPaletteRow,
            Subheading(Localization.English ? "Outline & glow" : "Contorno y resplandor"),
            rows["title_outline"], outlineColorEditor, rows["title_glow"],
            Subheading(Localization.English ? "Shadow" : "Sombra"),
            rows["title_shadow_x"], rows["title_shadow_y"], rows["title_shadow"], shadowColorEditor,
            Subheading(Localization.English ? "3D extrusion" : "Extrusión 3D"),
            rows["title_extrude_depth"], rows["title_extrude_x"], rows["title_extrude_y"],
            rows["title_extrude_mode"], rows["title_extrude_target_x"], rows["title_extrude_target_y"], rows["title_extrude_convergence"],
            rows["title_extrude_opacity"], rows["title_extrude_quality"], extrusionQualityWarning, extrusionColorEditor,
            rows["title_crystal"]);

        var animateRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Theme.PanelRaised,
            Margin = new Padding(8, 4, 8, 5),
            Padding = new Padding(0, 2, 0, 2)
        };
        animate.Margin = Padding.Empty;
        animateRow.Controls.Add(animate);

        AddCategory(Localization.English ? "Animation" : "Animación",
            animateRow,
            Subheading(Localization.English ? "Wave" : "Onda"),
            rows["title_wave"], rows["title_wave_x"], rows["title_wave_length"], rows["title_wave_speed"], rows["title_wave_phase"],
            Subheading("Shimmer"),
            rows["title_shimmer"], rows["title_shimmer_speed"], rows["title_shimmer_frequency"],
            rows["title_shimmer_width"], rows["title_shimmer_randomness"], rows["title_shimmer_phase"], rows["title_shimmer_extrusion"], shimmerColorEditor,
            Subheading(Localization.English ? "Fade reveal" : "Fade reveal"),
            rows["title_fade_mode"], rows["title_fade_progress"], rows["title_fade_softness"],
            rows["title_fade_offset"], rows["title_fade_duration"], rows["title_fade_easing"],
            Subheading(Localization.English ? "Other temporal effects" : "Otros efectos temporales"),
            rows["title_reveal"], rows["title_glitch"]);

        var info = new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = Theme.Muted,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            Text = Localization.English
                ? "Live Preview is immediate. Apply to active updates the selected ASCII Title layer; Add to scene creates a new layer."
                : "La Vista previa es inmediata. Aplicar a activa actualiza la capa ASCII Title seleccionada; Añadir a escena crea una capa nueva."
        };
        root.Controls.Add(info, 0, 3);

        string SelectedFont() => string.IsNullOrWhiteSpace(font.Text) ? "Consolas" : font.Text;
        string SelectedPalette() => string.IsNullOrWhiteSpace(palette.Text) ? _settings.PaletteName : palette.Text;
        string SelectedCharset() => string.IsNullOrWhiteSpace(charset.Text) ? _settings.CharsetName : charset.Text;
        int SelectedLetterSpacing() => Math.Max(0, (int)Math.Round(rows["title_letter_spacing"].Value));
        string SelectedCharsetRamp()
        {
            string name = SelectedCharset();
            if (_data.Charsets.TryGetValue(name, out string? ramp))
                return string.IsNullOrEmpty(ramp) ? " " : ramp;
            if (activeTitle is not null && name.Equals(activeTitle.CharsetName, StringComparison.OrdinalIgnoreCase))
                return string.IsNullOrEmpty(activeTitle.Charset) ? " " : activeTitle.Charset;
            if (name.Equals(_settings.CharsetName, StringComparison.OrdinalIgnoreCase))
                return string.IsNullOrEmpty(_settings.Charset) ? " " : _settings.Charset;
            return " ";
        }
        string SelectedPrefab() => string.IsNullOrWhiteSpace(prefab.Text) ? "System Font" : prefab.Text;
        string SelectedStyle() => string.IsNullOrWhiteSpace(visualStyle.Text) ? "Custom" : visualStyle.Text;
        string TitleText() => text.Text.Length == 0 ? " " : text.Text[..Math.Min(text.Text.Length, 512)];

        void EnsurePreviewFillsHost()
        {
            if (titlePreview.IsDisposed || titlePreview.Parent is not Control host || host.IsDisposed) return;
            titlePreview.Dock = DockStyle.Fill;
            host.BackColor = titlePreview.PreviewBackgroundColor;
            titlePreview.BringToFront();
            host.PerformLayout();
            titlePreview.Invalidate();
        }

        void RestoreDockedPreviewLayout()
        {
            studioSplit.Panel1Collapsed = false;
            studioSplit.Panel1MinSize = 0;
            studioSplit.Panel2MinSize = 0;
            int maximum = studioSplit.Height - controlsPanelMin - studioSplit.SplitterWidth;
            if (maximum >= previewPanelMin)
            {
                studioSplit.SplitterDistance = Math.Clamp(_titleStudioPreviewHeight, previewPanelMin, maximum);
                studioSplit.Panel1MinSize = previewPanelMin;
                studioSplit.Panel2MinSize = controlsPanelMin;
            }
        }

        void DockPreview()
        {
            GlyphoreWindow? current = detachedPreview;
            if (current is null || current.IsDisposed) return;

            GlyphoreWindow window = current;
            detachedPreview = null;
            RestoreDockedPreviewLayout();
            titlePreview.Parent = previewHost;
            titlePreview.Dock = DockStyle.Fill;
            previewDockPlaceholder.Visible = false;
            previewDockPlaceholder.SendToBack();
            titlePreview.BringToFront();
            detachButton.Text = Localization.English ? "↗ Detach preview" : "↗ Desacoplar preview";
            detachedPreviewZoom = null;
            detachedPreviewZoomValue = null;
            detachedPreviewMode = null;

            closingDetachedPreview = true;
            try { window.Close(); }
            finally
            {
                closingDetachedPreview = false;
                if (!window.IsDisposed) window.Dispose();
            }
            EnsurePreviewFillsHost();
        }

        void DetachPreview()
        {
            GlyphoreWindow? current = detachedPreview;
            if (current is not null && !current.IsDisposed)
            {
                if (current.WindowState == FormWindowState.Minimized)
                    current.WindowState = FormWindowState.Normal;
                current.Show();
                current.BringToFront();
                current.Activate();
                return;
            }

            var window = new GlyphoreWindow
            {
                Text = Localization.English ? "Live Preview · Glyphoré" : "Vista previa en directo · Glyphoré",
                StartPosition = FormStartPosition.Manual,
                MinimumSize = new Size(560, 360),
                ClientSize = new Size(Math.Max(760, previewHost.Width), Math.Max(460, previewHost.Height + 50)),
                BackColor = Theme.Bg,
                ForeColor = Theme.Text,
                ShowInTaskbar = true,
                ShowIcon = true,
                MinimizeBox = true,
                MaximizeBox = true,
                Resizable = true
            };
            if (Icon is not null) window.Icon = Icon;

            Rectangle work = Screen.FromControl(dialog).WorkingArea;
            int maxWidth = Math.Max(window.MinimumSize.Width, work.Width - 32);
            int maxHeight = Math.Max(window.MinimumSize.Height, work.Height - 32);
            if (window.Width > maxWidth || window.Height > maxHeight)
                window.Size = new Size(Math.Min(window.Width, maxWidth), Math.Min(window.Height, maxHeight));

            int gap = 18;
            int preferredRight = dialog.Right + gap;
            int preferredLeft = dialog.Left - gap - window.Width;
            int x;
            if (preferredRight + window.Width <= work.Right)
                x = preferredRight;
            else if (preferredLeft >= work.Left)
                x = preferredLeft;
            else
                x = Math.Clamp(dialog.Left + 54, work.Left, Math.Max(work.Left, work.Right - window.Width));

            int y = Math.Clamp(dialog.Top, work.Top, Math.Max(work.Top, work.Bottom - window.Height));
            window.Location = new Point(x, y);

            var detachedRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.Bg,
                Padding = new Padding(10),
                Margin = Padding.Empty
            };
            detachedRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            detachedRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));

            var host = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = previewBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            var detachedTools = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                BackColor = Theme.Bg,
                Padding = new Padding(0, 7, 0, 0),
                Margin = Padding.Empty
            };

            AddPreviewBackgroundButton(detachedTools, Localization.English ? "Dark" : "Oscuro", 64,
                () => SetPreviewBackground(Color.FromArgb(18, 18, 20), PreviewBackgroundMode.Solid));
            AddPreviewBackgroundButton(detachedTools, Localization.English ? "Light" : "Claro", 64,
                () => SetPreviewBackground(Color.FromArgb(236, 236, 238), PreviewBackgroundMode.Solid));
            AddPreviewBackgroundButton(detachedTools, "Checker", 78,
                () => SetPreviewBackground(Color.Black, PreviewBackgroundMode.Checkerboard));
            AddPreviewBackgroundButton(detachedTools, Localization.English ? "Custom" : "Personalizado", 92, () =>
            {
                using var picker = new ColorDialog { Color = previewBackground, FullOpen = true };
                if (picker.ShowDialog(window) != DialogResult.OK) return;
                SetPreviewBackground(picker.Color, PreviewBackgroundMode.Solid);
            });

            var detachedReplay = Btn(Localization.English ? "↻ Replay" : "↻ Repetir", (_, _) =>
            {
                NotifyDiscordPreviewing();
                titlePreview.RestartAnimation();
            });
            detachedReplay.Width = 92;
            detachedReplay.Height = 32;
            detachedReplay.Margin = new Padding(3, 0, 6, 0);
            detachedTools.Controls.Add(detachedReplay);

            detachedTools.Controls.Add(new Label
            {
                Text = Localization.English ? "Framing" : "Encuadre",
                ForeColor = Theme.Muted,
                AutoSize = true,
                Margin = new Padding(4, 8, 4, 0)
            });
            var detachedMode = new SafeComboBox { Width = 100, Margin = new Padding(0, 1, 7, 0) };
            detachedPreviewMode = detachedMode;
            SetupCombo(detachedMode);
            detachedMode.Items.AddRange(["FIT", "FILL", "STRETCH"]);
            detachedMode.SelectedItem = previewMode.SelectedItem ?? "FIT";
            detachedMode.SelectedIndexChanged += (_, _) =>
            {
                if (syncingPreviewUi) return;
                SetPreviewMode(detachedMode.SelectedItem?.ToString() switch
                {
                    "FILL" => PreviewViewMode.Fill,
                    "STRETCH" => PreviewViewMode.Stretch,
                    _ => PreviewViewMode.Fit
                }, previewMode);
            };
            detachedTools.Controls.Add(detachedMode);

            detachedTools.Controls.Add(new Label
            {
                Text = "Zoom",
                ForeColor = Theme.Muted,
                AutoSize = true,
                Margin = new Padding(3, 8, 4, 0)
            });
            var detachedZoom = new GlyphSlider
            {
                Width = 122,
                Height = 30,
                Minimum = 10,
                Maximum = 400,
                Value = Math.Clamp((int)Math.Round(titlePreview.PreviewZoom * 100.0), 10, 400),
                MouseWheelAdjustsValue = false,
                Margin = new Padding(0, 0, 4, 0)
            };
            detachedPreviewZoom = detachedZoom;
            var detachedZoomValue = new Label
            {
                Text = $"{detachedZoom.Value}%",
                ForeColor = Theme.AccentText,
                AutoSize = false,
                Width = 46,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 6, 0)
            };
            detachedPreviewZoomValue = detachedZoomValue;
            detachedZoom.ValueChanged += (_, _) =>
            {
                if (!syncingPreviewUi)
                    SetPreviewZoom(detachedZoom.Value);
            };
            detachedTools.Controls.Add(detachedZoom);
            detachedTools.Controls.Add(detachedZoomValue);

            var dockButton = Btn(Localization.English ? "↙ Dock preview" : "↙ Acoplar preview", (_, _) => DockPreview());
            dockButton.Width = 154;
            dockButton.Height = 32;
            dockButton.Margin = new Padding(3, 0, 0, 0);
            detachedTools.Controls.Add(dockButton);
            studioTips.SetToolTip(dockButton, Localization.English
                ? "Returns this live preview to ASCII Title Studio."
                : "Devuelve esta preview en directo al Estudio de títulos ASCII.");
            studioTips.SetToolTip(detachedZoom, Localization.English
                ? "Editor-only zoom. It never changes title geometry or exported size."
                : "Zoom exclusivo del editor. Nunca cambia la geometría del título ni el tamaño exportado.");

            detachedRoot.Controls.Add(host, 0, 0);
            detachedRoot.Controls.Add(detachedTools, 0, 1);
            window.ContentPanel.Controls.Add(detachedRoot);

            detachedPreview = window;
            if (!studioSplit.Panel1Collapsed && studioSplit.SplitterDistance > 0)
                _titleStudioPreviewHeight = studioSplit.SplitterDistance;
            previewDockPlaceholder.Visible = false;
            titlePreview.Parent = host;
            titlePreview.Dock = DockStyle.Fill;
            titlePreview.BringToFront();
            studioSplit.Panel1Collapsed = true;
            EnsurePreviewFillsHost();
            detachButton.Text = Localization.English ? "↙ Dock preview" : "↙ Acoplar preview";

            window.FormClosing += (_, e) =>
            {
                if (closingDetachedPreview) return;
                if (dialog.IsDisposed || !dialog.IsHandleCreated) return;

                // User-close means “dock back”, but application shutdown must still be able to
                // destroy the detached HWND. Title Studio sets closingDetachedPreview for that path.
                e.Cancel = true;
                dialog.BeginInvoke((Action)DockPreview);
            };
            window.FormClosed += (_, _) =>
            {
                if (ReferenceEquals(detachedPreview, window)) detachedPreview = null;
                detachedPreviewZoom = null;
                detachedPreviewZoomValue = null;
                detachedPreviewMode = null;
            };

            // Deliberately ownerless and modeless: its HWND/taskbar/Alt+Tab/minimize state are
            // independent from Main and Title Studio while still living in the same process.
            window.Show();
            window.BringToFront();
            window.Activate();
        }

        void UpdatePrefabUiState()
        {
            bool usesSystemFont = SelectedPrefab() is "System Font" or "Custom";
            font.Enabled = usesSystemFont;
            bold.Enabled = usesSystemFont;
            italic.Enabled = usesSystemFont;
        }

        bool CurrentStyleUsesAnimation()
        {
            return rows["title_shimmer"].Value > .001 ||
                   Math.Abs(rows["title_wave"].Value) > .001 ||
                   Math.Abs(rows["title_wave_x"].Value) > .001 ||
                   rows["title_reveal"].Value > .001 ||
                   rows["title_fade_mode"].Value > .5 ||
                   rows["title_glitch"].Value > .001;
        }

        void RefreshPreview()
        {
            if (dialog.IsDisposed) return;
            previewLayer.Effect = "ASCII Title";
            previewLayer.Preset = SelectedStyle();
            previewLayer.TitlePrefab = SelectedPrefab();
            previewLayer.TitleText = TitleText();
            previewLayer.TitleFont = SelectedFont();
            previewLayer.TitleBold = bold.Checked;
            previewLayer.TitleItalic = italic.Checked;
            previewLayer.TitleAnimate = animate.Checked;
            foreach (var pair in rows) previewLayer.Values[pair.Key] = pair.Value.Value;
            if (_data.Palettes.TryGetValue(SelectedPalette(), out var stops)) previewLayer.SetPalette(SelectedPalette(), stops);
            previewLayer.SetCharset(SelectedCharset(), SelectedCharsetRamp());
            previewLayer.Touch();
            titlePreview.Settings = previewScene.CreateSettings(previewLayer);
            EnsurePreviewFillsHost();
            titlePreview.Invalidate();
        }

        void MarkPrefabCustom()
        {
            if (applyingDialog) return;
            if (!string.Equals(prefab.Text, "Custom", StringComparison.Ordinal))
            {
                applyingDialog = true;
                prefab.SelectedItem = "Custom";
                applyingDialog = false;
            }
            UpdatePrefabUiState();
            RefreshPreview();
        }

        void MarkStyleCustom()
        {
            if (applyingDialog) return;
            if (!string.Equals(visualStyle.Text, "Custom", StringComparison.Ordinal))
            {
                applyingDialog = true;
                visualStyle.SelectedItem = "Custom";
                applyingDialog = false;
            }
            RefreshPreview();
        }

        void ApplyPrefab(string name)
        {
            if (string.Equals(name, "Custom", StringComparison.OrdinalIgnoreCase))
            {
                UpdatePrefabUiState();
                RefreshPreview();
                return;
            }

            var definition = TitlePrefabs.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (definition is null) return;

            applyingDialog = true;
            try
            {
                // Only System Font is backed by an installed Windows font. FIGlet prefabs are
                // generated by Glyphoré and do not depend on the Base font selector.
                if (definition.Name == "System Font")
                {
                    if (!font.Items.Contains(definition.Font)) font.Items.Add(definition.Font);
                    font.SelectedItem = definition.Font;
                    bold.Checked = definition.Bold;
                    italic.Checked = definition.Italic;
                }
            }
            finally { applyingDialog = false; }

            UpdatePrefabUiState();
            RefreshPreview();
        }

        void ApplyVisualStyle(string name)
        {
            if (string.Equals(name, "Custom", StringComparison.OrdinalIgnoreCase))
            {
                RefreshPreview();
                return;
            }
            if (!_data.Presets.TryGetValue("ASCII Title", out var group) || !group.TryGetValue(name, out var definition))
                return;

            applyingDialog = true;
            try
            {
                foreach (var desc in ParameterCatalog.Specific["ASCII Title"])
                    if (rows.TryGetValue(desc.Key, out var row))
                        row.SetValue(desc.Default);

                foreach (var item in definition)
                {
                    if (item.Value.ValueKind == JsonValueKind.Number && item.Value.TryGetDouble(out double numeric))
                    {
                        if (rows.TryGetValue(item.Key, out var row)) row.SetValue(numeric);
                    }
                    else if (item.Key.Equals("palette", StringComparison.OrdinalIgnoreCase) && item.Value.ValueKind == JsonValueKind.String)
                    {
                        string paletteName = item.Value.GetString() ?? SelectedPalette();
                        if (palette.Items.Contains(paletteName)) palette.SelectedItem = paletteName;
                    }
                }

                foreach (var syncColor in colorSynchronizers) syncColor();

                // A preset whose visible identity is temporal should behave as advertised the
                // moment it is selected. The user can still freeze it afterwards with Animate.
                if (CurrentStyleUsesAnimation()) animate.Checked = true;
            }
            finally { applyingDialog = false; }

            if (animate.Checked && CurrentStyleUsesAnimation()) titlePreview.RestartAnimation();
            RefreshPreview();
        }

        prefab.SelectedIndexChanged += (_, _) =>
        {
            if (applyingDialog) return;
            ApplyPrefab(prefab.Text);
        };
        visualStyle.SelectedIndexChanged += (_, _) =>
        {
            if (applyingDialog) return;
            ApplyVisualStyle(visualStyle.Text);
        };
        text.TextChanged += (_, _) => RefreshPreview();
        font.SelectedIndexChanged += (_, _) => MarkPrefabCustom();
        palette.SelectedIndexChanged += (_, _) => MarkStyleCustom();
        charset.SelectedIndexChanged += (_, _) => RefreshPreview();
        bold.CheckedChanged += (_, _) => MarkPrefabCustom();
        italic.CheckedChanged += (_, _) => MarkPrefabCustom();
        animate.CheckedChanged += (_, _) =>
        {
            if (applyingDialog) return;
            if (animate.Checked) titlePreview.RestartAnimation();
            RefreshPreview();
        };
        foreach (var row in rows.Values) row.ValueChanged += _ => MarkStyleCustom();
        rows["title_extrude_quality"].ValueChanged += _ => SyncExtrusionQualityWarning();

        if (activeTitle is null)
        {
            applyingDialog = true;
            prefab.SelectedItem = "FIGlet · Standard";
            visualStyle.SelectedItem = visualStyle.Items.Contains("Neon Title") ? "Neon Title" : "Custom";
            animate.Checked = true;
            applyingDialog = false;
            ApplyPrefab("FIGlet · Standard");
            if (visualStyle.Text != "Custom") ApplyVisualStyle(visualStyle.Text);
            else
            {
                titlePreview.RestartAnimation();
                RefreshPreview();
            }
        }
        else
        {
            UpdatePrefabUiState();
            RefreshPreview();
            if (animate.Checked) titlePreview.RestartAnimation();
        }

        // ACTIONS ----------------------------------------------------------------------------
        SceneEffectLayer CommitToScene(bool applyToActive)
        {
            string selectedFont = SelectedFont();
            string selectedPalette = SelectedPalette();
            string selectedCharset = SelectedCharset();
            string titleText = TitleText();

            SceneEffectLayer? currentTitle = _scene.ActiveLayer is { Effect: "ASCII Title" } selected ? selected : null;
            SceneEffectLayer target;
            if (applyToActive)
            {
                if (currentTitle is null)
                    throw new InvalidOperationException(Localization.English
                        ? "Select an ASCII Title layer in the main window first."
                        : "Selecciona primero una capa ASCII Title en la ventana principal.");
                target = currentTitle;
            }
            else
            {
                if (_scene.Layers.Count >= GlyphoreScene.MaxLayers)
                    throw new InvalidOperationException(Localization.English ? "The scene has reached the layer limit." : "La escena ha alcanzado el límite de capas.");
                SyncSceneFromSettings();
                target = SceneEffectLayer.FromSettings(_settings, UniqueLayerName(Localization.English ? "ASCII Title" : "Título ASCII"));
                target.Effect = "ASCII Title";
                _scene.Layers.Insert(0, target);
                _scene.ActiveLayerId = target.Id;
            }

            target.Effect = "ASCII Title";
            target.Preset = SelectedStyle();
            target.TitlePrefab = SelectedPrefab();
            target.TitleText = titleText;
            target.TitleFont = selectedFont;
            target.TitleBold = bold.Checked;
            target.TitleItalic = italic.Checked;
            target.TitleAnimate = animate.Checked;
            foreach (var pair in rows) target.Values[pair.Key] = pair.Value.Value;
            target.Touch();

            if (_data.Palettes.TryGetValue(selectedPalette, out var targetStops)) target.SetPalette(selectedPalette, targetStops);
            target.SetCharset(selectedCharset, SelectedCharsetRamp());

            _scene.ActiveLayerId = target.Id;
            LoadActiveLayerIntoEditor();
            RecordHistory(force: true);
            return target;
        }

        var actionBar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Theme.Bg,
            Margin = Padding.Empty,
            Padding = new Padding(0, 10, 0, 0)
        };
        actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
        actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));
        root.Controls.Add(actionBar, 0, 4);

        var copyActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Theme.Bg,
            Margin = Padding.Empty
        };
        var sceneActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Theme.Bg,
            Margin = Padding.Empty
        };
        var closeActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Theme.Bg,
            Margin = Padding.Empty
        };
        actionBar.Controls.Add(copyActions, 0, 0);
        actionBar.Controls.Add(sceneActions, 1, 0);
        actionBar.Controls.Add(closeActions, 2, 0);

        var copy = Btn(Localization.English ? "Copy ASCII" : "Copiar ASCII", (_, _) =>
        {
            try
            {
                string copied;
                if (AsciiTitlePrefabGenerator.IsGeneratedPrefab(SelectedPrefab()))
                {
                    copied = AsciiTitlePrefabGenerator.GenerateWithCharset(TitleText(), SelectedPrefab(), SelectedCharsetRamp(), SelectedLetterSpacing());
                }
                else
                {
                    RefreshPreview();
                    string frame = titlePreview.CaptureCurrentAsciiFrame();
                    if (frame.StartsWith("OpenGL ", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException(frame);
                    copied = CropAsciiFrame(frame);
                }
                Clipboard.SetText(copied);
                info.Text = Localization.English ? "ASCII title copied to clipboard." : "Título ASCII copiado al portapapeles.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(dialog, ex.Message, "Glyphoré", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        });
        copy.Width = 126;
        copy.Height = 34;
        copy.Margin = new Padding(0, 0, 8, 0);
        copyActions.Controls.Add(copy);

        var discordCopy = Btn(Localization.English ? "Copy for Discord" : "Copiar para Discord", (_, _) =>
        {
            try
            {
                string copied = AsciiTitlePrefabGenerator.IsGeneratedPrefab(SelectedPrefab())
                    ? AsciiTitlePrefabGenerator.GenerateWithCharset(TitleText(), SelectedPrefab(), SelectedCharsetRamp(), SelectedLetterSpacing())
                    : CropAsciiFrame(titlePreview.CaptureCurrentAsciiFrame());
                copied = AsciiTitlePrefabGenerator.ToDiscordSafeAscii(copied).TrimEnd();
                string payload = "```\n" + copied + "\n```";
                Clipboard.SetText(payload, TextDataFormat.UnicodeText);
                info.Text = payload.Length > 2000
                    ? (Localization.English ? "Copied as a Discord code block, but it exceeds Discord's 2000-character message limit." : "Copiado como bloque de código, pero supera el límite de 2000 caracteres de Discord.")
                    : (Localization.English ? "Discord-safe monospace code block copied." : "Bloque monoespaciado compatible con Discord copiado.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(dialog, ex.Message, "Glyphoré", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        });
        discordCopy.Width = 154;
        discordCopy.Height = 34;
        discordCopy.Margin = Padding.Empty;
        copyActions.Controls.Add(discordCopy);

        // Assigned before the callbacks are created so UpdateApplyState can be referenced
        // from either button without tripping C# definite-assignment analysis.
        Button apply = null!;

        var add = Btn(Localization.English ? "Add to scene" : "Añadir a escena", (_, _) =>
        {
            try
            {
                CommitToScene(applyToActive: false);
                info.Text = Localization.English ? "Title added to the scene." : "Título añadido a la escena.";
                UpdateApplyState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(dialog, ex.Message, "Glyphoré", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        });
        add.Width = 142;
        add.Height = 34;
        add.Margin = Padding.Empty;
        Theme.Button(add, accent: true);
        sceneActions.Controls.Add(add);

        apply = Btn(Localization.English ? "Apply to active" : "Aplicar a activa", (_, _) =>
        {
            try
            {
                CommitToScene(applyToActive: true);
                info.Text = Localization.English ? "Active title updated." : "Título activo actualizado.";
                UpdateApplyState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(dialog, ex.Message, "Glyphoré", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        });
        apply.Width = 142;
        apply.Height = 34;
        apply.Margin = new Padding(0, 0, 8, 0);
        sceneActions.Controls.Add(apply);

        var close = Btn(Localization.English ? "Close" : "Cerrar", (_, _) => dialog.Close());
        close.Width = 110;
        close.Height = 34;
        close.Margin = Padding.Empty;
        closeActions.Controls.Add(close);

        void UpdateApplyState()
        {
            bool hasActiveTitle = _scene.ActiveLayer is { Effect: "ASCII Title" };
            apply.Enabled = hasActiveTitle;
            studioTips.SetToolTip(apply, hasActiveTitle
                ? (Localization.English
                    ? "Updates the ASCII Title layer currently selected in the main scene."
                    : "Actualiza la capa ASCII Title seleccionada actualmente en la escena principal.")
                : (Localization.English
                    ? "Unavailable until an ASCII Title layer is selected in the main window."
                    : "No disponible hasta seleccionar una capa ASCII Title en la ventana principal."));
        }

        studioTips.SetToolTip(copy, Localization.English
            ? "Copies only the generated ASCII banner."
            : "Copia únicamente el banner ASCII generado.");
        studioTips.SetToolTip(discordCopy, Localization.English
            ? "Copies a monospace version prepared to paste as a Discord code block."
            : "Copia una versión monoespaciada preparada para pegarse como bloque de código en Discord.");
        studioTips.SetToolTip(add, Localization.English
            ? "Creates a new ASCII Title layer with the current Studio settings."
            : "Crea una nueva capa ASCII Title con la configuración actual del Studio.");
        studioTips.SetToolTip(close, Localization.English ? "Closes ASCII Title Studio." : "Cierra el Estudio de títulos ASCII.");
        UpdateApplyState();

        dialog.Activated += (_, _) =>
        {
            // The main window remains interactive while Studio is open. If the user selects
            // a mask there, make this preview (including a detached preview) edit that same mask.
            if (_scene.ActiveLayer is { Effect: "ASCII Title" } selectedTitle)
            {
                activeTitle = selectedTitle;
                previewLayer.Masks = selectedTitle.Masks;
                titlePreview.ActiveMaskId = selectedTitle.Masks.Any(mask => mask.Id == _activeMaskId) ? _activeMaskId : null;
                titlePreview.MaskSnapping = _maskSnapping.Checked;
                titlePreview.MaskRotationSnapping = _maskRotationSnapping.Checked;
            }
            else
            {
                titlePreview.ActiveMaskId = null;
            }
            UpdateApplyState();
            if (previewScene.Width != _scene.Width || previewScene.Height != _scene.Height || previewScene.Fps != Math.Min(60, Math.Max(15, _scene.Fps)))
            {
                previewScene.Width = _scene.Width;
                previewScene.Height = _scene.Height;
                previewScene.Fps = Math.Min(60, Math.Max(15, _scene.Fps));
                titlePreview.TargetFps = previewScene.Fps;
                RefreshPreview();
            }
        };
        dialog.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Escape) return;
            e.Handled = true;
            dialog.Close();
        };
        studioSplit.SplitterMoved += (_, _) =>
        {
            if (studioSplit.Panel1Collapsed || studioSplit.Height <= 0) return;
            _titleStudioPreviewHeight = studioSplit.SplitterDistance;
        };
        dialog.Shown += (_, _) =>
        {
            // SplitContainer validates minima against its *current* size, so apply them only
            // after the dialog has completed its first real layout.
            studioSplit.Panel1MinSize = 0;
            studioSplit.Panel2MinSize = 0;
            int maximum = studioSplit.Height - controlsPanelMin - studioSplit.SplitterWidth;
            if (maximum >= previewPanelMin)
            {
                studioSplit.SplitterDistance = Math.Clamp(_titleStudioPreviewHeight, previewPanelMin, maximum);
                studioSplit.Panel1MinSize = previewPanelMin;
                studioSplit.Panel2MinSize = controlsPanelMin;
            }
            EnsurePreviewFillsHost();
            if (animate.Checked) titlePreview.RestartAnimation();
        };
        dialog.FormClosing += (_, _) =>
        {
            GlyphoreWindow? current = detachedPreview;
            if (current is null || current.IsDisposed) return;
            GlyphoreWindow window = current;
            detachedPreview = null;
            closingDetachedPreview = true;
            try
            {
                if (!titlePreview.IsDisposed)
                {
                    titlePreview.Parent = previewHost;
                    titlePreview.Dock = DockStyle.Fill;
                }
                window.Close();
            }
            finally
            {
                closingDetachedPreview = false;
                if (!window.IsDisposed) window.Dispose();
            }
        };
        dialog.FormClosed += (_, _) =>
        {
            _titleStudioPresenceOpen = false;
            UpdateDiscordPresenceContext();
            studioTips.Dispose();
            if (rows.TryGetValue("title_mix_custom_palette", out ParameterRow? hiddenPaletteMixRow) && hiddenPaletteMixRow.Parent is null)
                hiddenPaletteMixRow.Dispose();
        };

        _titleStudioPresenceOpen = true;
        UpdateDiscordPresenceContext();
        _detachedWindows.ShowSingle("title-studio", dialog);
    }

    private static string CropAsciiFrame(string frame)
    {
        string[] lines = frame.Replace("\r", string.Empty).Split('\n');
        int first = 0;
        while (first < lines.Length && string.IsNullOrWhiteSpace(lines[first])) first++;
        int last = lines.Length - 1;
        while (last >= first && string.IsNullOrWhiteSpace(lines[last])) last--;
        if (first > last) return string.Empty;

        int left = int.MaxValue;
        int right = -1;
        for (int y = first; y <= last; y++)
        {
            string line = lines[y];
            for (int x = 0; x < line.Length; x++)
            {
                if (char.IsWhiteSpace(line[x])) continue;
                left = Math.Min(left, x);
                right = Math.Max(right, x);
            }
        }

        if (right < left) return string.Join('\n', lines[first..(last + 1)]);
        var output = new List<string>(last - first + 1);
        for (int y = first; y <= last; y++)
        {
            string line = lines[y];
            if (line.Length <= left) output.Add(string.Empty);
            else output.Add(line.Substring(left, Math.Min(right - left + 1, line.Length - left)).TrimEnd());
        }
        return string.Join('\n', output);
    }
}

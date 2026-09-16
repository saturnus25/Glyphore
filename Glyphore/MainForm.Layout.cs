namespace Glyphore;

internal sealed partial class MainForm
{
    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            BackColor = Theme.Bg,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        ContentPanel.Controls.Add(root);

        var header = new Panel { Dock = DockStyle.Fill, BackColor = Theme.PanelRaised, Padding = new Padding(14, 8, 16, 9) };
        var brand = new BrandMarkControl { Left = 14, Top = 9, Width = 44, Height = 44, Anchor = AnchorStyles.Left | AnchorStyles.Top };
        var brandName = new Label
        {
            Text = "Glyphoré",
            ForeColor = Theme.Text,
            Font = new Font("Segoe UI Semibold", 16f, FontStyle.Bold),
            AutoSize = true,
            Left = 68,
            Top = 9
        };
        var brandSubtitle = new Label
        {
            Text = "PROCEDURAL CHARACTER ART STUDIO",
            ForeColor = Theme.AccentText,
            Font = new Font("Segoe UI Semibold", 7.5f),
            AutoSize = true,
            Left = 69,
            Top = 40
        };
        int presetCount = _data.Presets.Values.Sum(group => group.Count);
        var techLabel = new Label
        {
            Text = $"{_data.Presets.Count} EFFECTS  ·  {presetCount} PRESETS",
            ForeColor = Theme.Muted,
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Top = 25
        };
        var settingsButton = Btn(Localization.Text("button.settings"), (_, _) => OpenPreferences());
        settingsButton.Name = "SettingsButton";
        settingsButton.Tag = "button.settings";
        settingsButton.SetBounds(0, 15, 92, 32);
        settingsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _tips.SetToolTip(settingsButton, Localization.English ? "Glyphoré settings" : "Ajustes de Glyphoré");

        var aboutButton = Btn("About", (_, _) =>
        {
            _detachedWindows.ShowSingle(
                "about",
                () => new AboutForm(_windowIcon, _detachedWindows));
        });
        aboutButton.Name = "AboutButton";
        aboutButton.SetBounds(0, 15, 92, 32);
        aboutButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _tips.SetToolTip(aboutButton, "About Glyphoré · Third-Party Licenses");

        void LayoutHeaderRight()
        {
            aboutButton.Left = Math.Max(0, header.ClientSize.Width - aboutButton.Width - 18);
            settingsButton.Left = Math.Max(0, aboutButton.Left - settingsButton.Width - 8);
            techLabel.Left = Math.Max(0, settingsButton.Left - techLabel.PreferredSize.Width - 18);
        }
        LayoutHeaderRight();
        header.SizeChanged += (_, _) => LayoutHeaderRight();
        var accentLine = new Panel { Dock = DockStyle.Bottom, Height = 2, BackColor = Theme.Accent };
        header.Controls.AddRange([brand, brandName, brandSubtitle, techLabel, settingsButton, aboutButton, accentLine]);
        root.Controls.Add(header, 0, 0);

        // Do not set SplitterDistance / Panel*MinSize before the control has been laid out.
        // A freshly-created SplitContainer still has its tiny default size, and WinForms can
        // reject a 440 + 500 px minimum layout before the form ever becomes visible.
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterWidth = 5,
            FixedPanel = FixedPanel.None,
            BackColor = Theme.Border
        };
        root.Controls.Add(split, 0, 1);

        _left.Dock = DockStyle.Top;
        _left.AutoScroll = false;
        _left.AutoSize = true;
        _left.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _left.WrapContents = false;
        _left.FlowDirection = FlowDirection.TopDown;
        _left.BackColor = Theme.Bg;
        _left.Padding = new Padding(14, 12, 12, 14);

        var leftScroll = new ThemedScrollPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Bg,
            Margin = Padding.Empty
        };
        leftScroll.SetContent(_left);
        split.Panel1.BackColor = Theme.Bg;
        split.Panel1.Controls.Add(leftScroll);

        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            BackColor = Theme.Bg,
            Padding = new Padding(10)
        };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        split.Panel2.Controls.Add(right);

        _status.Dock = DockStyle.Fill;
        _status.TextAlign = ContentAlignment.MiddleRight;
        _status.ForeColor = Theme.AccentText;
        _status.BackColor = Theme.PanelRaised;
        _status.Padding = new Padding(10, 0, 10, 0);
        _status.Text = Localization.Text("status.init");
        right.Controls.Add(_status, 0, 0);

        var previewTools = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Theme.Panel,
            Padding = new Padding(4, 4, 4, 2),
            Margin = Padding.Empty
        };
        previewTools.Controls.Add(new Label
        {
            Text = Localization.English ? "Preview Background:" : "Fondo preview:",
            ForeColor = Theme.Muted,
            AutoSize = true,
            Margin = new Padding(2, 7, 6, 0)
        });
        Button PreviewBgButton(string text, Action action)
        {
            var button = Btn(text, (_, _) => action());
            button.AutoSize = true;
            button.Height = 28;
            button.Margin = new Padding(0, 1, 5, 0);
            return button;
        }
        previewTools.Controls.Add(PreviewBgButton(Localization.English ? "Dark" : "Oscuro", () =>
            SetEditorPreviewBackground(Color.Black, PreviewBackgroundMode.Solid)));
        previewTools.Controls.Add(PreviewBgButton(Localization.English ? "Light" : "Claro", () =>
            SetEditorPreviewBackground(Color.White, PreviewBackgroundMode.Solid)));
        previewTools.Controls.Add(PreviewBgButton("Checkerboard", () =>
            SetEditorPreviewBackground(_preview.PreviewBackgroundColor, PreviewBackgroundMode.Checkerboard)));
        previewTools.Controls.Add(PreviewBgButton(Localization.English ? "Custom…" : "Personalizado…", () =>
        {
            using var picker = new ColorDialog { Color = _preview.PreviewBackgroundColor, FullOpen = true };
            if (picker.ShowDialog(this) != DialogResult.OK) return;
            SetEditorPreviewBackground(picker.Color, PreviewBackgroundMode.Solid);
        }));

        previewTools.Controls.Add(new Label
        {
            Text = "Zoom",
            ForeColor = Theme.Muted,
            AutoSize = true,
            Margin = new Padding(10, 7, 4, 0)
        });
        var mainPreviewZoom = new GlyphSlider
        {
            Width = 128,
            Height = 28,
            Minimum = 10,
            Maximum = 400,
            Value = 100,
            MouseWheelAdjustsValue = false,
            Margin = new Padding(0, 0, 5, 0)
        };
        var mainPreviewZoomValue = new Label
        {
            Text = "100%",
            ForeColor = Theme.AccentText,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Width = 48,
            Height = 28,
            Margin = new Padding(0, 0, 5, 0)
        };
        mainPreviewZoom.ValueChanged += (_, _) =>
        {
            _preview.PreviewZoom = mainPreviewZoom.Value / 100.0;
            mainPreviewZoomValue.Text = $"{mainPreviewZoom.Value}%";
        };
        _tips.SetToolTip(mainPreviewZoom, Localization.English
            ? "Editor-only preview zoom. Below 100% pulls the complete scene farther away; above 100% zooms in. It never changes scene scale or export."
            : "Zoom exclusivo de la preview. Por debajo de 100% aleja la escena completa; por encima acerca. No cambia la escala de la escena ni la exportación.");
        previewTools.Controls.Add(mainPreviewZoom);
        previewTools.Controls.Add(mainPreviewZoomValue);
        right.Controls.Add(previewTools, 0, 1);

        var previewHost = new Panel { Dock = DockStyle.Fill, BackColor = Theme.BorderHot, Padding = new Padding(1) };
        _preview.Dock = DockStyle.Fill;
        previewHost.Controls.Add(_preview);
        right.Controls.Add(previewHost, 0, 2);

        _importView.Dock = DockStyle.Fill;
        _importView.Visible = false;
        _importView.ReadOnly = true;
        _importView.WordWrap = false;
        _importView.BackColor = Color.Black;
        _importView.ForeColor = Theme.Text;
        _importView.Font = new Font("Cascadia Mono", 10);
        _importView.BorderStyle = BorderStyle.None;
        previewHost.Controls.Add(_importView);
        _importView.BringToFront();
        _importTimer.Tick += (_, _) => UpdateImportedFrame();
        _importTimer.Start();

        var languageGroup = Group(Localization.Text("group.language"), 62);
        languageGroup.Tag = "group.language";
        SetupCombo(_language);
        _language.Items.AddRange(["Español", "English"]);
        _language.SelectedIndex = 0;
        _language.SetBounds(10, 25, 395, 28);
        languageGroup.Controls.Add(_language);
        _left.Controls.Add(languageGroup);

        _left.Controls.Add(BuildSceneGroup());

        var effectGroup = Group(Localization.Text("group.effect"), 161);
        effectGroup.Tag = "group.effect";
        SetupCombo(_effect);
        SetupCombo(_preset);
        _effect.SetBounds(10, 25, 395, 28);
        _preset.SetBounds(10, 62, 395, 28);
        effectGroup.Controls.AddRange([_effect, _preset]);

        var randomButton = Btn(Localization.Text("button.random"), (_, _) =>
        {
            _seed.Value = Random.Shared.Next(0, int.MaxValue);
        });
        randomButton.Tag = "button.random";
        randomButton.SetBounds(10, 95, 92, 26);
        effectGroup.Controls.Add(randomButton);

        var restartButton = Btn(Localization.Text("button.restart"), (_, _) =>
        {
            NotifyDiscordPreviewing();
            _preview.RestartAnimation();
            if (_importView.Visible)
            {
                _importClock.Restart();
                _importIndex = -1;
            }
        });
        restartButton.Tag = "button.restart";
        restartButton.SetBounds(107, 95, 92, 26);
        effectGroup.Controls.Add(restartButton);

        _pauseButton.Text = Localization.Text("button.pause");
        Theme.Button(_pauseButton);
        _pauseButton.Tag = "button.pause";
        _pauseButton.SetBounds(204, 95, 92, 26);
        _pauseButton.Click += (_, _) => TogglePause();
        effectGroup.Controls.Add(_pauseButton);

        var benchmarkButton = Btn(Localization.Text("button.benchmark"), (_, _) => Benchmark());
        benchmarkButton.Tag = "button.benchmark";
        benchmarkButton.SetBounds(301, 95, 104, 26);
        effectGroup.Controls.Add(benchmarkButton);

        var titleStudioButton = Btn(Localization.Text("button.titlestudio"), (_, _) => OpenTitleStudio());
        titleStudioButton.Tag = "button.titlestudio";
        titleStudioButton.SetBounds(10, 128, 395, 28);
        effectGroup.Controls.Add(titleStudioButton);
        _left.Controls.Add(effectGroup);

        var outputGroup = Group(Localization.Text("group.output"), 244);
        outputGroup.Tag = "group.output";
        SetupNumeric(_width, 1, int.MaxValue, 178);
        SetupNumeric(_height, 1, int.MaxValue, 50);
        SetupNumeric(_fps, 1, int.MaxValue, 30);
        SetupNumeric(_duration, 0.001m, decimal.MaxValue, 6, 3);
        SetupNumeric(_seed, 0, int.MaxValue, 1337);
        AddNumericRow(outputGroup, "label.width", _width, 25);
        AddNumericRow(outputGroup, "label.height", _height, 53);
        AddNumericRow(outputGroup, "FPS", _fps, 81);
        AddNumericRow(outputGroup, "label.duration", _duration, 109);
        AddNumericRow(outputGroup, "Seed", _seed, 137);

        _exportCredit.Text = Localization.Text("check.credit");
        _exportCredit.Checked = true;
        Theme.CheckBox(_exportCredit);
        _exportCredit.Tag = "check.credit";
        _exportCredit.SetBounds(10, 166, 395, 24);
        outputGroup.Controls.Add(_exportCredit);

        _exportButton.Text = Localization.Text("button.export") + "  (Ctrl+E)";
        Theme.Button(_exportButton, accent: true);
        _exportButton.Tag = "button.export";
        _exportButton.Font = new Font(Font, FontStyle.Bold);
        _exportButton.SetBounds(10, 198, 395, 34);
        _exportButton.Click += (_, _) => _ = ExportDialogAsync();
        outputGroup.Controls.Add(_exportButton);
        _left.Controls.Add(outputGroup);

        var generalGroup = Group(Localization.Text("group.general"), ParameterCatalog.General.Length * 34 + 30);
        generalGroup.Tag = "group.general";
        _generalParams.SetBounds(10, 23, 395, generalGroup.Height - 28);
        _generalParams.BackColor = Theme.Panel;
        generalGroup.Controls.Add(_generalParams);
        _left.Controls.Add(generalGroup);
        foreach (var description in ParameterCatalog.General) AddParamRow(_generalParams, description);

        var specificGroup = Group(Localization.Text("group.specific"), 80);
        specificGroup.Tag = "group.specific";
        specificGroup.Name = "specificGroup";
        _specificParams.SetBounds(10, 23, 395, 52);
        _specificParams.BackColor = Theme.Panel;
        specificGroup.Controls.Add(_specificParams);
        _left.Controls.Add(specificGroup);

        var charsetGroup = Group(Localization.Text("group.charset"), 210);
        charsetGroup.Tag = "group.charset";
        SetupCombo(_charsetPreset);
        _charsetPreset.SetBounds(10, 25, 395, 28);
        Theme.TextBox(_charset);
        _charset.SetBounds(10, 60, 395, 28);
        SetupNumeric(_glyphSize, 70, 400, 190);
        AddNumericRow(charsetGroup, "label.glyphsize", _glyphSize, 94);
        _invert.Text = Localization.Text("check.invert");
        Theme.CheckBox(_invert);
        _invert.SetBounds(10, 126, 170, 26);
        _invert.Tag = "check.invert";
        charsetGroup.Controls.AddRange([_charsetPreset, _charset, _invert]);

        var copyButton = Btn(Localization.Text("button.copy"), (_, _) => CopyFrame());
        copyButton.Tag = "button.copy";
        copyButton.SetBounds(270, 126, 135, 27);
        charsetGroup.Controls.Add(copyButton);

        var applyCharsetSelectedButton = Btn(Localization.Text("button.charsetselected"), (_, _) =>
        {
            ApplyCurrentCharsetToSelectedLayers();
            PushSceneOnly();
        });
        applyCharsetSelectedButton.Tag = "button.charsetselected";
        applyCharsetSelectedButton.SetBounds(10, 164, 194, 28);
        var applyCharsetAllButton = Btn(Localization.Text("button.charsetall"), (_, _) =>
        {
            ApplyCurrentCharsetToAllLayers();
            PushSceneOnly();
        });
        applyCharsetAllButton.Tag = "button.charsetall";
        applyCharsetAllButton.SetBounds(211, 164, 194, 28);
        charsetGroup.Controls.AddRange([applyCharsetSelectedButton, applyCharsetAllButton]);
        _left.Controls.Add(charsetGroup);

        var colorGroup = Group(Localization.Text("group.color"), 232);
        colorGroup.Tag = "group.color";
        SetupCombo(_palette);
        _palette.SetBounds(10, 25, 395, 28);
        _color.Text = Localization.Text("check.color");
        _color.Checked = true;
        Theme.CheckBox(_color);
        _color.Tag = "check.color";
        _color.SetBounds(10, 58, 210, 26);
        _paletteStops.SetBounds(10, 89, 395, 62);
        _paletteStops.BackColor = Theme.Panel;
        _paletteStops.AutoScroll = false;
        colorGroup.Controls.AddRange([_palette, _color, _paletteStops]);

        var addColorButton = Btn(Localization.Text("button.addcolor"), (_, _) => AddPaletteStop());
        addColorButton.Tag = "button.addcolor";
        addColorButton.SetBounds(10, 158, 105, 28);
        var importButton = Btn(Localization.Text("button.import"), (_, _) => ImportPs());
        importButton.Tag = "button.import";
        importButton.SetBounds(121, 158, 135, 28);
        var saveFrameButton = Btn(Localization.Text("button.save"), (_, _) => SaveFrame());
        saveFrameButton.Tag = "button.save";
        saveFrameButton.SetBounds(262, 158, 143, 28);
        colorGroup.Controls.AddRange([addColorButton, importButton, saveFrameButton]);

        var applyPaletteSelectedButton = Btn(Localization.Text("button.paletteselected"), (_, _) => ApplyCurrentPaletteToSelectedLayers());
        applyPaletteSelectedButton.Tag = "button.paletteselected";
        applyPaletteSelectedButton.SetBounds(10, 192, 194, 28);
        var applyPaletteAllButton = Btn(Localization.Text("button.paletteall"), (_, _) => ApplyCurrentPaletteToAllLayers());
        applyPaletteAllButton.Tag = "button.paletteall";
        applyPaletteAllButton.SetBounds(211, 192, 194, 28);
        colorGroup.Controls.AddRange([applyPaletteSelectedButton, applyPaletteAllButton]);
        _left.Controls.Add(colorGroup);

        _left.Controls.Add(BuildGlobalTransformGroup());
        _left.Controls.Add(BuildPostProcessGroup());

        var shapeGroup = Group(Localization.Text("group.shape"), 70);
        shapeGroup.Tag = "group.shape";
        SetupCombo(_shape);
        _shape.Items.AddRange(["Square", "Diamond", "Star", "Hex", "Cross"]);
        _shape.SelectedIndex = 0;
        _shape.SetBounds(10, 28, 395, 28);
        shapeGroup.Controls.Add(_shape);
        _left.Controls.Add(shapeGroup);

        _tips.SetToolTip(randomButton, Localization.Tip("button.random"));
        _tips.SetToolTip(restartButton, Localization.Tip("button.restart"));
        _tips.SetToolTip(_pauseButton, Localization.Tip("button.pause"));
        _tips.SetToolTip(benchmarkButton, Localization.Tip("button.benchmark"));
        _tips.SetToolTip(addColorButton, Localization.Tip("button.addcolor"));
        _tips.SetToolTip(importButton, Localization.Tip("button.import"));
        _tips.SetToolTip(saveFrameButton, Localization.Tip("button.save"));

        WireEvents();
        ApplyStaticTips();
        _left.ClientSizeChanged += (_, _) => LayoutSidebar();
        split.SplitterMoved += (_, _) => LayoutSidebar();

        Shown += (_, _) =>
        {
            ConfigureMainSplit(split);
            LayoutSidebar();
        };

        LayoutSidebar();
        KeyPreview = true;
        KeyDown += HandleShortcutKeyDown;
    }

    private static void ConfigureMainSplit(SplitContainer split)
    {
        // At Shown time the SplitContainer finally has its real client width. Set the desired
        // distance first while the default minimums are still active, then apply our minimums.
        int width = split.ClientSize.Width;
        if (width <= 0) return;

        const int desiredLeft = 470;
        const int desiredLeftMinimum = 440;
        const int desiredRightMinimum = 500;

        int maxDistance = Math.Max(0, width - split.SplitterWidth - desiredRightMinimum);
        int distance = Math.Clamp(desiredLeft, 0, maxDistance);

        // If Windows/DPI scaling ever leaves less room than expected, degrade gracefully instead
        // of throwing during startup. The Form MinimumSize normally keeps us above this path.
        int leftMinimum = Math.Min(desiredLeftMinimum, Math.Max(0, distance));
        int rightMinimum = Math.Min(desiredRightMinimum, Math.Max(0, width - split.SplitterWidth - distance));

        split.SplitterDistance = distance;
        split.Panel1MinSize = leftMinimum;
        split.Panel2MinSize = rightMinimum;
    }

    private void LayoutSidebar()
    {
        if (_left.IsDisposed || _left.ClientSize.Width <= 0) return;

        // The themed scrollbar lives outside the sidebar content viewport, so the flow panel's
        // ClientSize already represents the exact width available to its controls.
        int available = Math.Max(80, _left.ClientSize.Width - _left.Padding.Horizontal);

        foreach (Control control in _left.Controls)
        {
            if (control is ThemedGroupBox group) group.Width = available;
        }

        int content = Math.Max(60, available - 20);
        foreach (var combo in new[] { _language, _effect, _preset, _charsetPreset, _palette, _shape }) combo.Width = content;
        _layerListHost.Width = content;
        if (ReferenceEquals(_layers.Parent, _layerListHost)) _layers.Width = content;
        _charset.Width = content;
        _exportCredit.Width = content;
        _exportButton.Width = content;
        _generalParams.Width = content;
        _specificParams.Width = content;
        _paletteStops.Width = content;

        foreach (ParameterRow row in _generalParams.Controls.OfType<ParameterRow>()) row.Width = content;
        foreach (ParameterRow row in _specificParams.Controls.OfType<ParameterRow>()) row.Width = content;
        foreach (Button button in _specificParams.Controls.OfType<Button>()) button.Width = content;

        if (_left.Controls.Cast<Control>().FirstOrDefault(c => Equals(c.Tag, "group.scene")) is Control sceneGroup)
        {
            _layerListHost.Width = content;
            if (ReferenceEquals(_layers.Parent, _layerListHost)) _layers.Width = content;
            _layerBlend.Left = Math.Max(264, available - 151);
            _layerBlend.Width = Math.Max(105, available - _layerBlend.Left - 10);

            var rowButtons = sceneGroup.Controls.OfType<Button>().Where(button => button.Top >= 150 && button.Top < 185).OrderBy(button => button.Left).ToArray();
            int gap = 5;
            int buttonWidth = Math.Max(52, (content - gap * Math.Max(0, rowButtons.Length - 1)) / Math.Max(1, rowButtons.Length));
            for (int i = 0; i < rowButtons.Length; i++)
            {
                rowButtons[i].Left = 10 + i * (buttonWidth + gap);
                rowButtons[i].Width = buttonWidth;
            }

            var selectionButtons = sceneGroup.Controls.OfType<Button>().Where(button => button.Top >= 185 && button.Top < 220).OrderBy(button => button.Left).ToArray();
            int selectionGap = 5;
            int selectionWidth = Math.Max(90, (content - selectionGap * Math.Max(0, selectionButtons.Length - 1)) / Math.Max(1, selectionButtons.Length));
            for (int i = 0; i < selectionButtons.Length; i++)
            {
                selectionButtons[i].Left = 10 + i * (selectionWidth + selectionGap);
                selectionButtons[i].Width = selectionWidth;
            }

            _respectLayerOrder.Width = content;

            var historyButtons = sceneGroup.Controls.OfType<Button>().Where(button => button.Top >= 245 && button.Top < 287).OrderBy(button => button.Left).ToArray();
            int historyWidth = Math.Max(120, (content - 7) / 2);
            for (int i = 0; i < historyButtons.Length; i++)
            {
                historyButtons[i].Left = 10 + i * (historyWidth + 7);
                historyButtons[i].Width = historyWidth;
            }

            var fileButtons = sceneGroup.Controls.OfType<Button>()
                .Where(button => Equals(button.Tag, "button.sceneopen") || Equals(button.Tag, "button.scenesave"))
                .OrderBy(button => button.Left)
                .ToArray();
            int fileWidth = Math.Max(120, (content - 7) / 2);
            for (int i = 0; i < fileButtons.Length; i++)
            {
                fileButtons[i].Left = 10 + i * (fileWidth + 7);
                fileButtons[i].Width = fileWidth;
            }

            _sceneBackground.Left = 10;
            _sceneBackground.Width = content;
            _previewFollowsSceneBackground.Left = 10;
            _previewFollowsSceneBackground.Width = content;

            var maskButtons = sceneGroup.Controls.OfType<Button>()
                .Where(button => Equals(button.Tag, "button.maskadd") || Equals(button.Tag, "button.maskremove"))
                .OrderBy(button => button.Left)
                .ToArray();
            int maskButtonWidth = Math.Max(120, (content - 7) / 2);
            for (int i = 0; i < maskButtons.Length; i++)
            {
                maskButtons[i].Left = 10 + i * (maskButtonWidth + 7);
                maskButtons[i].Width = maskButtonWidth;
            }

            var maskFields = sceneGroup.Controls.OfType<MaskSliderField>()
                .OrderBy(field => field.Top)
                .ThenBy(field => field.Left)
                .ToArray();
            int maskFieldGap = 7;
            int maskFieldWidth = Math.Max(120, (content - maskFieldGap) / 2);
            for (int i = 0; i < maskFields.Length; i++)
            {
                maskFields[i].Left = 10 + (i % 2) * (maskFieldWidth + maskFieldGap);
                maskFields[i].Width = maskFieldWidth;
            }
        }

        if (_left.Controls.Cast<Control>().FirstOrDefault(c => Equals(c.Tag, "group.effect")) is Control effectGroup)
        {
            var actionButtons = effectGroup.Controls.OfType<Button>().Where(b => b.Top >= 90 && b.Top < 125).OrderBy(b => b.Left).ToArray();
            int gap = 5;
            int buttonWidth = Math.Max(70, (content - gap * Math.Max(0, actionButtons.Length - 1)) / Math.Max(1, actionButtons.Length));
            for (int i = 0; i < actionButtons.Length; i++)
            {
                actionButtons[i].Left = 10 + i * (buttonWidth + gap);
                actionButtons[i].Width = buttonWidth;
            }

            var titleButton = effectGroup.Controls.OfType<Button>().FirstOrDefault(b => Equals(b.Tag, "button.titlestudio"));
            if (titleButton is not null)
            {
                titleButton.Left = 10;
                titleButton.Width = content;
            }
        }

        if (_left.Controls.Cast<Control>().FirstOrDefault(c => Equals(c.Tag, "group.output")) is Control outputGroup)
        {
            foreach (NumericUpDown numeric in outputGroup.Controls.OfType<NumericUpDown>())
            {
                numeric.Left = Math.Max(190, available - 135);
                numeric.Width = 125;
            }
            foreach (Label label in outputGroup.Controls.OfType<Label>()) label.Width = Math.Max(120, available - 175);
        }

        if (_left.Controls.Cast<Control>().FirstOrDefault(c => Equals(c.Tag, "group.charset")) is Control charsetGroup)
        {
            foreach (NumericUpDown numeric in charsetGroup.Controls.OfType<NumericUpDown>())
            {
                numeric.Left = Math.Max(190, available - 135);
                numeric.Width = 125;
            }
            foreach (Label label in charsetGroup.Controls.OfType<Label>()) label.Width = Math.Max(120, available - 175);

            var copy = charsetGroup.Controls.OfType<Button>().FirstOrDefault(b => Equals(b.Tag, "button.copy"));
            if (copy is not null)
            {
                copy.Width = Math.Min(135, Math.Max(100, content / 3));
                copy.Left = available - copy.Width - 10;
                _invert.Width = Math.Max(120, copy.Left - 20);
            }


            var charsetButtons = charsetGroup.Controls.OfType<Button>().Where(b => b.Top >= 160).OrderBy(b => b.Left).ToArray();
            int charsetGap = 7;
            int charsetWidth = Math.Max(120, (content - charsetGap) / 2);
            for (int i = 0; i < charsetButtons.Length; i++)
            {
                charsetButtons[i].Left = 10 + i * (charsetWidth + charsetGap);
                charsetButtons[i].Width = charsetWidth;
            }
        }

        if (_left.Controls.Cast<Control>().FirstOrDefault(c => Equals(c.Tag, "group.color")) is Control colorGroup)
        {
            var actionButtons = colorGroup.Controls.OfType<Button>().Where(b => b.Top >= 150 && b.Top < 190).OrderBy(b => b.Left).ToArray();
            int gap = 6;
            int actionWidth = Math.Max(84, (content - gap * Math.Max(0, actionButtons.Length - 1)) / Math.Max(1, actionButtons.Length));
            for (int i = 0; i < actionButtons.Length; i++)
            {
                actionButtons[i].Left = 10 + i * (actionWidth + gap);
                actionButtons[i].Width = actionWidth;
            }

            var paletteButtons = colorGroup.Controls.OfType<Button>().Where(b => b.Top >= 190).OrderBy(b => b.Left).ToArray();
            int paletteWidth = Math.Max(120, (content - 7) / 2);
            for (int i = 0; i < paletteButtons.Length; i++)
            {
                paletteButtons[i].Left = 10 + i * (paletteWidth + 7);
                paletteButtons[i].Width = paletteWidth;
            }
        }

    }
}

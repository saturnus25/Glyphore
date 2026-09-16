namespace Glyphore;

internal sealed partial class MainForm
{
    private ThemedGroupBox BuildSceneGroup()
    {
        var group = Group(Localization.Text("group.scene"), 1025);
        group.Tag = "group.scene";
        group.Name = "sceneGroup";

        _layerListHost.SetBounds(10, 25, 395, 88);
        _layerListHost.BackColor = Theme.Input;
        _layerListHost.Padding = Padding.Empty;
        group.Controls.Add(_layerListHost);

        _layers.Dock = DockStyle.Fill;
        _layers.BackColor = Theme.Input;
        _layers.ForeColor = Theme.Text;
        _layers.BorderStyle = BorderStyle.FixedSingle;
        _layers.IntegralHeight = false;
        _layers.SelectionMode = SelectionMode.MultiExtended;
        _layers.DrawMode = DrawMode.OwnerDrawFixed;
        _layers.ItemHeight = 22;
        _layers.DrawItem += DrawSceneLayerItem;
        _layers.SelectedIndexChanged += (_, _) => HandleSceneListSelection();
        _layers.MouseDoubleClick += (_, e) =>
        {
            int index = _layers.IndexFromPoint(e.Location);
            if (index < 0 || _layers.Items[index] is not SceneEffectLayer layer) return;
            ActivateLayer(layer.Id);
            RenameActiveLayer();
        };

        _layerDetachedPlaceholder.Dock = DockStyle.Fill;
        _layerDetachedPlaceholder.BackColor = Theme.PanelRaised;
        _layerDetachedPlaceholder.Visible = false;
        var detachedLayersLabel = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Theme.Muted,
            Text = Localization.English
                ? "Layers are detached into their own window."
                : "Las capas están desacopladas en su propia ventana."
        };
        _layerDetachedPlaceholder.Controls.Add(detachedLayersLabel);
        _layerListHost.Controls.Add(_layerDetachedPlaceholder);
        _layerListHost.Controls.Add(_layers);
        _layers.BringToFront();

        _layerVisible.Text = Localization.Text("check.layervisible");
        _layerVisible.Tag = "check.layervisible";
        Theme.CheckBox(_layerVisible);
        _layerVisible.SetBounds(10, 120, 95, 25);
        _layerVisible.CheckedChanged += (_, _) =>
        {
            if (_applying || !_sceneReady) return;
            foreach (var layer in SelectedLayers()) layer.Visible = _layerVisible.Checked;
            RefreshLayerList();
            PushSceneOnly();
        };
        group.Controls.Add(_layerVisible);

        var opacityLabel = new Label
        {
            Text = Localization.Text("label.opacity"),
            Tag = "label.opacity",
            Left = 112,
            Top = 123,
            AutoSize = true,
            Height = 22,
            ForeColor = Theme.Text
        };
        group.Controls.Add(opacityLabel);

        SetupNumeric(_layerOpacity, 0, 100, 100);
        _layerOpacity.SetBounds(181, 119, 65, 25);
        _layerOpacity.ValueChanged += (_, _) =>
        {
            if (_applying || !_sceneReady) return;
            double opacity = (double)_layerOpacity.Value / 100.0;
            foreach (var layer in SelectedLayers()) layer.Opacity = opacity;
            PushSceneOnly();
        };
        group.Controls.Add(_layerOpacity);

        SetupCombo(_layerBlend);
        _layerBlend.Items.AddRange(Localization.English ? new object[] { "Normal", "Additive" } : new object[] { "Normal", "Aditivo" });
        _layerBlend.SetBounds(253, 119, 152, 28);
        _layerBlend.SelectedIndexChanged += (_, _) =>
        {
            if (_applying || !_sceneReady) return;
            var mode = _layerBlend.SelectedIndex == 1 ? LayerBlendMode.Additive : LayerBlendMode.Normal;
            foreach (var layer in SelectedLayers()) layer.BlendMode = mode;
            PushSceneOnly();
        };
        group.Controls.Add(_layerBlend);

        var add = Btn(Localization.Text("button.layeradd"), (_, _) => AddLayer());
        add.Tag = "button.layeradd";
        var duplicate = Btn(Localization.Text("button.layerduplicate"), (_, _) => DuplicateLayer());
        duplicate.Tag = "button.layerduplicate";
        var remove = Btn(Localization.Text("button.layerremove"), (_, _) => RemoveLayer());
        remove.Tag = "button.layerremove";
        var up = Btn("↑", (_, _) => MoveLayer(-1));
        var down = Btn("↓", (_, _) => MoveLayer(1));

        add.SetBounds(10, 153, 76, 27);
        duplicate.SetBounds(91, 153, 100, 27);
        remove.SetBounds(196, 153, 76, 27);
        up.SetBounds(277, 153, 61, 27);
        down.SetBounds(343, 153, 62, 27);
        group.Controls.AddRange([add, duplicate, remove, up, down]);

        var selectAll = Btn(Localization.Text("button.layerselectall"), (_, _) => SelectAllLayers());
        selectAll.Tag = "button.layerselectall";
        var clearSelection = Btn(Localization.Text("button.layerclear"), (_, _) => ClearLayerSelection());
        clearSelection.Tag = "button.layerclear";
        var rename = Btn(Localization.Text("button.layerrename"), (_, _) => RenameActiveLayer());
        rename.Tag = "button.layerrename";
        _layerDetachButton.Text = Localization.English ? "↗ Detach" : "↗ Desacoplar";
        _layerDetachButton.Tag = "button.layerdetach";
        Theme.Button(_layerDetachButton);
        _layerDetachButton.Click += (_, _) => ToggleDetachedLayers();
        selectAll.SetBounds(10, 187, 95, 27);
        clearSelection.SetBounds(110, 187, 95, 27);
        rename.SetBounds(210, 187, 95, 27);
        _layerDetachButton.SetBounds(310, 187, 95, 27);
        group.Controls.AddRange([selectAll, clearSelection, rename, _layerDetachButton]);

        _respectLayerOrder.Text = Localization.Text("check.respectlayerorder");
        _respectLayerOrder.Tag = "check.respectlayerorder";
        Theme.CheckBox(_respectLayerOrder);
        _respectLayerOrder.Checked = true;
        _respectLayerOrder.SetBounds(10, 221, 395, 25);
        _respectLayerOrder.CheckedChanged += (_, _) =>
        {
            if (_applying || !_sceneReady) return;
            _scene.RespectLayerOrder = _respectLayerOrder.Checked;
            PushSceneOnly();
        };
        group.Controls.Add(_respectLayerOrder);

        _undoButton.Text = Localization.Text("button.undo") + "  Ctrl+Z";
        _undoButton.Tag = "button.undo";
        Theme.Button(_undoButton);
        _undoButton.SetBounds(10, 253, 194, 28);
        _undoButton.Click += (_, _) => UndoScene();
        group.Controls.Add(_undoButton);

        _redoButton.Text = Localization.Text("button.redo") + "  Ctrl+Y";
        _redoButton.Tag = "button.redo";
        Theme.Button(_redoButton);
        _redoButton.SetBounds(211, 253, 194, 28);
        _redoButton.Click += (_, _) => RedoScene();
        group.Controls.Add(_redoButton);

        var open = Btn(Localization.Text("button.sceneopen"), (_, _) => LoadSceneDialog());
        open.Tag = "button.sceneopen";
        var save = Btn(Localization.Text("button.scenesave"), (_, _) => SaveScene());
        save.Tag = "button.scenesave";
        open.SetBounds(10, 289, 194, 28);
        save.SetBounds(211, 289, 194, 28);
        group.Controls.AddRange([open, save]);

        Theme.Button(_sceneBackground);
        _sceneBackground.SetBounds(10, 326, 395, 28);
        _sceneBackground.Click += (_, _) =>
        {
            if (!_sceneReady) return;
            Color current;
            try { current = ColorTranslator.FromHtml(_scene.BackgroundColor); }
            catch { current = Color.Black; }
            using var picker = new ColorDialog { Color = current, FullOpen = true };
            if (picker.ShowDialog(this) != DialogResult.OK) return;
            _scene.BackgroundColor = $"#{picker.Color.R:X2}{picker.Color.G:X2}{picker.Color.B:X2}";
            SyncSceneBackgroundControl();
            PushSceneOnly();
        };
        group.Controls.Add(_sceneBackground);

        _previewFollowsSceneBackground.Text = Localization.Text("check.previewscenebackground");
        _previewFollowsSceneBackground.Tag = "check.previewscenebackground";
        Theme.CheckBox(_previewFollowsSceneBackground);
        _previewFollowsSceneBackground.SetBounds(10, 360, 395, 25);
        _previewFollowsSceneBackground.CheckedChanged += (_, _) =>
        {
            if (_applying) return;
            SyncPreviewBackgroundFromScene();
        };
        group.Controls.Add(_previewFollowsSceneBackground);
        _tips.SetToolTip(_previewFollowsSceneBackground, Localization.English
            ? "When enabled, the editor preview uses the scene background color. Preview-only presets automatically turn this off."
            : "Al activarlo, la preview del editor usa el color de fondo de la escena. Los fondos exclusivos de preview lo desactivan automáticamente.");

        BuildMaskControls(group, 392);

        _tips.SetToolTip(_layers, Localization.English
            ? "Select one or more procedural layers. The first item is the top layer. Ctrl/Shift selection is supported."
            : "Selecciona una o varias capas procedurales. La primera es la capa superior. Puedes usar Ctrl/Shift.");
        _tips.SetToolTip(_layerDetachButton, Localization.English
            ? "Moves the layer tree into an independent modeless window. It remains the same live list and selection state."
            : "Mueve el árbol de capas a una ventana independiente y no modal. Sigue siendo la misma lista y el mismo estado de selección.");
        _tips.SetToolTip(_layerVisible, Localization.English ? "Shows or hides the selected layer(s)." : "Muestra u oculta las capas seleccionadas.");
        _tips.SetToolTip(_layerOpacity, Localization.English ? "Changes opacity for the selected layer(s)." : "Cambia la opacidad de las capas seleccionadas.");
        _tips.SetToolTip(_layerBlend, Localization.English ? "Changes blend mode for the selected layer(s)." : "Cambia el modo de mezcla de las capas seleccionadas.");
        _tips.SetToolTip(_respectLayerOrder, Localization.Tip("check.respectlayerorder"));

        return group;
    }


    private void ToggleDetachedLayers()
    {
        if (_detachedLayersWindow is { IsDisposed: false } current)
        {
            current.Close();
            return;
        }

        var window = new GlyphoreWindow
        {
            Text = Localization.English ? "Scene Layers · Glyphoré" : "Capas de escena · Glyphoré",
            StartPosition = FormStartPosition.Manual,
            MinimumSize = new Size(420, 300),
            ClientSize = new Size(560, 520),
            BackColor = Theme.Bg,
            ForeColor = Theme.Text,
            ShowInTaskbar = true,
            ShowIcon = true,
            MinimizeBox = true,
            MaximizeBox = true,
            Resizable = true
        };
        if (Icon is not null) window.Icon = Icon;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Theme.Bg,
            Padding = new Padding(10),
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        var detachedHost = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Input, Margin = Padding.Empty };
        var tools = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoScroll = true,
            BackColor = Theme.Bg,
            Padding = new Padding(0, 7, 0, 0),
            Margin = Padding.Empty
        };
        Button Tool(string text, Action action, int width = 92)
        {
            var button = Btn(text, (_, _) => action());
            button.Width = width;
            button.Height = 29;
            button.Margin = new Padding(0, 0, 5, 3);
            tools.Controls.Add(button);
            return button;
        }
        Tool(Localization.Text("button.layeradd"), AddLayer);
        Tool(Localization.Text("button.layerduplicate"), DuplicateLayer, 108);
        Tool(Localization.Text("button.layerremove"), RemoveLayer);
        Tool("↑", () => MoveLayer(-1), 44);
        Tool("↓", () => MoveLayer(1), 44);
        Tool(Localization.Text("button.layerselectall"), SelectAllLayers, 112);
        Tool(Localization.Text("button.layerclear"), ClearLayerSelection, 112);
        Tool(Localization.Text("button.layerrename"), RenameActiveLayer, 104);
        Tool(Localization.English ? "↙ Dock" : "↙ Acoplar", () => window.Close(), 94);

        root.Controls.Add(detachedHost, 0, 0);
        root.Controls.Add(tools, 0, 1);
        window.ContentPanel.Controls.Add(root);

        _detachedLayersWindow = window;
        _layerDetachedPlaceholder.Visible = true;
        _layerDetachedPlaceholder.BringToFront();
        _layers.Parent = detachedHost;
        _layers.Dock = DockStyle.Fill;
        _layers.BringToFront();
        _layerDetachButton.Text = Localization.English ? "↙ Dock" : "↙ Acoplar";

        window.FormClosed += (_, _) =>
        {
            if (ReferenceEquals(_detachedLayersWindow, window)) _detachedLayersWindow = null;
            if (!_layerListHost.IsDisposed && !_layers.IsDisposed)
            {
                _layers.Parent = _layerListHost;
                _layers.Dock = DockStyle.Fill;
                _layerDetachedPlaceholder.Visible = false;
                _layerDetachedPlaceholder.SendToBack();
                _layers.BringToFront();
            }
            _layerDetachButton.Text = Localization.English ? "↗ Detach" : "↗ Desacoplar";
        };

        _detachedWindows.ShowSingle("scene-layers", window);
    }
}

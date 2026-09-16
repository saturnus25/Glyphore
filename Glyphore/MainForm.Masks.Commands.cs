namespace Glyphore;

internal sealed partial class MainForm
{
    private void DrawSceneLayerItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= _layers.Items.Count) return;
        object item = _layers.Items[e.Index]!;
        bool selected = (e.State & DrawItemState.Selected) != 0;
        using var bg = new SolidBrush(selected ? Theme.AccentSoft : Theme.Input);
        e.Graphics.FillRectangle(bg, e.Bounds);

        string text;
        Color color;
        int x;
        if (item is MaskListEntry entry)
        {
            text = $"↳  ◇ {entry.Mask.Name}";
            color = entry.Mask.Enabled ? MaskAccent : Theme.Muted;
            x = e.Bounds.Left + 24;
            using var connector = new Pen(Color.FromArgb(110, MaskAccent));
            int mid = e.Bounds.Top + e.Bounds.Height / 2;
            e.Graphics.DrawLine(connector, e.Bounds.Left + 10, e.Bounds.Top, e.Bounds.Left + 10, mid);
            e.Graphics.DrawLine(connector, e.Bounds.Left + 10, mid, e.Bounds.Left + 20, mid);
        }
        else
        {
            text = item.ToString() ?? string.Empty;
            color = Theme.Text;
            x = e.Bounds.Left + 4;
        }
        using var fg = new SolidBrush(color);
        e.Graphics.DrawString(text, _layers.Font, fg, x, e.Bounds.Top + 3);
    }

    private void HandleSceneListSelection()
    {
        if (_applying || !_sceneReady) return;
        if (_layers.SelectedItem is MaskListEntry entry)
        {
            ExitImportedMode();
            if (_scene.ActiveLayerId != entry.Layer.Id)
            {
                SyncSceneFromSettings();
                _scene.ActiveLayerId = entry.Layer.Id;
            }
            _activeMaskId = entry.Mask.Id;
            _preview.ActiveMaskId = _activeMaskId;
            LoadActiveLayerIntoEditor();
            SyncMaskControls();
            UpdateDiscordPresenceContext();
            _preview.Focus();
            return;
        }

        _activeMaskId = null;
        _preview.ActiveMaskId = null;
        UpdateDiscordPresenceContext();
        var selected = SelectedLayers(fallbackActive: false);
        if (selected.Count == 0) { SyncMaskControls(); return; }
        if (_scene.ActiveLayerId is Guid activeId && selected.Any(layer => layer.Id == activeId))
        {
            SyncLayerControls();
            return;
        }
        ActivateLayer(selected[0].Id);
    }

    private void ShowAddMaskMenu(Control anchor)
    {
        if (!_sceneReady || _scene.ActiveLayer is null) return;
        var menu = new ContextMenuStrip { BackColor = Theme.PanelRaised, ForeColor = Theme.Text, ShowImageMargin = false };
        void Add(string text, SceneMaskType type) => menu.Items.Add(text, null, (_, _) => AddMask(type));
        Add("Rectangle", SceneMaskType.Rectangle);
        Add("Circle / Ellipse", SceneMaskType.Ellipse);
        Add("Linear Gradient", SceneMaskType.LinearGradient);
        Add("Radial Gradient", SceneMaskType.RadialGradient);
        Add("Noise", SceneMaskType.Noise);
        Add(Localization.English ? "Rounded Rectangle" : "Rectángulo redondeado", SceneMaskType.RoundedRectangle);
        Add(Localization.English ? "Diamond" : "Rombo", SceneMaskType.Diamond);
        Add(Localization.English ? "Ring" : "Anillo", SceneMaskType.Ring);
        Add(Localization.English ? "Triangle" : "Triángulo", SceneMaskType.Triangle);
        Add(Localization.English ? "Polygon" : "Polígono", SceneMaskType.Hexagon);
        Add(Localization.English ? "Star" : "Estrella", SceneMaskType.Star);
        menu.Closed += (_, _) =>
        {
            // ToolStripDropDown can still be finishing its item-click/visibility path
            // after Closed is raised. Disposing synchronously here causes WinForms to
            // touch an already-disposed ContextMenuStrip (ObjectDisposedException).
            try
            {
                BeginInvoke((Action)(() =>
                {
                    if (!menu.IsDisposed) menu.Dispose();
                }));
            }
            catch (InvalidOperationException)
            {
                // The form is already tearing down; no further UI work will use the menu.
                if (!menu.IsDisposed) menu.Dispose();
            }
        };
        menu.Show(anchor, new Point(0, anchor.Height));
    }

    private void AddMask(SceneMaskType type)
    {
        if (!_sceneReady || _scene.ActiveLayer is not { } layer) return;
        if (layer.Masks.Count >= 8)
        {
            _status.Text = Localization.English ? "A layer can contain at most 8 masks." : "Una capa puede contener como máximo 8 masks.";
            return;
        }

        string baseName = type switch
        {
            SceneMaskType.Ellipse => "Circle Mask",
            SceneMaskType.LinearGradient => "Linear Gradient",
            SceneMaskType.RadialGradient => "Radial Gradient",
            SceneMaskType.Noise => "Noise Mask",
            SceneMaskType.RoundedRectangle => Localization.English ? "Rounded Rectangle" : "Rectángulo redondeado",
            SceneMaskType.Diamond => Localization.English ? "Diamond Mask" : "Máscara rombo",
            SceneMaskType.Ring => Localization.English ? "Ring Mask" : "Máscara anillo",
            SceneMaskType.Triangle => Localization.English ? "Triangle Mask" : "Máscara triángulo",
            SceneMaskType.Hexagon => Localization.English ? "Polygon Mask" : "Máscara polígono",
            SceneMaskType.Star => Localization.English ? "Star Mask" : "Máscara estrella",
            _ => "Rectangle Mask"
        };
        string name = baseName;
        int suffix = 2;
        while (layer.Masks.Any(mask => mask.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) name = $"{baseName} {suffix++}";

        var created = new SceneLayerMask { Type = type, Name = name };
        layer.Masks.Add(created);
        layer.Touch();
        _activeMaskId = created.Id;
        _preview.ActiveMaskId = created.Id;
        UpdateDiscordPresenceContext();
        RefreshLayerList();
        SyncMaskControls();
        PushSceneOnly();
    }

    private void RemoveActiveMask()
    {
        if (!_sceneReady || _activeMaskId is not Guid id) return;
        foreach (var layer in _scene.Layers)
        {
            int index = layer.Masks.FindIndex(mask => mask.Id == id);
            if (index < 0) continue;
            layer.Masks.RemoveAt(index);
            layer.Touch();
            _activeMaskId = null;
            _preview.ActiveMaskId = null;
            UpdateDiscordPresenceContext();
            RefreshLayerList();
            PushSceneOnly();
            return;
        }
    }

    private (SceneEffectLayer Layer, SceneLayerMask Mask)? ActiveMask()
    {
        if (_activeMaskId is not Guid id) return null;
        foreach (var layer in _scene.Layers)
        {
            var mask = layer.Masks.FirstOrDefault(candidate => candidate.Id == id);
            if (mask is not null) return (layer, mask);
        }
        return null;
    }
}

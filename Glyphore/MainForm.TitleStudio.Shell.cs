using System.Drawing.Text;

namespace Glyphore;

internal sealed partial class MainForm
{
    private sealed record TitleStudioShell(
        GlyphoreWindow Dialog,
        ToolTip Tips,
        TableLayoutPanel Root,
        RichTextBox Text,
        SafeComboBox Prefab,
        SafeComboBox VisualStyle,
        SafeComboBox Font,
        SafeComboBox Palette,
        SafeComboBox Charset,
        GlyphCheckBox Bold,
        GlyphCheckBox Italic,
        GlyphCheckBox Animate);

    private TitleStudioShell CreateTitleStudioShell(SceneEffectLayer? activeTitle)
    {
        var dialog = new GlyphoreWindow
        {
            Text = Localization.English ? "ASCII Title Studio · Glyphoré" : "Estudio de títulos ASCII · Glyphoré",
            StartPosition = FormStartPosition.CenterScreen,
            MinimumSize = new Size(980, 860),
            ClientSize = new Size(1120, 960),
            BackColor = Theme.Bg,
            ForeColor = Theme.Text,
            ShowInTaskbar = true,
            ShowIcon = true,
            MinimizeBox = true,
            MaximizeBox = true,
            Resizable = true
        };
        if (Icon is not null) dialog.Icon = Icon;

        var studioTips = new ToolTip
        {
            InitialDelay = 400,
            ReshowDelay = 100,
            AutoPopDelay = 12000,
            ShowAlways = true
        };

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(14, 16, 14, 14),
            Margin = Padding.Empty,
            BackColor = Theme.Bg
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        dialog.ContentPanel.Controls.Add(root);

        // TEXT -------------------------------------------------------------------------------
        var textGroup = new ThemedGroupBox
        {
            Text = Localization.English ? "Text" : "Texto",
            Dock = DockStyle.Fill,
            BackColor = Theme.Panel,
            ForeColor = Theme.Text,
            Margin = new Padding(0, 0, 0, 6)
        };
        var textHost = new ThemedRichTextBoxHost
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            ShowHorizontalScrollBar = false
        };
        RichTextBox text = textHost.Editor;
        text.AcceptsTab = false;
        text.WordWrap = true;
        text.Text = activeTitle?.TitleText ?? "GLYPHORÉ";
        textGroup.Controls.Add(textHost);
        root.Controls.Add(textGroup, 0, 0);

        // SOURCE / STYLE ---------------------------------------------------------------------
        var selectorGroup = new ThemedGroupBox
        {
            Text = Localization.English ? "Source & style" : "Fuente y estilo",
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Theme.Panel,
            ForeColor = Theme.Text,
            Margin = new Padding(0, 0, 0, 8),
            Padding = new Padding(10, 22, 10, 9)
        };
        var selectorGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            RowCount = 2,
            BackColor = Theme.Panel,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        selectorGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        selectorGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        selectorGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        selectorGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        selectorGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        selectorGroup.Controls.Add(selectorGrid);
        root.Controls.Add(selectorGroup, 0, 1);

        Control LabeledControl(string caption, Control control)
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.Panel,
                Margin = new Padding(4, 2, 8, 5),
                Padding = Padding.Empty
            };
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(new Label
            {
                Text = caption,
                Dock = DockStyle.Fill,
                AutoSize = true,
                ForeColor = Theme.Muted,
                TextAlign = ContentAlignment.BottomLeft,
                AutoEllipsis = true,
                Margin = Padding.Empty,
                Padding = new Padding(0, 0, 0, 2)
            }, 0, 0);
            control.Dock = DockStyle.Fill;
            control.MinimumSize = new Size(0, control.PreferredSize.Height);
            control.Margin = new Padding(0, 3, 0, 1);
            panel.Controls.Add(control, 0, 1);
            return panel;
        }

        var prefab = new SafeComboBox();
        SetupCombo(prefab);
        prefab.Items.Add("Custom");
        prefab.Items.AddRange(TitlePrefabs.Select(p => (object)p.Name).ToArray());
        string activePrefab = activeTitle?.TitlePrefab ?? string.Empty;
        prefab.SelectedItem = prefab.Items.Contains(activePrefab) ? activePrefab : "FIGlet · Standard";

        var visualStyle = new SafeComboBox();
        SetupCombo(visualStyle);
        visualStyle.Items.Add("Custom");
        if (_data.Presets.TryGetValue("ASCII Title", out var titleStyles))
            visualStyle.Items.AddRange(titleStyles.Keys.Cast<object>().ToArray());
        string activeStyle = activeTitle?.Preset ?? string.Empty;
        visualStyle.SelectedItem = visualStyle.Items.Contains(activeStyle) ? activeStyle : "Custom";

        var font = new SafeComboBox();
        SetupCombo(font);
        string[] fonts;
        try { fonts = FontFamily.Families.Select(f => f.Name).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToArray(); }
        catch { fonts = ["Consolas", "Cascadia Mono", "Segoe UI", "Arial"]; }
        font.Items.AddRange(fonts.Cast<object>().ToArray());
        string initialFont = activeTitle?.TitleFont ?? "Consolas";
        if (!font.Items.Contains(initialFont)) font.Items.Add(initialFont);
        font.SelectedItem = initialFont;

        var palette = new SafeComboBox();
        SetupCombo(palette);
        palette.Items.AddRange(_data.Palettes.Keys.Cast<object>().ToArray());
        string initialPalette = activeTitle?.PaletteName ?? _settings.PaletteName;
        if (!palette.Items.Contains(initialPalette)) palette.Items.Add(initialPalette);
        palette.SelectedItem = initialPalette;

        var charset = new SafeComboBox();
        SetupCombo(charset);
        charset.Items.AddRange(_data.Charsets.Keys.Cast<object>().ToArray());
        string initialCharset = activeTitle?.CharsetName ?? _settings.CharsetName;
        if (!charset.Items.Contains(initialCharset)) charset.Items.Add(initialCharset);
        charset.SelectedItem = initialCharset;

        var optionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Theme.Panel,
            Margin = Padding.Empty,
            Padding = new Padding(0, 6, 0, 0)
        };
        var bold = new GlyphCheckBox
        {
            Text = Localization.English ? "Bold" : "Negrita",
            Checked = activeTitle?.TitleBold ?? true,
            AutoSize = true,
            Margin = new Padding(0, 3, 14, 0)
        };
        var italic = new GlyphCheckBox
        {
            Text = Localization.English ? "Italic" : "Cursiva",
            Checked = activeTitle?.TitleItalic ?? false,
            AutoSize = true,
            Margin = new Padding(0, 3, 14, 0)
        };
        var animate = new GlyphCheckBox
        {
            Text = Localization.English ? "Animate" : "Animar",
            Checked = activeTitle?.TitleAnimate ?? true,
            AutoSize = true,
            Margin = new Padding(0, 3, 0, 0)
        };
        Theme.CheckBox(bold);
        Theme.CheckBox(italic);
        Theme.CheckBox(animate);
        optionPanel.Controls.AddRange([bold, italic]);

        selectorGrid.Controls.Add(LabeledControl(Localization.English ? "ASCII font / prefab" : "Fuente ASCII / prefab", prefab), 0, 0);
        selectorGrid.Controls.Add(LabeledControl(Localization.English ? "Visual style / preset" : "Estilo / preset visual", visualStyle), 1, 0);
        selectorGrid.Controls.Add(LabeledControl(Localization.English ? "Palette" : "Paleta", palette), 2, 0);
        selectorGrid.Controls.Add(LabeledControl(Localization.English ? "Base font" : "Fuente base", font), 0, 1);
        selectorGrid.Controls.Add(LabeledControl(Localization.English ? "Character set" : "Set de caracteres", charset), 1, 1);
        selectorGrid.Controls.Add(LabeledControl(Localization.English ? "Font options" : "Formato fuente", optionPanel), 2, 1);

        studioTips.SetToolTip(prefab, Localization.English
            ? "Chooses the ASCII/FIGlet geometry for the title."
            : "Elige la geometría ASCII/FIGlet del título.");
        studioTips.SetToolTip(visualStyle, Localization.English
            ? "Applies a visual preset to the title."
            : "Aplica un preset visual al título.");
        studioTips.SetToolTip(animate, Localization.English
            ? "Enables or freezes time-based title effects such as shimmer, wave, glitch and reveal."
            : "Activa o congela los efectos temporales del título, como shimmer, wave, glitch y reveal.");

        return new TitleStudioShell(
            dialog,
            studioTips,
            root,
            text,
            prefab,
            visualStyle,
            font,
            palette,
            charset,
            bold,
            italic,
            animate);
    }
}

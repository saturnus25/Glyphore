namespace Glyphore;

internal sealed class AboutForm : GlyphoreWindow
{
    private readonly Icon? _appIcon;
    private readonly DetachedWindowManager? _detachedWindows;
    private readonly GlyphButton _thirdPartyButton = new();

    internal bool HasThirdPartyLicensesAction => _thirdPartyButton.Text.Contains("Third-Party Licenses", StringComparison.Ordinal);

    public AboutForm(Icon? appIcon, DetachedWindowManager? detachedWindows = null)
    {
        _appIcon = appIcon;
        _detachedWindows = detachedWindows;

        Text = "About Glyphoré";
        ClientSize = new Size(720, 500);
        MinimumSize = new Size(620, 455);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Bg;
        ForeColor = Theme.Text;
        Font = new Font("Segoe UI", 9f);
        AutoScaleMode = AutoScaleMode.Dpi;
        ShowIcon = appIcon is not null;
        if (appIcon is not null) Icon = appIcon;
        ShowInTaskbar = true;
        MaximizeBox = false;
        MinimizeBox = true;
        Resizable = false;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(24, 22, 24, 22),
            BackColor = Theme.Bg,
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 142));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        ContentPanel.Controls.Add(root);

        var identity = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Theme.PanelRaised,
            Padding = new Padding(18),
            Margin = new Padding(0, 0, 0, 12)
        };
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104));
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        identity.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(identity, 0, 0);

        var brand = new BrandMarkControl
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 14, 0)
        };
        identity.Controls.Add(brand, 0, 0);

        string version = typeof(AboutForm).Assembly.GetName().Version?.ToString(3) ?? "6.0.0";
        var identityText = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Theme.PanelRaised,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        identityText.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        identityText.RowStyles.Add(new RowStyle(SizeType.Percent, 27));
        identityText.RowStyles.Add(new RowStyle(SizeType.Percent, 28));
        identity.Controls.Add(identityText, 1, 0);

        identityText.Controls.Add(new Label
        {
            Text = "Glyphoré",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            ForeColor = Theme.Text,
            Font = new Font("Segoe UI Semibold", 22f, FontStyle.Bold),
            Margin = Padding.Empty
        }, 0, 0);
        identityText.Controls.Add(new Label
        {
            Text = $"Version {version}",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Theme.AccentText,
            Font = new Font("Segoe UI Semibold", 9.5f),
            Margin = Padding.Empty
        }, 0, 1);
        identityText.Controls.Add(new Label
        {
            Text = "PROCEDURAL CHARACTER ART STUDIO",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = Theme.Muted,
            Font = new Font("Segoe UI Semibold", 8f),
            Margin = Padding.Empty
        }, 0, 2);

        var details = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Theme.Bg,
            Margin = Padding.Empty,
            Padding = new Padding(2, 8, 2, 0)
        };
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        details.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(details, 0, 1);

        details.Controls.Add(new Label
        {
            Text = Localization.English ? "Open source" : "Código abierto",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Theme.AccentText,
            Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
            Margin = Padding.Empty
        }, 0, 0);

        details.Controls.Add(new Label
        {
            Text = Localization.English
                ? "Glyphoré is released under the MIT License. Bundled third-party components keep their own license terms."
                : "Glyphoré se distribuye bajo la licencia MIT. Los componentes de terceros incluidos conservan sus propias licencias.",
            Dock = DockStyle.Fill,
            ForeColor = Theme.Muted,
            AutoEllipsis = false,
            Margin = Padding.Empty
        }, 0, 1);

        details.Controls.Add(new Label
        {
            Text = "Third-Party Licenses",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Theme.AccentText,
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            Margin = Padding.Empty
        }, 0, 2);

        details.Controls.Add(new Label
        {
            Text = Localization.English
                ? "Review the notices and full license texts included with Glyphoré."
                : "Consulta los avisos y textos completos de las licencias incluidas con Glyphoré.",
            Dock = DockStyle.Fill,
            ForeColor = Theme.Muted,
            Margin = Padding.Empty
        }, 0, 3);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Theme.Bg,
            Padding = new Padding(0, 12, 0, 0),
            Margin = Padding.Empty
        };
        root.Controls.Add(actions, 0, 2);

        _thirdPartyButton.Text = "Third-Party Licenses…";
        _thirdPartyButton.AutoSize = false;
        _thirdPartyButton.Size = new Size(238, 36);
        _thirdPartyButton.Margin = Padding.Empty;
        Theme.Button(_thirdPartyButton, accent: true);
        _thirdPartyButton.Click += (_, _) => OpenThirdPartyLicenses();
        actions.Controls.Add(_thirdPartyButton);

        KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Escape) return;
            e.Handled = true;
            Close();
        };
    }

    private void OpenThirdPartyLicenses()
    {
        if (_detachedWindows is not null)
        {
            _detachedWindows.ShowSingle(
                "third-party-licenses",
                () => new ThirdPartyLicensesForm(_appIcon, _detachedWindows));
            return;
        }

        using Form licenses = CreateThirdPartyLicensesWindow();
        licenses.ShowDialog(this);
    }

    internal Form CreateThirdPartyLicensesWindow() => new ThirdPartyLicensesForm(_appIcon, _detachedWindows);
}

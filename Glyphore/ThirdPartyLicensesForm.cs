namespace Glyphore;

internal sealed class ThirdPartyLicensesForm : GlyphoreWindow
{
    private readonly Icon? _appIcon;
    private readonly DetachedWindowManager? _detachedWindows;
    private readonly FlowLayoutPanel _entriesPanel = new();

    internal int RenderedEntryCount => _entriesPanel.Controls.OfType<Panel>().Count();

    public ThirdPartyLicensesForm(Icon? appIcon, DetachedWindowManager? detachedWindows = null)
    {
        _appIcon = appIcon;
        _detachedWindows = detachedWindows;

        Text = "Third-Party Licenses — Glyphoré";
        ClientSize = new Size(780, 590);
        MinimumSize = new Size(650, 480);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Bg;
        ForeColor = Theme.Text;
        Font = new Font("Segoe UI", 9f);
        AutoScaleMode = AutoScaleMode.Dpi;
        ShowIcon = appIcon is not null;
        if (appIcon is not null) Icon = appIcon;
        ShowInTaskbar = true;
        MaximizeBox = true;
        MinimizeBox = true;
        Resizable = true;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Theme.Bg,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        ContentPanel.Controls.Add(root);

        var heading = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Theme.PanelRaised,
            Padding = new Padding(22, 10, 22, 8),
            Margin = Padding.Empty
        };
        heading.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        heading.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(heading, 0, 0);
        heading.Controls.Add(new Label
        {
            Text = "Third-Party Licenses",
            Dock = DockStyle.Fill,
            ForeColor = Theme.Text,
            Font = new Font("Segoe UI Semibold", 16f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 0, 0);
        heading.Controls.Add(new Label
        {
            Text = Localization.English
                ? "License terms for components bundled with Glyphoré."
                : "Licencias de los componentes incluidos con Glyphoré.",
            Dock = DockStyle.Fill,
            ForeColor = Theme.Muted,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 0, 1);

        _entriesPanel.Dock = DockStyle.Top;
        _entriesPanel.FlowDirection = FlowDirection.TopDown;
        _entriesPanel.WrapContents = false;
        _entriesPanel.AutoScroll = false;
        _entriesPanel.AutoSize = true;
        _entriesPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _entriesPanel.BackColor = Theme.Bg;
        _entriesPanel.Padding = new Padding(18, 16, 18, 16);
        _entriesPanel.Margin = Padding.Empty;

        var entriesScroll = new ThemedScrollPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Bg,
            Margin = Padding.Empty
        };
        entriesScroll.SetContent(_entriesPanel);
        root.Controls.Add(entriesScroll, 0, 1);

        foreach (ThirdPartyLicenseEntry entry in ThirdPartyLicenses.Entries)
            _entriesPanel.Controls.Add(CreateEntryCard(entry));

        _entriesPanel.ClientSizeChanged += (_, _) => ResizeCards();
        ResizeCards();
    }

    private Panel CreateEntryCard(ThirdPartyLicenseEntry entry)
    {
        var card = new Panel
        {
            Height = 174,
            BackColor = Theme.Panel,
            Margin = new Padding(0, 0, 0, 12),
            Padding = new Padding(16),
            Tag = entry
        };

        card.Controls.Add(new Label
        {
            Text = $"{entry.Name} {entry.Version}",
            Left = 16,
            Top = 14,
            AutoSize = true,
            ForeColor = Theme.Text,
            Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold)
        });

        card.Controls.Add(new Label
        {
            Text = entry.LicenseName,
            Left = 16,
            Top = 43,
            AutoSize = true,
            ForeColor = Theme.AccentText,
            Font = new Font("Segoe UI Semibold", 9f)
        });

        card.Controls.Add(new Label
        {
            Text = entry.Copyright,
            Left = 16,
            Top = 68,
            AutoSize = true,
            ForeColor = Theme.Muted
        });

        if (!string.IsNullOrWhiteSpace(entry.Notes))
        {
            card.Controls.Add(new Label
            {
                Text = entry.Notes,
                Left = 16,
                Top = 92,
                Width = 500,
                Height = 36,
                ForeColor = Theme.Muted,
                Name = "EntryNotes"
            });
        }

        var viewButton = new GlyphButton
        {
            Text = Localization.English ? "View full license" : "Ver licencia completa",
            Width = 176,
            Height = 31,
            Left = 16,
            Top = 132,
            Tag = entry
        };
        Theme.Button(viewButton, accent: true);
        viewButton.Click += (_, _) => OpenLicense(entry);
        card.Controls.Add(viewButton);

        return card;
    }

    private void OpenLicense(ThirdPartyLicenseEntry entry)
    {
        if (_detachedWindows is not null)
        {
            _detachedWindows.ShowSingle(
                $"license:{entry.Name}:{entry.Version}",
                () => new FullLicenseForm(entry, _appIcon));
            return;
        }

        using Form viewer = CreateLicenseViewer(entry);
        viewer.ShowDialog(this);
    }

    private void ResizeCards()
    {
        int width = Math.Max(360, _entriesPanel.ClientSize.Width - _entriesPanel.Padding.Horizontal);
        foreach (Panel card in _entriesPanel.Controls.OfType<Panel>())
        {
            card.Width = width;
            Label? notes = card.Controls.Find("EntryNotes", false).OfType<Label>().FirstOrDefault();
            if (notes is not null) notes.Width = Math.Max(220, width - 32);
        }
    }

    internal Form CreateLicenseViewer(ThirdPartyLicenseEntry entry) => new FullLicenseForm(entry, _appIcon);
}

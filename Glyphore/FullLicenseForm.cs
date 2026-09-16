namespace Glyphore;

internal sealed class FullLicenseForm : GlyphoreWindow
{
    private readonly LicenseTextViewer _licenseText = new();

    internal string DisplayedLicenseText => _licenseText.Text;
    internal int FirstVisibleLicenseLine => _licenseText.FirstVisibleLine;
    internal int LicenseLineCount => _licenseText.Lines.Length;
    internal bool IsLicenseEndVisible => _licenseText.IsDocumentEndVisible;
    internal bool LicenseIsReadOnly => _licenseText.ReadOnly;
    internal bool LicenseHasNativeScrollBars => _licenseText.ScrollBars == RichTextBoxScrollBars.Both;

    public FullLicenseForm(ThirdPartyLicenseEntry entry, Icon? appIcon)
    {
        ArgumentNullException.ThrowIfNull(entry);

        Text = $"{entry.LicenseName} — {entry.Name}";
        ClientSize = new Size(880, 700);
        MinimumSize = new Size(640, 500);
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
            RowCount = 3,
            BackColor = Theme.Bg,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        ContentPanel.Controls.Add(root);

        var heading = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Theme.PanelRaised,
            Padding = new Padding(20, 9, 20, 8),
            Margin = Padding.Empty
        };
        heading.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        heading.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(heading, 0, 0);
        heading.Controls.Add(new Label
        {
            Text = entry.LicenseName,
            Dock = DockStyle.Fill,
            ForeColor = Theme.Text,
            Font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        }, 0, 0);
        heading.Controls.Add(new Label
        {
            Text = $"{entry.Name} {entry.Version} · {entry.Copyright}",
            Dock = DockStyle.Fill,
            ForeColor = Theme.Muted,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            Margin = Padding.Empty
        }, 0, 1);

        // License text deliberately uses the RichEdit control's native scrolling. It is less
        // decorative than a custom scrollbar, but it is the most reliable path for long legal
        // documents: wheel, thumb drag, Page Up/Down, arrows and resize all share one native
        // scroll range, while the text remains selectable/copyable and strictly read-only.
        var viewerFrame = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(12, 12, 12, 6),
            Padding = new Padding(1),
            BackColor = Theme.Border
        };
        _licenseText.Dock = DockStyle.Fill;
        _licenseText.Margin = Padding.Empty;
        _licenseText.ReadOnly = true;
        _licenseText.WordWrap = false;
        _licenseText.DetectUrls = false;
        _licenseText.BackColor = Theme.Input;
        _licenseText.ForeColor = Theme.Text;
        _licenseText.Font = new Font("Cascadia Mono", 9.5f);
        _licenseText.Text = ThirdPartyLicenses.ReadFullLicense(entry);
        viewerFrame.Controls.Add(_licenseText);
        root.Controls.Add(viewerFrame, 0, 1);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Theme.Bg,
            Padding = new Padding(12, 6, 12, 10),
            Margin = Padding.Empty
        };
        var close = new GlyphButton
        {
            Text = Localization.English ? "Close" : "Cerrar",
            AutoSize = false,
            Size = new Size(112, 32),
            Margin = Padding.Empty
        };
        Theme.Button(close);
        close.Click += (_, _) => Close();
        actions.Controls.Add(close);
        root.Controls.Add(actions, 0, 2);
    }

    internal void ScrollLicenseToEndForTest()
    {
        _licenseText.SelectionLength = 0;
        _licenseText.SelectionStart = _licenseText.TextLength;
        _licenseText.ScrollToCaret();
    }

    internal void ScrollLicenseToStartForTest()
    {
        _licenseText.SelectionLength = 0;
        _licenseText.SelectionStart = 0;
        _licenseText.ScrollToCaret();
    }
}

/// <summary>
/// Read-only legal-text viewer. The operating system owns the scroll range and scrollbar so
/// every line remains reachable even after resize/DPI changes. Home/End intentionally mean
/// document start/end in this viewer; selection variants keep the RichEdit defaults.
/// </summary>

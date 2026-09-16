namespace Glyphore;

internal sealed partial class MainForm
{
    private readonly AppPreferences _preferences = AppPreferences.Load();
    private readonly DiscordRichPresenceService _discordPresence;
    private readonly GlyphoreActivityMessageFilter _discordActivityFilter;
    private bool _titleStudioPresenceOpen;
    private bool _presenceShutdownStarted;
    private bool _presenceShutdownComplete;

    private void InitializeDiscordPresence()
    {
        Application.AddMessageFilter(_discordActivityFilter);
        _discordPresence.SetContext(PresenceContext.Scene);
        _discordPresence.Start();
        FormClosing += HandlePresenceAwareFormClosing;
    }

    private async void HandlePresenceAwareFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_presenceShutdownComplete) return;
        e.Cancel = true;
        if (_presenceShutdownStarted) return;

        _presenceShutdownStarted = true;
        Application.RemoveMessageFilter(_discordActivityFilter);
        try
        {
            await _discordPresence.DisposeAsync();
        }
        catch
        {
            // Presence shutdown is best-effort and must never trap the application open.
        }
        finally
        {
            _presenceShutdownComplete = true;
            _presenceShutdownStarted = false;
            FormClosing -= HandlePresenceAwareFormClosing;
            BeginInvoke((Action)Close);
        }
    }

    private void UpdateDiscordPresenceContext()
    {
        PresenceContext context = _activeMaskId.HasValue
            ? PresenceContext.MaskEditing
            : _titleStudioPresenceOpen
                ? PresenceContext.AsciiTitleStudio
                : PresenceContext.Scene;
        _discordPresence.SetContext(context);
    }

    private void NotifyDiscordPreviewing()
    {
        _discordPresence.NotifyUserActivity();
        _discordPresence.NotifyPreviewing();
    }

    private void OpenPreferences()
    {
        _detachedWindows.ShowSingle("preferences", () =>
        {
            var window = new GlyphoreWindow
            {
                Text = Localization.English ? "Settings · Glyphoré" : "Ajustes · Glyphoré",
                StartPosition = FormStartPosition.CenterScreen,
                ClientSize = new Size(520, 215),
                MinimumSize = new Size(520, 215),
                BackColor = Theme.Bg,
                ForeColor = Theme.Text,
                ShowInTaskbar = true,
                ShowIcon = true,
                MinimizeBox = true,
                MaximizeBox = false,
                Resizable = false
            };
            if (Icon is not null) window.Icon = Icon;

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Bg,
                Padding = new Padding(16)
            };
            window.ContentPanel.Controls.Add(panel);

            var group = new ThemedGroupBox
            {
                Text = "Discord",
                Left = 16,
                Top = 16,
                Width = 468,
                Height = 135,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Theme.Panel,
                ForeColor = Theme.Text
            };
            panel.Controls.Add(group);

            var check = new GlyphCheckBox
            {
                Text = "Discord Rich Presence",
                Left = 14,
                Top = 30,
                Width = 420,
                Height = 26,
                Checked = _preferences.DiscordRichPresenceEnabled
            };
            Theme.CheckBox(check);
            group.Controls.Add(check);

            var description = new Label
            {
                Left = 34,
                Top = 63,
                Width = 405,
                Height = 46,
                ForeColor = Theme.Muted,
                Text = Localization.English
                    ? "Shows your Glyphoré activity on Discord."
                    : "Muestra en Discord cuándo estás usando Glyphoré."
            };
            group.Controls.Add(description);

            check.CheckedChanged += (_, _) =>
            {
                _preferences.DiscordRichPresenceEnabled = check.Checked;
                _preferences.Save();
                _discordPresence.SetEnabled(check.Checked);
                _discordPresence.NotifyUserActivity();
                UpdateDiscordPresenceContext();
            };

            return window;
        });
    }
}

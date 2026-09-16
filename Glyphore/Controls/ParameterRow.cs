using System.ComponentModel;

namespace Glyphore;

internal sealed class ParameterRow : UserControl
{
    private readonly GlyphSlider? _track;
    private readonly TextBox? _text;
    private readonly SafeComboBox? _combo;
    private readonly ParamDesc _desc;
    private readonly ToolTip _tip = new() { InitialDelay = 450, ReshowDelay = 100, AutoPopDelay = 10000, ShowAlways = true };
    private readonly System.Windows.Forms.Timer? _textCommitTimer;
    private bool _sync;
    private bool _textDirty;

    public event Action<double>? ValueChanged;
    public double Value { get; private set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool MouseWheelEditsValue
    {
        get => _track?.MouseWheelAdjustsValue ?? false;
        set
        {
            if (_track is not null) _track.MouseWheelAdjustsValue = value;
        }
    }

    public ParameterRow(ParamDesc description, double value)
    {
        description = Localization.Param(description);
        _desc = description;
        Height = 34;
        Width = 390;
        Dock = DockStyle.None;
        BackColor = Theme.Panel;
        ForeColor = Theme.Text;

        var label = new Label
        {
            Text = description.Label,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Left = 0,
            Top = 4,
            Width = 125,
            Height = 26,
            ForeColor = Theme.Text
        };
        Controls.Add(label);

        if (description.Options is { Length: > 0 } options)
        {
            _combo = new SafeComboBox
            {
                Left = 127,
                Top = 4,
                Width = 263,
                Height = 26,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Theme.Combo(_combo);
            _combo.Items.AddRange(options.Select(option => (object)(Localization.English ? option.En : option.Es)).ToArray());
            _combo.SelectedIndexChanged += (_, _) =>
            {
                if (_sync || _combo.SelectedIndex < 0) return;
                SetInternal(_combo.SelectedIndex, true, true);
            };
            Resize += (_, _) => _combo.Width = Math.Max(90, Width - _combo.Left);
            Controls.Add(_combo);
        }
        else
        {
            _track = new GlyphSlider
            {
                Left = 127,
                Top = 2,
                Width = 180,
                Height = 30,
                Minimum = 0,
                Maximum = 1000
            };
            _text = new TextBox { Left = 312, Top = 5, Width = 78, Height = 24 };
            _textCommitTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _textCommitTimer.Tick += (_, _) =>
            {
                _textCommitTimer.Stop();
                CommitText(hardCommit: false);
            };

            Theme.TextBox(_text);
            Controls.Add(_track);
            Controls.Add(_text);

            _track.ValueChanged += (_, _) =>
            {
                if (_sync) return;

                // A slider interaction is newer than any text that was waiting to be committed.
                // Cancel it and make the textbox reflect the slider immediately.
                CancelPendingText();
                double v = description.Min + (_track.Value / 1000.0) * (description.Max - description.Min);
                SetInternal(v, true, updateText: true);
            };

            _text.TextChanged += (_, _) =>
            {
                if (_sync) return;
                _textDirty = true;
                _textCommitTimer.Stop();
                _textCommitTimer.Start();
            };

            _text.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                _textCommitTimer.Stop();
                CommitText(hardCommit: true);
                e.SuppressKeyPress = true;
            };

            _text.Leave += (_, _) =>
            {
                _textCommitTimer.Stop();
                if (_textDirty) CommitText(hardCommit: true);
            };

            Resize += (_, _) =>
            {
                _text.Left = Math.Max(_track.Left + 52, Width - 80);
                _track.Width = Math.Max(40, _text.Left - _track.Left - 5);
            };
        }

        if (!string.IsNullOrWhiteSpace(description.Help))
        {
            _tip.SetToolTip(this, description.Help);
            _tip.SetToolTip(label, description.Help);
            if (_track is not null) _tip.SetToolTip(_track, description.Help);
            if (_text is not null) _tip.SetToolTip(_text, description.Help);
            if (_combo is not null) _tip.SetToolTip(_combo, description.Help);
        }

        SetValue(value, false);
    }

    public void SetValue(double value, bool notify = false)
    {
        CancelPendingText();
        SetInternal(value, notify, updateText: true);
    }

    private void SetInternal(double value, bool notify, bool updateText)
    {
        if (!double.IsFinite(value)) return;

        _sync = true;
        try
        {
            if (_combo is not null && _desc.Options is { Length: > 0 } options)
            {
                int index = Math.Clamp((int)Math.Round(value), 0, options.Length - 1);
                Value = index;
                if (_combo.SelectedIndex != index) _combo.SelectedIndex = index;
            }
            else
            {
                // Slider Min/Max are a comfortable editing range, not a hard limit.
                // Typed values may exceed it; the thumb simply stays pinned to the nearest edge.
                Value = value;
                double sliderValue = Math.Clamp(value, _desc.Min, _desc.Max);
                if (_track is not null)
                {
                    _track.Value = (int)Math.Round((sliderValue - _desc.Min) / Math.Max(1e-12, _desc.Max - _desc.Min) * 1000.0);
                }

                if (_text is not null && updateText)
                {
                    string formatted = FormatValue(value);
                    if (!string.Equals(_text.Text, formatted, StringComparison.Ordinal))
                    {
                        _text.Text = formatted;
                        _text.SelectionStart = _text.TextLength;
                    }
                }
            }
        }
        finally
        {
            _sync = false;
        }

        if (notify) ValueChanged?.Invoke(Value);
    }

    private string FormatValue(double value) => _desc.Digits == 0
        ? value.ToString("0", System.Globalization.CultureInfo.InvariantCulture)
        : value.ToString("0." + new string('#', Math.Max(1, _desc.Digits)), System.Globalization.CultureInfo.InvariantCulture);

    private void CommitText(bool hardCommit)
    {
        if (_text is null || !_textDirty) return;

        string text = _text.Text.Trim().Replace(',', '.');
        if (double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value) && double.IsFinite(value))
        {
            bool changed = value != Value;
            _textDirty = false;
            // During debounced live typing, preserve exactly what the user typed so the caret
            // does not jump. Enter/Leave normalize the display. A later slider interaction
            // always forces the textbox to the slider value.
            SetInternal(value, changed, updateText: hardCommit);
            return;
        }

        // Partial input such as "-", "." or "1e" is allowed while typing.
        // Only reject it when the user explicitly commits or leaves the field.
        if (!hardCommit) return;

        _textDirty = false;
        System.Media.SystemSounds.Beep.Play();
        SetInternal(Value, false, updateText: true);
    }

    private void CancelPendingText()
    {
        _textCommitTimer?.Stop();
        _textDirty = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _textCommitTimer?.Dispose();
            _tip.Dispose();
        }
        base.Dispose(disposing);
    }
}

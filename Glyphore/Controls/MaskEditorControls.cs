namespace Glyphore;

internal sealed class MaskListEntry
{
    public SceneEffectLayer Layer { get; }
    public SceneLayerMask Mask { get; }
    public MaskListEntry(SceneEffectLayer layer, SceneLayerMask mask) { Layer = layer; Mask = mask; }
    public override string ToString() => $"   ↳ ◇ {Mask.Name}";
}

internal sealed class MaskSliderField : UserControl
{
    private const int SliderSteps = 100_000;
    private readonly Label _label = new();
    private readonly Label _description = new();
    private readonly GlyphSlider _slider = new();
    private readonly TextBox _text = new();
    private readonly System.Windows.Forms.Timer _commitTimer = new() { Interval = 250 };
    private readonly double _minimum;
    private readonly double _maximum;
    private readonly int _digits;
    private bool _sync;
    private bool _textDirty;

    public event EventHandler? ValueChanged;
    public double Value { get; private set; }

    public MaskSliderField(string label, double minimum, double maximum, double value, int digits)
    {
        _minimum = minimum;
        _maximum = maximum;
        _digits = digits;
        Height = 76;
        BackColor = Theme.Panel;

        _label.Text = label;
        _label.ForeColor = Theme.Text;
        _label.TextAlign = ContentAlignment.MiddleLeft;
        _label.SetBounds(0, 0, 180, 18);

        _description.ForeColor = Theme.Muted;
        _description.TextAlign = ContentAlignment.MiddleLeft;
        _description.AutoEllipsis = false;
        _description.SetBounds(0, 17, 180, 29);

        _slider.Minimum = 0;
        _slider.Maximum = SliderSteps;
        _slider.MouseWheelAdjustsValue = false;
        _slider.SetBounds(0, 47, 125, 27);

        _text.SetBounds(132, 49, 62, 23);
        Theme.TextBox(_text);

        Controls.AddRange([_label, _description, _slider, _text]);

        _slider.ValueChanged += (_, _) =>
        {
            if (_sync) return;
            _commitTimer.Stop();
            _textDirty = false;
            double valueFromSlider = _minimum + (_slider.Value / (double)SliderSteps) * (_maximum - _minimum);
            SetValue(valueFromSlider, notify: true, updateText: true);
        };

        _text.TextChanged += (_, _) =>
        {
            if (_sync) return;
            _textDirty = true;
            _commitTimer.Stop();
            _commitTimer.Start();
        };
        _text.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            _commitTimer.Stop();
            CommitText(hardCommit: true);
            e.SuppressKeyPress = true;
        };
        _text.Leave += (_, _) =>
        {
            _commitTimer.Stop();
            if (_textDirty) CommitText(hardCommit: true);
        };
        _commitTimer.Tick += (_, _) =>
        {
            _commitTimer.Stop();
            CommitText(hardCommit: false);
        };

        Resize += (_, _) => LayoutChildren();
        SetValue(value);
    }

    public void SetLabel(string text) => _label.Text = text;
    public void SetDescription(string text) => _description.Text = text;

    public void SetToolTip(ToolTip toolTip, string text)
    {
        toolTip.SetToolTip(this, text);
        toolTip.SetToolTip(_label, text);
        toolTip.SetToolTip(_description, text);
        toolTip.SetToolTip(_slider, text);
        toolTip.SetToolTip(_text, text);
    }

    public void SetValue(double value, bool notify = false) => SetValue(value, notify, updateText: true);

    private void SetValue(double value, bool notify, bool updateText)
    {
        if (!double.IsFinite(value)) return;
        double next = Math.Clamp(value, _minimum, _maximum);
        bool changed = Math.Abs(next - Value) > 1e-12;

        _sync = true;
        try
        {
            Value = next;
            double t = (_maximum - _minimum) <= 1e-12 ? 0 : (next - _minimum) / (_maximum - _minimum);
            _slider.Value = (int)Math.Round(Math.Clamp(t, 0, 1) * SliderSteps);
            if (updateText)
            {
                string formatted = FormatValue(next);
                if (!string.Equals(_text.Text, formatted, StringComparison.Ordinal))
                {
                    _text.Text = formatted;
                    _text.SelectionStart = _text.TextLength;
                }
            }
        }
        finally { _sync = false; }

        if (notify && changed) ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CommitText(bool hardCommit)
    {
        if (!_textDirty) return;
        string raw = _text.Text.Trim().Replace(',', '.');
        if (double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double parsed) && double.IsFinite(parsed))
        {
            _textDirty = false;
            // The textbox is exact; only the engine's meaningful/safe range is clamped.
            SetValue(parsed, notify: true, updateText: hardCommit || parsed < _minimum || parsed > _maximum);
            return;
        }
        if (!hardCommit) return;
        _textDirty = false;
        System.Media.SystemSounds.Beep.Play();
        SetValue(Value, notify: false, updateText: true);
    }

    private string FormatValue(double value) => _digits <= 0
        ? value.ToString("0", System.Globalization.CultureInfo.InvariantCulture)
        : value.ToString("0." + new string('#', _digits), System.Globalization.CultureInfo.InvariantCulture);

    private void LayoutChildren()
    {
        _label.Width = Math.Max(60, Width);
        _description.Width = Math.Max(60, Width);
        int textWidth = 62;
        _text.Left = Math.Max(50, Width - textWidth);
        _text.Width = textWidth;
        _slider.Width = Math.Max(38, _text.Left - 7);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _commitTimer.Dispose();
        base.Dispose(disposing);
    }
}

namespace Glyphore;

internal sealed partial class MainForm
{
    private ThemedGroupBox Group(string title, int height)
    {
        return new ThemedGroupBox
        {
            Text = title,
            Width = 430,
            Height = height,
            ForeColor = Theme.Text,
            BackColor = Theme.Panel,
            Margin = new Padding(0, 0, 0, 7)
        };
    }

    private Button Btn(string text, EventHandler click)
    {
        var button = new GlyphButton { Text = text };
        Theme.Button(button);
        button.Click += click;
        return button;
    }

    private static void SetupCombo(ComboBox combo)
    {
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.DrawMode = DrawMode.OwnerDrawFixed;
        combo.ItemHeight = 22;
        Theme.Combo(combo);
        combo.DrawItem += (_, e) =>
        {
            if (e.Index < 0) return;
            bool selected = (e.State & DrawItemState.Selected) != 0;
            using var background = new SolidBrush(selected ? Theme.AccentSoft : Theme.Input);
            using var foreground = new SolidBrush(Theme.Text);
            e.Graphics.FillRectangle(background, e.Bounds);
            e.Graphics.DrawString(
                combo.Items[e.Index]?.ToString() ?? string.Empty,
                combo.Font,
                foreground,
                e.Bounds.Left + 3,
                e.Bounds.Top + 3);
        };
    }

    private static void SetupNumeric(
        NumericUpDown numeric,
        decimal min,
        decimal max,
        decimal value,
        int decimals = 0)
    {
        numeric.Minimum = min;
        numeric.Maximum = max;
        numeric.Value = Math.Clamp(value, min, max);
        numeric.DecimalPlaces = decimals;
        numeric.Increment = decimals > 0 ? 0.1m : 1m;
        Theme.Numeric(numeric);
    }

    private static void AddNumericRow(Control parent, string labelKey, NumericUpDown numeric, int y)
    {
        bool localized = labelKey.StartsWith("label.");
        var label = new Label
        {
            Text = localized ? Localization.Text(labelKey) : labelKey,
            Tag = localized ? labelKey : null,
            Left = 10,
            Top = y + 3,
            Width = 180,
            Height = 24,
            ForeColor = Theme.Text
        };
        numeric.SetBounds(280, y, 125, 25);
        numeric.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        parent.Controls.Add(label);
        parent.Controls.Add(numeric);
    }
}

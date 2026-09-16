namespace Glyphore;

internal sealed class GlyphNumericUpDown : NumericUpDown
{
    private const int WmMouseWheel = 0x020A;

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmMouseWheel)
        {
            int delta = unchecked((short)((long)m.WParam >> 16));
            MouseWheelRouting.Route(this, delta);
            return;
        }

        base.WndProc(ref m);
    }
}

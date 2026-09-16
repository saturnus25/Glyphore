namespace Glyphore;

internal sealed class SafeComboBox : ComboBox
{
    private const int WM_MOUSEWHEEL = 0x020A;

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_MOUSEWHEEL && !DroppedDown)
        {
            int delta = unchecked((short)((long)m.WParam >> 16));
            MouseWheelRouting.Route(this, delta);
            return;
        }

        base.WndProc(ref m);
    }
}

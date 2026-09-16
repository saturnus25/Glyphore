namespace Glyphore;

internal interface IGlyphWheelScrollHost
{
    bool ScrollByWheel(int delta);
}

internal static class MouseWheelRouting
{
    public static bool Route(Control source, int delta)
    {
        if (delta == 0) return false;

        for (Control? control = source.Parent; control is not null; control = control.Parent)
        {
            if (control is IGlyphWheelScrollHost host && host.ScrollByWheel(delta))
                return true;

            if (control is ScrollableControl scrollable && scrollable.AutoScroll)
            {
                int lines = SystemInformation.MouseWheelScrollLines;
                if (lines == 0) return false;
                int step = lines < 0
                    ? Math.Max(1, scrollable.ClientSize.Height)
                    : Math.Max(24, source.Font.Height * Math.Max(1, lines));
                int current = -scrollable.AutoScrollPosition.Y;
                int next = Math.Max(0, current - (int)Math.Round(delta / 120.0 * step));
                scrollable.AutoScrollPosition = new Point(0, next);
                return true;
            }
        }

        return false;
    }
}

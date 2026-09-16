namespace Glyphore;

internal static class ColorUtil
{
    internal static Color ParseHtmlOrWhite(string value)
    {
        try { return ColorTranslator.FromHtml(value); }
        catch { return Color.White; }
    }
}

namespace Glyphore;

internal static class OutputAttribution
{
    public const string ProjectUrl = "https://github.com/saturnus25/Glyphore";
    public const string CreditEnglish = "Created with Glyphoré — " + ProjectUrl;
    public const string CreditSpanish = "Creado con Glyphoré — " + ProjectUrl;

    public static string Credit => Localization.English ? CreditEnglish : CreditSpanish;
}

using System.Text;

namespace Glyphore;

internal sealed record ThirdPartyLicenseEntry(
    string Name,
    string Version,
    string Copyright,
    string LicenseName,
    string LicenseResourceName,
    string? Notes = null);

internal static class ThirdPartyLicenses
{
    private const string Apache20Resource = "Glyphore.Licenses.Apache-2.0.txt";

    private static readonly ThirdPartyLicenseEntry[] Items =
    [
        new(
            Name: "Figgle",
            Version: "0.6.6",
            Copyright: "Copyright Drew Noakes and contributors",
            LicenseName: "Apache License 2.0",
            LicenseResourceName: Apache20Resource,
            Notes: "Glyphoré itself remains licensed under MIT. This Apache-2.0 notice applies to Figgle only, not to Glyphoré as a whole."),
        new(
            Name: "Figgle.Fonts",
            Version: "0.6.6",
            Copyright: "Copyright Drew Noakes and contributors",
            LicenseName: "Apache License 2.0",
            LicenseResourceName: Apache20Resource,
            Notes: "Package license notice. Individual FIGlet font data can contain additional author/copyright/license comments and should be audited before release use.")
    ];

    public static IReadOnlyList<ThirdPartyLicenseEntry> Entries => Items;

    public static string ReadFullLicense(ThirdPartyLicenseEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        using Stream? stream = typeof(ThirdPartyLicenses).Assembly.GetManifestResourceStream(entry.LicenseResourceName);
        if (stream is null)
        {
            string available = string.Join(", ", typeof(ThirdPartyLicenses).Assembly.GetManifestResourceNames());
            throw new InvalidOperationException(
                $"Embedded license resource '{entry.LicenseResourceName}' was not found. Available resources: {available}");
        }

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        string text = reader.ReadToEnd();
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException($"Embedded license resource '{entry.LicenseResourceName}' is empty.");

        return text;
    }

    public static void ValidateEmbeddedResources()
    {
        if (Items.Length == 0)
            throw new InvalidOperationException("The third-party license catalog is empty.");

        foreach (ThirdPartyLicenseEntry entry in Items)
        {
            string text = ReadFullLicense(entry);
            if (entry.LicenseName.Equals("Apache License 2.0", StringComparison.OrdinalIgnoreCase))
            {
                if (!text.Contains("Apache License", StringComparison.Ordinal) ||
                    !text.Contains("Version 2.0, January 2004", StringComparison.Ordinal) ||
                    !text.Contains("END OF TERMS AND CONDITIONS", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"The embedded Apache-2.0 text for {entry.Name} does not appear to be complete.");
                }
            }
        }
    }
}

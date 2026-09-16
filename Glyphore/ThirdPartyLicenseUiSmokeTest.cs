namespace Glyphore;

internal static class ThirdPartyLicenseUiSmokeTest
{
    public static void Run()
    {
        ThirdPartyLicenses.ValidateEmbeddedResources();

        ThirdPartyLicenseEntry first = ThirdPartyLicenses.Entries[0];

        using var about = new AboutForm(null)
        {
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000)
        };
        about.Show();
        Application.DoEvents();
        if (!about.IsHandleCreated || !about.HasThirdPartyLicensesAction)
            throw new InvalidOperationException("About did not expose the Third-Party Licenses action.");
        if (about.ContentPanel.Bounds != about.ClientRectangle)
            throw new InvalidOperationException("About content does not fill the native client area.");

        using Form licenseListBase = about.CreateThirdPartyLicensesWindow();
        if (licenseListBase is not ThirdPartyLicensesForm licenseList)
            throw new InvalidOperationException("The About license action did not create the expected license window.");
        licenseList.StartPosition = FormStartPosition.Manual;
        licenseList.Location = new Point(-32000, -32000);
        licenseList.Show();
        Application.DoEvents();
        if (!licenseList.IsHandleCreated || licenseList.RenderedEntryCount != ThirdPartyLicenses.Entries.Count)
            throw new InvalidOperationException("The third-party license list did not render every catalog entry.");
        if (licenseList.ContentPanel.Bounds != licenseList.ClientRectangle)
            throw new InvalidOperationException("Third-Party Licenses content does not fill the native client area.");

        using Form viewerBase = licenseList.CreateLicenseViewer(first);
        if (viewerBase is not FullLicenseForm viewer)
            throw new InvalidOperationException("The license list did not create the expected full-license viewer.");
        viewer.StartPosition = FormStartPosition.Manual;
        viewer.Location = new Point(-32000, -32000);
        viewer.Show();
        Application.DoEvents();

        string text = viewer.DisplayedLicenseText;
        if (viewer.ContentPanel.Bounds != viewer.ClientRectangle)
            throw new InvalidOperationException("Full License content does not fill the native client area.");

        if (!viewer.IsHandleCreated ||
            !text.Contains("Apache License", StringComparison.Ordinal) ||
            !text.Contains("Version 2.0, January 2004", StringComparison.Ordinal) ||
            !text.Contains("END OF TERMS AND CONDITIONS", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The full Apache-2.0 text was not available in the license viewer.");
        }

        if (!viewer.LicenseIsReadOnly || !viewer.LicenseHasNativeScrollBars || viewer.LicenseLineCount < 20)
            throw new InvalidOperationException("The full-license viewer is not configured as a read-only scrollable text viewport.");

        // Prove that the RichEdit scroll range reaches both ends of the embedded license. This
        // specifically guards against the previous custom-scrollbar bug where the last part of
        // a long license existed in Text but could not be reached by the viewport.
        viewer.ClientSize = new Size(650, 500);
        viewer.ScrollLicenseToEndForTest();
        Application.DoEvents();
        if (viewer.FirstVisibleLicenseLine <= 0 || !viewer.IsLicenseEndVisible)
            throw new InvalidOperationException("The full-license viewport cannot reach the end of the embedded license.");

        viewer.ScrollLicenseToStartForTest();
        Application.DoEvents();
        if (viewer.FirstVisibleLicenseLine != 0)
            throw new InvalidOperationException("The full-license viewport cannot return to the first line.");

        viewer.ClientSize = new Size(1000, 760);
        viewer.ScrollLicenseToEndForTest();
        Application.DoEvents();
        if (!viewer.IsLicenseEndVisible)
            throw new InvalidOperationException("The full-license scroll range became invalid after resize.");

        viewer.ScrollLicenseToStartForTest();
        Application.DoEvents();

        viewer.Close();
        licenseList.Close();
        about.Close();
        Application.DoEvents();
    }
}

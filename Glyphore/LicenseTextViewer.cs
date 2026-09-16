namespace Glyphore;

internal sealed class LicenseTextViewer : RichTextBox
{
    private const int EmGetFirstVisibleLine = 0x00CE;

    internal int FirstVisibleLine => IsHandleCreated
        ? SendMessage(Handle, EmGetFirstVisibleLine, IntPtr.Zero, IntPtr.Zero).ToInt32()
        : 0;

    internal bool IsDocumentEndVisible
    {
        get
        {
            if (!IsHandleCreated || TextLength == 0) return true;
            Point end = GetPositionFromCharIndex(Math.Max(0, TextLength - 1));
            return end.Y >= -Font.Height && end.Y <= ClientSize.Height;
        }
    }

    public LicenseTextViewer()
    {
        Multiline = true;
        ReadOnly = true;
        WordWrap = false;
        DetectUrls = false;
        HideSelection = false;
        ShortcutsEnabled = true;
        BorderStyle = BorderStyle.None;
        ScrollBars = RichTextBoxScrollBars.Both;
        TabStop = true;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Home)
        {
            SelectionLength = 0;
            SelectionStart = 0;
            ScrollToCaret();
            return true;
        }

        if (keyData == Keys.End)
        {
            SelectionLength = 0;
            SelectionStart = TextLength;
            ScrollToCaret();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}

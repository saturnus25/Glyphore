using System.Text;

namespace Glyphore;

internal sealed partial class MainForm
{
    private void CopyFrame()
    {
        try
        {
            string frame = CurrentAsciiFrame();
            Clipboard.SetText(frame);
            _status.Text = Localization.English ? "ASCII frame copied" : "Frame ASCII copiado";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Glyphoré", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveFrame()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = Localization.English ? "Text|*.txt" : "Texto|*.txt",
            FileName = "glyphore-frame.txt"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        File.WriteAllText(dialog.FileName, CurrentAsciiFrame(), new UTF8Encoding(false));
    }

    private string CurrentAsciiFrame()
    {
        if (_importView.Visible && _importFrames.Count > 0)
            return _importFrames[Math.Clamp(_importIndex, 0, _importFrames.Count - 1)];

        return _preview.CaptureCurrentAsciiFrame();
    }
}

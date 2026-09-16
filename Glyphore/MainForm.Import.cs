using System.Text;

namespace Glyphore;

internal sealed partial class MainForm
{
    private void ImportPs()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = Localization.English ? "PowerShell|*.ps1|All files|*.*" : "PowerShell|*.ps1|Todos|*.*"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var (frames, fps) = PowerShellImport.Parse(File.ReadAllText(dialog.FileName, Encoding.UTF8));
            EnterImportedMode(frames, fps, Path.GetFileName(dialog.FileName));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Import PowerShell", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void EnterImportedMode(List<string> frames, double fps, string source)
    {
        _importFrames = frames.Select(PowerShellImport.StripAnsi).ToList();
        _importFps = Math.Max(1, fps);
        _importIndex = -1;
        _importClock.Restart();
        _pauseButton.Text = Localization.Text("button.pause");
        _preview.Visible = false;
        _importView.Visible = true;
        _importView.BringToFront();

        int width = _importFrames.Max(frame => frame.Split('\n').Max(line => line.TrimEnd('\r').Length));
        int height = _importFrames.Max(frame => frame.Split('\n').Length);
        _status.Text = $"Imported · {source} · {_importFrames.Count} frames · {_importFps:0.##} FPS · {width}×{height}";
        UpdateImportedFrame();
    }

    private void ExitImportedMode()
    {
        if (!_importView.Visible) return;

        _importClock.Stop();
        _importView.Visible = false;
        _preview.Visible = true;
        _pauseButton.Text = _preview.Paused ? Localization.Text("button.resume") : Localization.Text("button.pause");
        _preview.BringToFront();
        _importFrames = [];
        _importIndex = -1;
    }

    private void UpdateImportedFrame()
    {
        if (!_importView.Visible || _importFrames.Count == 0) return;

        int index = (int)(_importClock.Elapsed.TotalSeconds * _importFps) % _importFrames.Count;
        if (index == _importIndex) return;

        _importIndex = index;
        int selection = _importView.SelectionStart;
        _importView.Text = _importFrames[index];
        _importView.SelectionStart = Math.Min(selection, _importView.TextLength);
        _status.Text = Localization.English
            ? $"Imported frames · {_importFrames.Count} · {_importFps:0.##} FPS · frame {index + 1}"
            : $"Frames importados · {_importFrames.Count} · {_importFps:0.##} FPS · frame {index + 1}";
    }
}

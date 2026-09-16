namespace Glyphore;

internal sealed partial class MainForm
{
    private void Benchmark()
    {
        try
        {
            double ms = _preview.BenchmarkGpu();
            string message = Localization.English
                ? $"GPU direct preview\n\n{_preview.GpuInfo}\n{_settings.Width}×{_settings.Height} ASCII over {_preview.Width}×{_preview.Height} px\n\n{ms:0.000} ms/frame GPU+driver\n~{1000.0 / ms:0} theoretical FPS (without target limit)"
                : $"GPU direct preview\n\n{_preview.GpuInfo}\n{_settings.Width}×{_settings.Height} ASCII sobre {_preview.Width}×{_preview.Height} px\n\n{ms:0.000} ms/frame GPU+driver\n~{1000.0 / ms:0} FPS teóricos (sin límite de target)";
            MessageBox.Show(message, "Benchmark", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Benchmark", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

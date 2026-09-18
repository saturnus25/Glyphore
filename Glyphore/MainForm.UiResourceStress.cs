namespace Glyphore;

internal sealed partial class MainForm
{
    internal void RunDynamicUiStressBatch(int startIteration, int count)
    {
        string[] effects = ParameterCatalog.Specific.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray();
        if (effects.Length == 0) throw new InvalidOperationException("No effect-specific controls are available for the UI stress test.");

        for (int offset = 0; offset < count; offset++)
        {
            int iteration = startIteration + offset;
            _settings.Effect = effects[iteration % effects.Length];
            RebuildPresets();

            int stopCount = 2 + iteration % 7;
            _settings.PaletteName = "Custom";
            _settings.PaletteStops = Enumerable.Range(0, stopCount)
                .Select(index => $"#{(iteration * 29 + index * 53) & 0xFFFFFF:X6}")
                .ToList();
            RebuildPaletteStops();

            if ((iteration % 20) == 0)
            {
                AddLayer();
                RemoveLayer();
            }

            if ((iteration & 7) == 0) Application.DoEvents();
        }

        Application.DoEvents();
    }
}

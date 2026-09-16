namespace Glyphore;

internal sealed partial class MainForm
{
    private void ApplyCurrentCharsetToSelectedLayers()
    {
        if (!_sceneReady) return;
        string charset = string.IsNullOrEmpty(_settings.Charset) ? " " : _settings.Charset;
        foreach (var layer in SelectedLayers())
            layer.SetCharset(_settings.CharsetName, charset);
    }

    private void ApplyCurrentCharsetToAllLayers()
    {
        if (!_sceneReady) return;
        string charset = string.IsNullOrEmpty(_settings.Charset) ? " " : _settings.Charset;
        foreach (var layer in _scene.Layers)
            layer.SetCharset(_settings.CharsetName, charset);

        // Keep the legacy/default scene fields useful for older readers and migrations.
        _scene.CharsetName = _settings.CharsetName;
        _scene.Charset = charset;
    }

    private void MarkCurrentCharsetCustom()
    {
        string charset = string.IsNullOrEmpty(_settings.Charset) ? " " : _settings.Charset;
        string? matchingPreset = _data.Charsets.FirstOrDefault(pair => pair.Value == charset).Key;
        string name = string.IsNullOrEmpty(matchingPreset) ? "Custom" : matchingPreset;
        _settings.CharsetName = name;

        _applying = true;
        try
        {
            if (!_charsetPreset.Items.Contains(name)) _charsetPreset.Items.Add(name);
            _charsetPreset.SelectedItem = name;
        }
        finally { _applying = false; }
    }
}

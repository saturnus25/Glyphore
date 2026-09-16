namespace Glyphore;

internal sealed partial class MainForm
{
    private GlyphoreScene _scene = new();
    private bool _sceneReady;
    private string? _scenePath;

    private readonly ListBox _layers = new();
    private readonly Panel _layerListHost = new();
    private readonly Panel _layerDetachedPlaceholder = new();
    private readonly GlyphButton _layerDetachButton = new();
    private GlyphoreWindow? _detachedLayersWindow;
    private readonly GlyphCheckBox _layerVisible = new();
    private readonly GlyphNumericUpDown _layerOpacity = new();
    private readonly SafeComboBox _layerBlend = new();
    private readonly GlyphCheckBox _respectLayerOrder = new();
    private readonly GlyphButton _sceneBackground = new();
    private readonly GlyphCheckBox _previewFollowsSceneBackground = new();


    private void SyncSceneBackgroundControl()
    {
        string value = string.IsNullOrWhiteSpace(_scene.BackgroundColor) ? "#000000" : _scene.BackgroundColor;
        _sceneBackground.Text = (Localization.English ? "Scene background" : "Fondo de escena") + $"   {value}";
        _tips.SetToolTip(_sceneBackground, Localization.English
            ? "Background baked only when raster export uses Scene Background. It remains a scene property even when preview sync is enabled."
            : "Fondo que se hornea al exportar con Fondo de escena. Sigue siendo una propiedad de la escena aunque la sincronización de preview esté activa.");
        SyncPreviewBackgroundFromScene();
    }

    private void SyncPreviewBackgroundFromScene()
    {
        if (!_previewFollowsSceneBackground.Checked) return;
        Color sceneColor;
        try { sceneColor = ColorTranslator.FromHtml(_scene.BackgroundColor); }
        catch { sceneColor = Color.Black; }
        _preview.PreviewBackgroundMode = PreviewBackgroundMode.Solid;
        _preview.PreviewBackgroundColor = sceneColor;
    }

    private void SetEditorPreviewBackground(Color color, PreviewBackgroundMode mode)
    {
        if (_previewFollowsSceneBackground.Checked)
            _previewFollowsSceneBackground.Checked = false;
        _preview.PreviewBackgroundMode = mode;
        _preview.PreviewBackgroundColor = color;
    }
}

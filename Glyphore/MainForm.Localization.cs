namespace Glyphore;

internal sealed partial class MainForm
{
    private void ApplyLanguage()
    {
        foreach (Control c in Controls) ApplyLanguageRecursive(c);
        _generalParams.Controls.Clear();
        foreach (var key in ParameterCatalog.General.Select(x=>x.Key)) _paramRows.Remove(key);
        foreach (var d in ParameterCatalog.General) AddParamRow(_generalParams,d);
        RebuildSpecific();
        RebuildPaletteStops();
        UpdateEffectTip();
        ApplyStaticTips();
        ApplySceneLanguage();
        _pauseButton.Text = (_importView.Visible ? !_importClock.IsRunning : _preview.Paused) ? Localization.Text("button.resume") : Localization.Text("button.pause");
    }

    private void ApplyLanguageRecursive(Control c)
    {
        if (c.Tag is string key && (key.StartsWith("group.") || key.StartsWith("button.") || key.StartsWith("check.") || key.StartsWith("label.")))
        {
            c.Text = Localization.Text(key);
            if (ReferenceEquals(c, _exportButton)) c.Text += "  (Ctrl+E)";
            string tip=Localization.Tip(key); if(!string.IsNullOrEmpty(tip)) _tips.SetToolTip(c,tip);
        }
        foreach (Control child in c.Controls) ApplyLanguageRecursive(child);
    }

    private void ApplyStaticTips()
    {
        bool english = Localization.English;
        _tips.SetToolTip(
            _layers,
            english ? "Select one or more layers to edit. The first item is the top layer." : "Selecciona una o varias capas para editar. La primera es la capa superior.");
        _tips.SetToolTip(
            _layerVisible,
            english ? "Shows or hides the selected layer(s)." : "Muestra u oculta las capas seleccionadas.");
        _tips.SetToolTip(
            _layerOpacity,
            english ? "Changes opacity for the selected layer(s)." : "Cambia la opacidad de las capas seleccionadas.");
        _tips.SetToolTip(
            _layerBlend,
            english ? "Changes blend mode for the selected layer(s)." : "Cambia el modo de mezcla de las capas seleccionadas.");
        _tips.SetToolTip(
            _respectLayerOrder,
            Localization.Tip("check.respectlayerorder"));
        _tips.SetToolTip(
            _effect,
            english ? "Choose the kind of animation you want to generate." : "Elige qué tipo de animación quieres generar.");
        _tips.SetToolTip(
            _preset,
            english ? "Choose a prepared setup for the current effect." : "Elige una configuración preparada para el efecto actual.");
        _tips.SetToolTip(
            _width,
            english ? "Changes how many characters are used in each row." : "Cambia cuántos caracteres hay en cada fila.");
        _tips.SetToolTip(
            _height,
            english ? "Changes how many rows of characters are used." : "Cambia cuántas filas de caracteres hay.");
        _tips.SetToolTip(
            _fps,
            english ? "Sets the target frames per second for the preview." : "Marca cuántos frames por segundo intenta mostrar la vista previa.");
        _tips.SetToolTip(
            _duration,
            english ? "Sets how long the exported animation lasts." : "Marca cuánto dura la animación al exportarla.");
        _tips.SetToolTip(
            _seed,
            english ? "Changes the random variation without changing the effect type." : "Cambia la variación aleatoria sin cambiar el tipo de efecto.");
        _tips.SetToolTip(
            _charsetPreset,
            english ? "Choose a prepared character ramp for the selected layer(s)." : "Elige una rampa de caracteres preparada para las capas seleccionadas.");
        _tips.SetToolTip(
            _charset,
            english
                ? "Characters on the left represent dark areas and characters on the right represent bright areas. Editing this applies to the selected layer(s)."
                : "Los caracteres de la izquierda representan zonas oscuras y los de la derecha zonas claras. Al editarla se aplica a las capas seleccionadas.");
        _tips.SetToolTip(
            _glyphSize,
            english
                ? "Changes only how tightly glyphs fill their preview cells. 115% is tuned to resemble a compact terminal such as PowerShell or cmd; exported text is unchanged."
                : "Cambia solo cuánto llena cada carácter su celda en la preview. 115% está ajustado para parecerse a una terminal compacta como PowerShell o cmd; el texto exportado no cambia.");
        _tips.SetToolTip(
            _invert,
            english ? "Swaps which characters are used for bright and dark areas." : "Intercambia qué caracteres se usan para zonas claras y oscuras.");
        _tips.SetToolTip(
            _palette,
            english ? "Choose the colors used from dark areas to bright areas." : "Elige los colores usados desde las zonas oscuras hasta las claras.");
        _tips.SetToolTip(
            _color,
            english ? "Turns color on or off without removing the ASCII art." : "Activa o desactiva el color sin quitar el arte ASCII.");
        _tips.SetToolTip(_exportCredit, Localization.Tip("check.credit"));
        _tips.SetToolTip(
            _exportButton,
            Localization.Tip("button.export") + (english ? " Shortcut: Ctrl+E." : " Atajo: Ctrl+E."));
        _tips.SetToolTip(
            _shape,
            english ? "Choose the shape used by Spinning Shapes." : "Elige la figura usada por Spinning Shapes.");
    }

    private void UpdateEffectTip()
    {
        _tips.SetToolTip(_effect, Localization.EffectHelp(_effect.Text));
    }
}

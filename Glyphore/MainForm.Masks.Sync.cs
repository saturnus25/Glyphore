namespace Glyphore;

internal sealed partial class MainForm
{
    private void SyncMaskControls()
    {
        var active = ActiveMask();
        bool previousApplying = _applying;
        _applying = true;
        try
        {
            _maskAddButton.Enabled = _sceneReady && _scene.ActiveLayer is not null;
            _maskRemoveButton.Enabled = active is not null;
            SetMaskControlsEnabled(active is not null);
            if (active is not { } pair) return;
            var mask = pair.Mask;
            _maskType.SelectedIndex = Math.Clamp((int)mask.Type, 0, _maskType.Items.Count - 1);
            _maskEnabled.Checked = mask.Enabled;
            _maskInvert.Checked = mask.Invert;
            _maskX.SetValue(mask.X);
            _maskY.SetValue(mask.Y);
            _maskWidth.SetValue(mask.Width);
            _maskHeight.SetValue(mask.Height);
            _maskRotation.SetValue(mask.Rotation);
            _maskFeather.SetValue(mask.Feather);
            _maskStrength.SetValue(mask.Strength);
            _maskGradientAngle.SetValue(mask.GradientAngle);
            _maskGradientSoftness.SetValue(mask.GradientSoftness);
            _maskNoiseScale.SetValue(mask.NoiseScale);
            _maskShapeAmount.SetValue(mask.ShapeAmount);
            _maskTriangleType.SelectedIndex = Math.Clamp((int)mask.TriangleType, 0, 5);
            _maskShapeSides.SetValue(mask.Type == SceneMaskType.Star ? mask.StarPoints : mask.PolygonSides);
            _maskRotationSnapping.Checked = _preview.MaskRotationSnapping;
            UpdateMaskTypeControlState(mask.Type, enabled: true);
        }
        finally { _applying = previousApplying; }
    }

    private void SyncMaskGeometryControls(SceneLayerMask mask)
    {
        bool previousApplying = _applying;
        _applying = true;
        try
        {
            _maskX.SetValue(mask.X);
            _maskY.SetValue(mask.Y);
            _maskWidth.SetValue(mask.Width);
            _maskHeight.SetValue(mask.Height);
            _maskRotation.SetValue(mask.Rotation);
        }
        finally { _applying = previousApplying; }
    }

    private void SetMaskControlsEnabled(bool enabled)
    {
        foreach (Control control in new Control[] { _maskType, _maskEnabled, _maskInvert, _maskX, _maskY, _maskWidth, _maskHeight, _maskRotation, _maskFeather, _maskStrength, _maskGradientAngle, _maskGradientSoftness, _maskNoiseScale, _maskShapeAmount, _maskShapeSides, _maskTriangleType })
            control.Enabled = enabled;
        if (enabled && ActiveMask() is { } pair) UpdateMaskTypeControlState(pair.Mask.Type, enabled: true);
    }

    private void UpdateMaskTypeControlState(SceneMaskType type, bool enabled)
    {
        bool gradient = type is SceneMaskType.LinearGradient or SceneMaskType.RadialGradient;
        _maskGradientSoftness.Enabled = enabled && gradient;
        _maskGradientAngle.Enabled = enabled && type == SceneMaskType.LinearGradient;
        _maskNoiseScale.Enabled = enabled && type == SceneMaskType.Noise;
        _maskTriangleType.Enabled = enabled && type == SceneMaskType.Triangle;
        _maskShapeAmount.Enabled = enabled && type is SceneMaskType.RoundedRectangle or SceneMaskType.Ring or SceneMaskType.Star;
        _maskShapeSides.Enabled = enabled && type is SceneMaskType.Hexagon or SceneMaskType.Star;
        _maskShapeSides.SetLabel(type == SceneMaskType.Star
            ? (Localization.English ? "Points" : "Puntas")
            : (Localization.English ? "Sides" : "Lados"));
        _maskShapeSides.SetDescription(type == SceneMaskType.Star
            ? (Localization.English ? "Number of star points (3–32)." : "Número de puntas de la estrella (3–32).")
            : (Localization.English ? "Number of sides of the regular polygon (3–32)." : "Número de lados del polígono regular (3–32)."));
        _maskShapeAmount.SetLabel(type switch
        {
            SceneMaskType.Ring => Localization.English ? "Inner radius" : "Radio interior",
            SceneMaskType.Star => Localization.English ? "Inner radius" : "Radio interior",
            _ => Localization.English ? "Corner roundness" : "Redondeo"
        });
        _maskShapeAmount.SetDescription(type switch
        {
            SceneMaskType.Ring => Localization.English ? "Controls the inner hole radius of the ring." : "Controla el radio del hueco interior del anillo.",
            SceneMaskType.Star => Localization.English ? "Controls how deep the star valleys cut toward the centre." : "Controla cuánto se hunden hacia el centro los entrantes de la estrella.",
            SceneMaskType.RoundedRectangle => Localization.English ? "Rounds the rectangle corners from square to pill-like." : "Redondea las esquinas desde cuadradas hasta una forma tipo píldora.",
            _ => Localization.English ? "Shape-specific amount." : "Cantidad específica de la forma."
        });
        _maskFeather.Enabled = enabled && !gradient;
    }

    private void ApplyMaskControls()
    {
        if (_applying) return;
        var active = ActiveMask();
        if (active is not { } pair) return;
        var mask = pair.Mask;
        mask.Type = (SceneMaskType)Math.Clamp(_maskType.SelectedIndex, 0, Enum.GetValues<SceneMaskType>().Length - 1);
        mask.Enabled = _maskEnabled.Checked;
        mask.Invert = _maskInvert.Checked;
        mask.X = _maskX.Value;
        mask.Y = _maskY.Value;
        mask.Width = _maskWidth.Value;
        mask.Height = _maskHeight.Value;
        mask.Rotation = _maskRotation.Value;
        mask.Feather = _maskFeather.Value;
        mask.Strength = _maskStrength.Value;
        mask.GradientAngle = _maskGradientAngle.Value;
        mask.GradientSoftness = _maskGradientSoftness.Value;
        mask.NoiseScale = _maskNoiseScale.Value;
        mask.ShapeAmount = _maskShapeAmount.Value;
        mask.TriangleType = (SceneTriangleType)Math.Clamp(_maskTriangleType.SelectedIndex, 0, 5);
        int shapeCount = (int)Math.Round(_maskShapeSides.Value);
        if (mask.Type == SceneMaskType.Star) mask.StarPoints = shapeCount;
        else if (mask.Type == SceneMaskType.Hexagon) mask.PolygonSides = shapeCount;
        mask.Clamp();
        UpdateMaskTypeControlState(mask.Type, enabled: true);
        pair.Layer.Touch();
        _preview.ActiveMaskId = mask.Id;
        PushSceneOnly();
    }

    private void HandlePreviewMaskEdited(SceneEffectLayer layer, SceneLayerMask mask, bool commit)
    {
        if (_activeMaskId != mask.Id) return;
        if (commit)
        {
            SyncMaskControls();
            // The preview control already touched the layer during live manipulation.
            // Push once on commit so history receives one stable edit instead of one entry per MouseMove.
            PushSceneOnly();
        }
        else
        {
            SyncMaskGeometryControls(mask);
            _preview.Invalidate();
        }
    }

    private void HandlePreviewMaskDelete(SceneEffectLayer layer, SceneLayerMask mask)
    {
        if (_activeMaskId == mask.Id) RemoveActiveMask();
    }
}

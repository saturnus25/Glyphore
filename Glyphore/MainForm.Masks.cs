namespace Glyphore;

internal sealed partial class MainForm
{
    private Guid? _activeMaskId;
    private readonly SafeComboBox _maskType = new();
    private readonly GlyphButton _maskAddButton = new();
    private readonly GlyphButton _maskRemoveButton = new();
    private readonly GlyphCheckBox _maskEnabled = new();
    private readonly GlyphCheckBox _maskInvert = new();
    private readonly GlyphCheckBox _maskSnapping = new();
    private readonly GlyphCheckBox _maskRotationSnapping = new();
    private readonly SafeComboBox _maskTriangleType = new();
    private readonly MaskSliderField _maskX = new("X", -2, 3, .5, 4);
    private readonly MaskSliderField _maskY = new("Y", -2, 3, .5, 4);
    private readonly MaskSliderField _maskWidth = new(Localization.English ? "Width" : "Ancho", .01, 4, .5, 4);
    private readonly MaskSliderField _maskHeight = new(Localization.English ? "Height" : "Alto", .01, 4, .5, 4);
    private readonly MaskSliderField _maskRotation = new(Localization.English ? "Rotation" : "Rotación", -180, 180, 0, 2);
    private readonly MaskSliderField _maskFeather = new("Feather", 0, 1, .03, 4);
    private readonly MaskSliderField _maskStrength = new(Localization.English ? "Strength" : "Fuerza", 0, 1, 1, 4);
    private readonly MaskSliderField _maskGradientAngle = new(Localization.English ? "Angle" : "Ángulo", -180, 180, 0, 2);
    private readonly MaskSliderField _maskGradientSoftness = new(Localization.English ? "Falloff" : "Caída", .001, 1, .5, 4);
    private readonly MaskSliderField _maskNoiseScale = new("Noise", .1, 128, 8, 3);
    private readonly MaskSliderField _maskShapeAmount = new(Localization.English ? "Shape" : "Forma", 0, .95, .35, 4);
    private readonly MaskSliderField _maskShapeSides = new(Localization.English ? "Sides / points" : "Lados / puntas", 3, 32, 6, 0);
    private static readonly Color MaskAccent = Color.FromArgb(158, 112, 255);
}

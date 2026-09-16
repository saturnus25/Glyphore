namespace Glyphore;

internal enum LayerBlendMode
{
    Normal,
    Additive
}

internal enum SceneMaskType
{
    Rectangle = 0,
    Ellipse = 1,
    LinearGradient = 2,
    RadialGradient = 3,
    Noise = 4,
    RoundedRectangle = 5,
    Diamond = 6,
    Ring = 7,
    Triangle = 8,
    // Kept at value 9 for scene compatibility. The editor now exposes this as a
    // configurable regular polygon instead of a hard-coded hexagon.
    Hexagon = 9,
    Star = 10
}

internal enum SceneTriangleType
{
    Equilateral = 0,
    Isosceles = 1,
    Right = 2,
    Scalene = 3,
    Acute = 4,
    Obtuse = 5
}

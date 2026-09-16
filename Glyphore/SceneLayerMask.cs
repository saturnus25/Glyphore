namespace Glyphore;

internal sealed class SceneLayerMask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Mask";
    public bool Enabled { get; set; } = true;
    public SceneMaskType Type { get; set; } = SceneMaskType.Rectangle;
    public bool Invert { get; set; }
    public double X { get; set; } = 0.5;
    public double Y { get; set; } = 0.5;
    public double Width { get; set; } = 0.5;
    public double Height { get; set; } = 0.5;
    public double Rotation { get; set; }
    public double Feather { get; set; } = 0.03;
    public double Strength { get; set; } = 1.0;
    public double GradientAngle { get; set; }
    public double GradientSoftness { get; set; } = 0.5;
    public double NoiseScale { get; set; } = 8.0;
    public int NoiseSeed { get; set; } = 1337;
    // Shape-specific amount: rounded-rectangle corner radius, ring inner radius,
    // or star inner radius depending on mask type.
    public double ShapeAmount { get; set; } = 0.35;
    public SceneTriangleType TriangleType { get; set; } = SceneTriangleType.Equilateral;
    public int PolygonSides { get; set; } = 6;
    public int StarPoints { get; set; } = 5;

    public SceneLayerMask Clone() => new()
    {
        Id = Id,
        Name = Name,
        Enabled = Enabled,
        Type = Type,
        Invert = Invert,
        X = X,
        Y = Y,
        Width = Width,
        Height = Height,
        Rotation = Rotation,
        Feather = Feather,
        Strength = Strength,
        GradientAngle = GradientAngle,
        GradientSoftness = GradientSoftness,
        NoiseScale = NoiseScale,
        NoiseSeed = NoiseSeed,
        ShapeAmount = ShapeAmount,
        TriangleType = TriangleType,
        PolygonSides = PolygonSides,
        StarPoints = StarPoints
    };

    public SceneLayerMask Duplicate(string name)
    {
        var clone = Clone();
        clone.Id = Guid.NewGuid();
        clone.Name = name;
        return clone;
    }

    public void Clamp()
    {
        X = Math.Clamp(X, -2.0, 3.0);
        Y = Math.Clamp(Y, -2.0, 3.0);
        // Avoid numerically degenerate editor geometry. One percent of layer-space is
        // still visually very flat, but it cannot collapse into effectively infinite lines.
        Width = Math.Clamp(Width, 0.01, 4.0);
        Height = Math.Clamp(Height, 0.01, 4.0);
        if (Type == SceneMaskType.Triangle)
        {
            // A triangle with an almost-zero axis turns into a numerically valid but visually
            // absurd near-infinite line. Keep extreme squash available while avoiding the
            // degenerate gizmo/SDF case (maximum 32:1 aspect ratio).
            const double maxAspect = 32.0;
            if (Width > Height * maxAspect) Height = Width / maxAspect;
            else if (Height > Width * maxAspect) Width = Height / maxAspect;
        }
        Rotation = Math.Clamp(Rotation, -180.0, 180.0);
        Feather = Math.Clamp(Feather, 0.0, 1.0);
        Strength = Math.Clamp(Strength, 0.0, 1.0);
        GradientAngle = Math.Clamp(GradientAngle, -180.0, 180.0);
        GradientSoftness = Math.Clamp(GradientSoftness, 0.001, 1.0);
        NoiseScale = Math.Clamp(NoiseScale, 0.1, 128.0);
        ShapeAmount = Math.Clamp(ShapeAmount, 0.0, 0.95);
        TriangleType = Enum.IsDefined(TriangleType) ? TriangleType : SceneTriangleType.Equilateral;
        PolygonSides = Math.Clamp(PolygonSides, 3, 32);
        StarPoints = Math.Clamp(StarPoints, 3, 32);
    }

    public override string ToString() => $"◇ {Name}";
}

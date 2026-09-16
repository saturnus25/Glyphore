namespace Glyphore;

internal sealed class SceneTransform
{
    public double Rotation { get; set; }
    public double PerspectiveX { get; set; }
    public double PerspectiveY { get; set; }

    public SceneTransform Clone() => new()
    {
        Rotation = Rotation,
        PerspectiveX = PerspectiveX,
        PerspectiveY = PerspectiveY
    };

    public void Clamp()
    {
        Rotation = Math.Clamp(Rotation, -180.0, 180.0);
        PerspectiveX = Math.Clamp(PerspectiveX, -1.5, 1.5);
        PerspectiveY = Math.Clamp(PerspectiveY, -1.5, 1.5);
    }
}

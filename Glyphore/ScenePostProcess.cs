namespace Glyphore;

internal sealed class ScenePostProcess
{
    public bool Enabled { get; set; }
    public double Exposure { get; set; }
    public double Contrast { get; set; } = 1.0;
    public double Saturation { get; set; } = 1.0;
    public double Bloom { get; set; }
    public double BloomRadius { get; set; } = 1.0;
    public double Vignette { get; set; }
    public double Scanlines { get; set; }
    public double Grain { get; set; }
    public double ChromaticAberration { get; set; }
    public double Posterize { get; set; }
    public double Threshold { get; set; }
    public double Blur { get; set; }
    public double Sharpen { get; set; }
    public double Pixelate { get; set; } = 1.0;
    public double Dither { get; set; }

    public ScenePostProcess Clone() => new()
    {
        Enabled = Enabled,
        Exposure = Exposure,
        Contrast = Contrast,
        Saturation = Saturation,
        Bloom = Bloom,
        BloomRadius = BloomRadius,
        Vignette = Vignette,
        Scanlines = Scanlines,
        Grain = Grain,
        ChromaticAberration = ChromaticAberration,
        Posterize = Posterize,
        Threshold = Threshold,
        Blur = Blur,
        Sharpen = Sharpen,
        Pixelate = Pixelate,
        Dither = Dither
    };

    public void Clamp()
    {
        Exposure = Math.Clamp(Exposure, -5.0, 5.0);
        Contrast = Math.Clamp(Contrast, 0.0, 4.0);
        Saturation = Math.Clamp(Saturation, 0.0, 4.0);
        Bloom = Math.Clamp(Bloom, 0.0, 3.0);
        BloomRadius = Math.Clamp(BloomRadius, 0.25, 4.0);
        Vignette = Math.Clamp(Vignette, 0.0, 2.0);
        Scanlines = Math.Clamp(Scanlines, 0.0, 1.0);
        Grain = Math.Clamp(Grain, 0.0, 1.0);
        ChromaticAberration = Math.Clamp(ChromaticAberration, 0.0, 4.0);
        Posterize = Math.Clamp(Posterize, 0.0, 32.0);
        Threshold = Math.Clamp(Threshold, 0.0, 1.0);
        Blur = Math.Clamp(Blur, 0.0, 1.0);
        Sharpen = Math.Clamp(Sharpen, 0.0, 2.0);
        Pixelate = Math.Clamp(Pixelate, 1.0, 8.0);
        Dither = Math.Clamp(Dither, 0.0, 1.0);
    }
}

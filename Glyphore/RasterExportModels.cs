namespace Glyphore;

internal enum RasterBackgroundMode
{
    SceneBackground,
    SolidColor,
    Transparent
}

internal sealed record RasterFrame(int Width, int Height, byte[] Rgba32);

internal sealed record RasterExportProfile(
    string Id,
    string DisplayName,
    string Extension,
    bool SupportsAlpha,
    string Description,
    bool RequiresFfmpeg);

internal sealed record RasterExportOptions(
    RasterExportProfile Profile,
    RasterBackgroundMode BackgroundMode,
    Color SolidColor);

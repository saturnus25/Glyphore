namespace Glyphore;

internal readonly record struct Camera3DSpec(
    string Effect,
    string YawKey,
    string PitchKey,
    string DistanceKey,
    double DefaultYaw,
    double DefaultPitch,
    double DefaultDistance,
    double MinPitch,
    double MaxPitch,
    double MinDistance,
    double MaxDistance,
    string? PanXKey = null,
    string? PanYKey = null,
    double DragYawScale = .45,
    double DragPitchScale = .35,
    double WheelStep = .35);

internal static class Camera3D
{
    private static readonly Dictionary<string, Camera3DSpec> Specs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Spinning Donut"] = new("Spinning Donut", "donut_yaw", "donut_pitch", "donut_camera", 0, 0, 4.2, -85, 85, 2.2, 10.0, "donut_pan_x", "donut_pan_y"),
        ["3D Shapes"] = new("3D Shapes", "shape3d_yaw", "shape3d_pitch", "shape3d_camera", 0, 0, 4.2, -85, 85, 2.2, 8.0, "shape3d_pan_x", "shape3d_pan_y"),
        ["3D Terrain"] = new("3D Terrain", "terrain_yaw", "terrain_pitch", "terrain_zoom", 0, 0, 1.0, -55, 55, .55, 3.2, "terrain_pan_x", "terrain_pan_y", .35, .28, .18),
        ["SDF Lab"] = new("SDF Lab", "sdf_yaw", "sdf_pitch", "sdf_depth", 0, -8, 5.2, -75, 75, 1.8, 24.0, "sdf_pan_x", "sdf_pan_y"),
        ["Warp Grid 3D"] = new("Warp Grid 3D", "warpgrid_yaw", "warpgrid_pitch", "warpgrid_camera", 0, 0, 1.0, -55, 55, .5, 3.5, "warpgrid_pan_x", "warpgrid_pan_y", .35, .28, .18),
        ["Raymarch Lab"] = new("Raymarch Lab", "raymarch_yaw", "raymarch_pitch", "raymarch_camera", 0, 8, 4.2, -80, 80, 1.8, 9.0, "raymarch_pan_x", "raymarch_pan_y")
    };

    public static bool TryGetSpec(string effect, out Camera3DSpec spec) => Specs.TryGetValue(effect, out spec);

    public static double MinimumDistance(in Camera3DSpec spec, EffectSettings settings)
    {
        if (spec.Effect.Equals("SDF Lab", StringComparison.OrdinalIgnoreCase))
        {
            double copies = Math.Clamp(Math.Round(settings.Get("sdf_repeat")), 0, 4);
            return Math.Max(spec.MinDistance, copies * Math.Max(1.15, settings.Get("sdf_spacing")) + 1.15);
        }

        return spec.MinDistance;
    }

    public static double WrapYaw(double yaw)
    {
        while (yaw > 180) yaw -= 360;
        while (yaw < -180) yaw += 360;
        return yaw;
    }

    public static void Reset(EffectSettings settings, in Camera3DSpec spec)
    {
        settings.Set(spec.YawKey, spec.DefaultYaw);
        settings.Set(spec.PitchKey, spec.DefaultPitch);
        settings.Set(spec.DistanceKey, Math.Max(spec.DefaultDistance, MinimumDistance(spec, settings)));
        if (spec.PanXKey is not null) settings.Set(spec.PanXKey, 0);
        if (spec.PanYKey is not null) settings.Set(spec.PanYKey, 0);
    }
}

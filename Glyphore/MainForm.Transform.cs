namespace Glyphore;

internal sealed partial class MainForm
{
    private readonly Dictionary<string, ParameterRow> _transformRows = new(StringComparer.OrdinalIgnoreCase);

    private static readonly ParamDesc[] GlobalTransformParameters =
    [
        new("Rotación global", "scene_rotation", -180, 180, 0, 1, "Gira visualmente toda la composición final."),
        new("Perspectiva X", "scene_perspective_x", -1.5, 1.5, 0, 2, "Inclina toda la escena horizontalmente con un efecto de perspectiva."),
        new("Perspectiva Y", "scene_perspective_y", -1.5, 1.5, 0, 2, "Inclina toda la escena verticalmente con un efecto de perspectiva.")
    ];

    private ThemedGroupBox BuildGlobalTransformGroup()
    {
        var group = Group(Localization.Text("group.transform"), GlobalTransformParameters.Length * 34 + 62);
        group.Tag = "group.transform";

        var reset = Btn(Localization.Text("button.transformreset"), (_, _) => ResetGlobalTransform());
        reset.Tag = "button.transformreset";
        reset.SetBounds(270, 23, 135, 28);
        group.Controls.Add(reset);

        int y = 57;
        foreach (var desc in GlobalTransformParameters)
        {
            var row = new ParameterRow(desc, TransformValue(desc.Key))
            {
                Left = 10,
                Top = y,
                Width = 395
            };
            string key = desc.Key;
            row.ValueChanged += value =>
            {
                if (_applying || !_sceneReady) return;
                SetTransformValue(key, value);
                PushSceneOnly();
            };
            _transformRows[key] = row;
            group.Controls.Add(row);
            y += 34;
        }

        return group;
    }

    private double TransformValue(string key)
    {
        var t = _scene.Transform;
        return key switch
        {
            "scene_rotation" => t.Rotation,
            "scene_perspective_x" => t.PerspectiveX,
            "scene_perspective_y" => t.PerspectiveY,
            _ => 0
        };
    }

    private void SetTransformValue(string key, double value)
    {
        var t = _scene.Transform;
        switch (key)
        {
            case "scene_rotation": t.Rotation = value; break;
            case "scene_perspective_x": t.PerspectiveX = value; break;
            case "scene_perspective_y": t.PerspectiveY = value; break;
        }
        t.Clamp();
    }

    private void SyncGlobalTransformControls()
    {
        if (!_sceneReady) return;
        _applying = true;
        try
        {
            foreach (var desc in GlobalTransformParameters)
                if (_transformRows.TryGetValue(desc.Key, out var row)) row.SetValue(TransformValue(desc.Key));
        }
        finally { _applying = false; }
    }

    private void ResetGlobalTransform()
    {
        if (!_sceneReady) return;
        _scene.Transform = new SceneTransform();
        SyncGlobalTransformControls();
        PushSceneOnly();
    }
}

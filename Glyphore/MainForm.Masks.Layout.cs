namespace Glyphore;

internal sealed partial class MainForm
{
    private void BuildMaskControls(Control parent, int y)
    {
        var separator = new Label
        {
            Left = 10,
            Top = y,
            Width = 395,
            Height = 23,
            ForeColor = MaskAccent,
            Font = new Font(Font, FontStyle.Bold),
            Text = Localization.English ? "Masks / Modifiers" : "Masks / Modificadores"
        };
        parent.Controls.Add(separator);

        _maskAddButton.Text = Localization.Text("button.maskadd");
        _maskAddButton.Tag = "button.maskadd";
        Theme.Button(_maskAddButton);
        _maskAddButton.SetBounds(10, y + 27, 194, 27);
        _maskAddButton.Click += (sender, _) => ShowAddMaskMenu((Control)sender!);

        _maskRemoveButton.Text = Localization.Text("button.maskremove");
        _maskRemoveButton.Tag = "button.maskremove";
        Theme.Button(_maskRemoveButton);
        _maskRemoveButton.SetBounds(211, y + 27, 194, 27);
        _maskRemoveButton.Click += (_, _) => RemoveActiveMask();
        parent.Controls.AddRange([_maskAddButton, _maskRemoveButton]);

        SetupCombo(_maskType);
        PopulateMaskTypeChoices();
        _maskType.SetBounds(10, y + 60, 150, 28);
        _maskType.SelectedIndexChanged += (_, _) => ApplyMaskControls();
        parent.Controls.Add(_maskType);

        _maskEnabled.Text = Localization.English ? "Enabled" : "Activa";
        Theme.CheckBox(_maskEnabled);
        _maskEnabled.SetBounds(168, y + 61, 70, 25);
        _maskEnabled.CheckedChanged += (_, _) => ApplyMaskControls();
        parent.Controls.Add(_maskEnabled);

        _maskInvert.Text = Localization.English ? "Invert" : "Invertir";
        Theme.CheckBox(_maskInvert);
        _maskInvert.SetBounds(243, y + 61, 75, 25);
        _maskInvert.CheckedChanged += (_, _) => ApplyMaskControls();
        parent.Controls.Add(_maskInvert);

        _maskSnapping.Text = Localization.English ? "Pos. snap" : "Snap pos.";
        Theme.CheckBox(_maskSnapping);
        _maskSnapping.Checked = true;
        _maskSnapping.SetBounds(323, y + 61, 82, 25);
        _maskSnapping.CheckedChanged += (_, _) =>
        {
            if (_applying) return;
            _preview.MaskSnapping = _maskSnapping.Checked;
            _preview.Invalidate();
        };
        parent.Controls.Add(_maskSnapping);

        var triangleLabel = new Label
        {
            Left = 10, Top = y + 94, Width = 74, Height = 24,
            Text = Localization.English ? "Triangle" : "Triángulo",
            ForeColor = Theme.Muted, TextAlign = ContentAlignment.MiddleLeft
        };
        SetupCombo(_maskTriangleType);
        _maskTriangleType.Items.AddRange(Localization.English
            ? ["Equilateral", "Isosceles", "Right", "Scalene", "Acute", "Obtuse"]
            : ["Equilátero", "Isósceles", "Rectángulo", "Escaleno", "Acutángulo", "Obtusángulo"]);
        _maskTriangleType.SetBounds(84, y + 92, 120, 28);
        _maskTriangleType.SelectedIndex = 0;
        _maskTriangleType.SelectedIndexChanged += (_, _) => ApplyMaskControls();

        _maskRotationSnapping.Text = Localization.English ? "Rot. snap 15°" : "Snap rot. 15°";
        Theme.CheckBox(_maskRotationSnapping);
        _maskRotationSnapping.Checked = true;
        _maskRotationSnapping.SetBounds(211, y + 94, 194, 25);
        _maskRotationSnapping.CheckedChanged += (_, _) =>
        {
            if (_applying) return;
            _preview.MaskRotationSnapping = _maskRotationSnapping.Checked;
            _preview.Invalidate();
        };
        parent.Controls.AddRange([triangleLabel, _maskTriangleType, _maskRotationSnapping]);

        AddMaskSliderPair(parent, _maskX, _maskY, y + 126);
        AddMaskSliderPair(parent, _maskWidth, _maskHeight, y + 204);
        AddMaskSliderPair(parent, _maskRotation, _maskFeather, y + 282);
        AddMaskSliderPair(parent, _maskStrength, _maskGradientAngle, y + 360);
        AddMaskSliderPair(parent, _maskGradientSoftness, _maskNoiseScale, y + 438);
        AddMaskSliderPair(parent, _maskShapeAmount, _maskShapeSides, y + 516);

        foreach (var field in new[] { _maskX, _maskY, _maskWidth, _maskHeight, _maskRotation, _maskFeather, _maskStrength, _maskGradientAngle, _maskGradientSoftness, _maskNoiseScale, _maskShapeAmount, _maskShapeSides })
            field.ValueChanged += (_, _) => ApplyMaskControls();

        _tips.SetToolTip(_maskAddButton, Localization.English ? "Adds a mask as a child of the active layer." : "Añade una máscara como hija de la capa activa.");
        _tips.SetToolTip(_maskRemoveButton, Localization.English ? "Removes the selected mask from its parent layer." : "Quita la máscara seleccionada de su capa padre.");
        _tips.SetToolTip(_maskSnapping, Localization.English ? "Snap the active mask to layer/scene center and edges. Hold Ctrl or Alt while dragging to ignore snapping." : "Ajusta la mask al centro y bordes. Mantén Ctrl o Alt al arrastrar para ignorarlo.");
        _tips.SetToolTip(_maskRotationSnapping, Localization.English ? "Gently snaps rotation within 3° of each 15° increment. Snap points are shown while rotating; Ctrl or Alt temporarily ignores them." : "Ajusta suavemente la rotación a menos de 3° de cada múltiplo de 15°. Los puntos de snap se muestran al rotar; Ctrl o Alt los ignoran temporalmente.");
        _tips.SetToolTip(_maskTriangleType, Localization.English ? "Triangle geometry preset. Rotation still controls its direction." : "Geometría del triángulo. La rotación sigue controlando su orientación.");
        ApplyMaskSliderDescriptions();
        _maskGradientSoftness.SetToolTip(_tips, Localization.English ? "Gradient falloff profile. 0 keeps opacity longer, 0.5 is linear, and 1 fades strongly from the opaque origin." : "Perfil de caída del gradiente. 0 conserva la opacidad más tiempo, 0,5 es lineal y 1 desvanece con fuerza desde el origen opaco.");
        _maskAddButton.Enabled = false;
        _maskRemoveButton.Enabled = false;
        SetMaskControlsEnabled(false);
    }

    private static void AddMaskSliderPair(Control parent, MaskSliderField left, MaskSliderField right, int y)
    {
        left.SetBounds(10, y, 194, 76);
        right.SetBounds(211, y, 194, 76);
        parent.Controls.AddRange([left, right]);
    }

    private void PopulateMaskTypeChoices()
    {
        int selected = _maskType.SelectedIndex;
        _maskType.Items.Clear();
        _maskType.Items.AddRange(Localization.English
            ? new object[] { "Rectangle", "Ellipse / Circle", "Linear Gradient", "Radial Gradient", "Noise", "Rounded Rectangle", "Diamond", "Ring", "Triangle", "Polygon", "Star" }
            : new object[] { "Rectángulo", "Elipse / Círculo", "Gradiente lineal", "Gradiente radial", "Ruido", "Rectángulo redondeado", "Rombo", "Anillo", "Triángulo", "Polígono", "Estrella" });
        if (_maskType.Items.Count > 0)
            _maskType.SelectedIndex = Math.Clamp(selected < 0 ? 0 : selected, 0, _maskType.Items.Count - 1);
    }

    private void ApplyMaskLanguage()
    {
        bool previousApplying = _applying;
        _applying = true;
        try
        {
            PopulateMaskTypeChoices();
            _maskEnabled.Text = Localization.English ? "Enabled" : "Activa";
            _maskInvert.Text = Localization.English ? "Invert" : "Invertir";
            _maskSnapping.Text = Localization.English ? "Pos. snap" : "Snap pos.";
            _maskRotationSnapping.Text = Localization.English ? "Rot. snap 15°" : "Snap rot. 15°";
            _maskWidth.SetLabel(Localization.English ? "Width" : "Ancho");
            _maskHeight.SetLabel(Localization.English ? "Height" : "Alto");
            _maskRotation.SetLabel(Localization.English ? "Rotation" : "Rotación");
            _maskStrength.SetLabel(Localization.English ? "Strength" : "Fuerza");
            _maskGradientAngle.SetLabel(Localization.English ? "Angle" : "Ángulo");
            _maskGradientSoftness.SetLabel(Localization.English ? "Falloff" : "Caída");
            _maskShapeAmount.SetLabel(Localization.English ? "Shape" : "Forma");
            int triangleSelection = Math.Clamp(_maskTriangleType.SelectedIndex < 0 ? 0 : _maskTriangleType.SelectedIndex, 0, 5);
            _maskTriangleType.Items.Clear();
            _maskTriangleType.Items.AddRange(Localization.English
                ? ["Equilateral", "Isosceles", "Right", "Scalene", "Acute", "Obtuse"]
                : ["Equilátero", "Isósceles", "Rectángulo", "Escaleno", "Acutángulo", "Obtusángulo"]);
            _maskTriangleType.SelectedIndex = triangleSelection;
            ApplyMaskSliderDescriptions();
        }
        finally { _applying = previousApplying; }

        if (ActiveMask() is { } active) UpdateMaskTypeControlState(active.Mask.Type, enabled: true);
    }

    private void ApplyMaskSliderDescriptions()
    {
        if (Localization.English)
        {
            _maskX.SetDescription("Horizontal position in parent-layer space.");
            _maskY.SetDescription("Vertical position in parent-layer space.");
            _maskWidth.SetDescription("Mask region width relative to the layer.");
            _maskHeight.SetDescription("Mask region height relative to the layer.");
            _maskRotation.SetDescription("Rotates the mask around its own centre.");
            _maskFeather.SetDescription("Softens shape/noise edges; gradients use Falloff.");
            _maskStrength.SetDescription("0 = no effect · 1 = full mask effect.");
            _maskGradientAngle.SetDescription("Direction of the linear opacity gradient.");
            _maskGradientSoftness.SetDescription("0 = late fall · 0.5 = linear · 1 = early fall.");
            _maskNoiseScale.SetDescription("Frequency/scale of the procedural noise pattern.");
            _maskShapeAmount.SetDescription("Rounded corners, ring/star inner radius, depending on mask type.");
            _maskShapeSides.SetDescription("Polygon side count or star point count.");
        }
        else
        {
            _maskX.SetDescription("Posición horizontal relativa a la capa padre.");
            _maskY.SetDescription("Posición vertical relativa a la capa padre.");
            _maskWidth.SetDescription("Anchura de la región de la mask en la capa.");
            _maskHeight.SetDescription("Altura de la región de la mask en la capa.");
            _maskRotation.SetDescription("Gira la mask alrededor de su propio centro.");
            _maskFeather.SetDescription("Suaviza bordes de formas/ruido; gradientes usan Caída.");
            _maskStrength.SetDescription("0 = sin efecto · 1 = efecto completo de la mask.");
            _maskGradientAngle.SetDescription("Dirección del gradiente lineal de opacidad.");
            _maskGradientSoftness.SetDescription("0 = caída tardía · 0,5 = lineal · 1 = caída temprana.");
            _maskNoiseScale.SetDescription("Frecuencia/escala del patrón procedural de ruido.");
            _maskShapeAmount.SetDescription("Redondeo o radio interior del anillo/estrella según la forma.");
            _maskShapeSides.SetDescription("Número de lados del polígono o puntas de la estrella.");
        }
    }
}

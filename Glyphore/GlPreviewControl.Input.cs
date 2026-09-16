namespace Glyphore;

internal sealed partial class GlPreviewControl
{
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_ERASEBKGND) { m.Result = new IntPtr(1); return; }
        base.WndProc(ref m);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (TryBeginMaskEdit(e)) return;
        if (e.Button != MouseButtons.Left || !Camera3D.TryGetSpec(_settings.Effect, out var spec)) return;

        _cameraOrbiting = true;
        _cameraOrbitSpec = spec;
        _cameraOrbitLast = e.Location;
        Capture = true;
        Cursor = Cursors.SizeAll;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (UpdateMaskEdit(e)) return;
        if (!_cameraOrbiting) return;

        if (!_settings.Effect.Equals(_cameraOrbitSpec.Effect, StringComparison.OrdinalIgnoreCase))
        {
            EndCameraOrbit();
            return;
        }

        int dx = e.X - _cameraOrbitLast.X;
        int dy = e.Y - _cameraOrbitLast.Y;
        _cameraOrbitLast = e.Location;

        double yaw = Camera3D.WrapYaw(_settings.Get(_cameraOrbitSpec.YawKey) + dx * _cameraOrbitSpec.DragYawScale);
        double pitch = Math.Clamp(
            _settings.Get(_cameraOrbitSpec.PitchKey) - dy * _cameraOrbitSpec.DragPitchScale,
            _cameraOrbitSpec.MinPitch,
            _cameraOrbitSpec.MaxPitch);
        double distance = Math.Clamp(
            _settings.Get(_cameraOrbitSpec.DistanceKey),
            Camera3D.MinimumDistance(_cameraOrbitSpec, _settings),
            _cameraOrbitSpec.MaxDistance);

        _settings.Set(_cameraOrbitSpec.YawKey, yaw);
        _settings.Set(_cameraOrbitSpec.PitchKey, pitch);
        _settings.Set(_cameraOrbitSpec.DistanceKey, distance);
        CameraChanged?.Invoke(_cameraOrbitSpec.Effect, yaw, pitch, distance);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left && _maskDragMode != MaskDragMode.None) { EndMaskEdit(true); return; }
        if (e.Button == MouseButtons.Left && _cameraOrbiting) EndCameraOrbit();
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);
        if (_maskDragMode != MaskDragMode.None && !Capture) EndMaskEdit(true);
        if (_cameraOrbiting && !Capture) EndCameraOrbit();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (Camera3D.TryGetSpec(_settings.Effect, out var spec))
        {
            double minDistance = Camera3D.MinimumDistance(spec, _settings);
            double distance = Math.Clamp(
                _settings.Get(spec.DistanceKey) - Math.Sign(e.Delta) * spec.WheelStep,
                minDistance,
                spec.MaxDistance);
            _settings.Set(spec.DistanceKey, distance);
            CameraChanged?.Invoke(spec.Effect, _settings.Get(spec.YawKey), _settings.Get(spec.PitchKey), distance);
            Invalidate();
            return;
        }

        base.OnMouseWheel(e);
    }

    private void EndCameraOrbit()
    {
        _cameraOrbiting = false;
        Capture = false;
        Cursor = Cursors.Default;
    }
}

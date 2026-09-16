using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Glyphore;


internal enum PreviewViewMode
{
    Fit = 0,
    Fill = 1,
    Stretch = 2
}

internal enum PreviewBackgroundMode
{
    Solid = 0,
    Checkerboard = 1
}

internal sealed partial class GlPreviewControl : Control
{
    private const int WM_ERASEBKGND = 0x0014;
    private const int CS_OWNDC = 0x0020;
    private const int WGL_CONTEXT_MAJOR_VERSION_ARB = 0x2091;
    private const int WGL_CONTEXT_MINOR_VERSION_ARB = 0x2092;
    private const int WGL_CONTEXT_PROFILE_MASK_ARB = 0x9126;
    private const int WGL_CONTEXT_CORE_PROFILE_BIT_ARB = 0x00000001;
    private const uint GL_RENDERER = 0x1F01;
    private const uint GL_VERSION = 0x1F02;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr WglCreateContextAttribs(IntPtr hdc, IntPtr share, int[] attribs);
    [DllImport("opengl32.dll")] private static extern IntPtr wglGetProcAddress(string name);

    private IntPtr _dc;
    private IntPtr _rc;
    private uint _previewProgram;
    private uint _captureProgram;
    private uint _vao;
    private uint _atlas;
    private uint _intensityTex;
    private uint _intensityFbo;
    private uint _colorTex;
    private uint _colorFbo;
    private uint _glyphSelectTex;
    private uint _glyphSelectFbo;
    private uint _glyphMapTex;
    private uint _rasterExportTex;
    private uint _rasterExportFbo;
    private int _rasterExportW;
    private int _rasterExportH;
    private int _intensityW;
    private int _intensityH;
    private int _atlasCols = 1, _atlasRows = 1;
    private int _atlasGlyphCount = 1;
    private string _atlasRamp = "";
    private string _sceneGlyphSignature = "";
    private readonly Dictionary<Guid, (int Slot, int Count)> _layerGlyphRows = new();
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly Stopwatch _fpsClock = Stopwatch.StartNew();
    private int _frames;
    private double _actualFps;
    private System.Threading.CancellationTokenSource? _schedulerCts;
    private System.Threading.Thread? _schedulerThread;
    private int _paintQueued;
    private static readonly Dictionary<(uint Program, string Name), int> UniformLocationCache = new();
    private volatile bool _loaded;
    private string _gpuInfo = "OpenGL no inicializado";
    private EffectSettings _settings = new();
    private GlyphoreScene? _scene;
    private readonly Dictionary<Guid, LayerClockState> _layerClocks = new();
    private readonly Dictionary<Guid, LayerRenderSettings> _layerRenderSettings = new();
    private double _lastSceneTimelineTime;
    private bool _sceneClockInitialized;
    private int _targetFps = 30;
    private volatile bool _paused;
    private double _pauseAt;
    private double _pausedAccum;
    // Preview clocks are integrated instead of recomputed as elapsed*slider.
    // This prevents live speed/time-frequency edits from teleporting stateful-looking effects.
    private double _animationTime;
    private double _temporalTime;
    private double _lastTimelineTime;
    private bool _previewClockInitialized;
    private bool _cameraOrbiting;
    private Point _cameraOrbitLast;
    private Camera3DSpec _cameraOrbitSpec;
    private Guid? _activeMaskId;
    private bool _maskSnapping = true;
    private bool _maskRotationSnapping = true;
    private bool _showInactiveMasks;
    private float _maskSnapGuideX = -1f;
    private float _maskSnapGuideY = -1f;


    private sealed class LayerClockState
    {
        public double Animation;
        public double Temporal;
    }

    private sealed class LayerRenderSettings
    {
        public int CommonRevision = int.MinValue;
        public int LayerRevision = int.MinValue;
        public EffectSettings Settings = new();
    }


    private Color _previewBackgroundColor = Color.Black;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color PreviewBackgroundColor
    {
        get => _previewBackgroundColor;
        set
        {
            if (_previewBackgroundColor == value) return;
            _previewBackgroundColor = value;
            BackColor = value;
            Invalidate();
        }
    }

    private PreviewViewMode _previewViewMode = PreviewViewMode.Stretch;
    private PreviewBackgroundMode _previewBackgroundMode = PreviewBackgroundMode.Solid;
    private double _previewZoom = 1.0;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PreviewBackgroundMode PreviewBackgroundMode
    {
        get => _previewBackgroundMode;
        set { if (_previewBackgroundMode == value) return; _previewBackgroundMode = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PreviewViewMode PreviewViewMode
    {
        get => _previewViewMode;
        set
        {
            if (_previewViewMode == value) return;
            _previewViewMode = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double PreviewZoom
    {
        get => _previewZoom;
        set
        {
            double next = Math.Clamp(double.IsFinite(value) ? value : 1.0, 0.10, 4.0);
            if (Math.Abs(_previewZoom - next) < 0.0001) return;
            _previewZoom = next;
            Invalidate();
        }
    }

    public event Action<double, double, string>? FrameStats;
    public event Action<string, double, double, double>? CameraChanged;
    public event Action<SceneEffectLayer, SceneLayerMask, bool>? MaskEdited;
    public event Action<SceneEffectLayer, SceneLayerMask>? MaskDeleteRequested;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Guid? ActiveMaskId
    {
        get => _activeMaskId;
        set { if (_activeMaskId == value) return; _activeMaskId = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool MaskSnapping
    {
        get => _maskSnapping;
        set { _maskSnapping = value; if (!value) { _maskSnapGuideX = -1f; _maskSnapGuideY = -1f; } Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool MaskRotationSnapping
    {
        get => _maskRotationSnapping;
        set { _maskRotationSnapping = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowInactiveMasks
    {
        get => _showInactiveMasks;
        set { _showInactiveMasks = value; Invalidate(); }
    }
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public EffectSettings Settings
    {
        get => _settings;
        set
        {
            _settings = value;
            SyncAtlasIfNeeded();
            // While running, the precision scheduler owns repaint cadence.
            // This prevents slider ValueChanged events from bypassing TargetFps.
            if (_paused) Invalidate();
        }
    }


    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public GlyphoreScene? Scene
    {
        get => _scene;
        set
        {
            if (!ReferenceEquals(_scene, value))
            {
                _layerClocks.Clear();
                _layerRenderSettings.Clear();
                _lastSceneTimelineTime = 0;
                _sceneClockInitialized = false;
            }
            _scene = value;
            if (_paused) Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int TargetFps { get => System.Threading.Volatile.Read(ref _targetFps); set => System.Threading.Volatile.Write(ref _targetFps, Math.Max(1, value)); }
    public string GpuInfo => _gpuInfo;
    public double CurrentTimeSeconds => _paused ? _pauseAt : _clock.Elapsed.TotalSeconds - _pausedAccum;
    public bool Paused => _paused;
    public void TogglePause() { if (!_paused) { _pauseAt = _clock.Elapsed.TotalSeconds - _pausedAccum; _paused = true; } else { _pausedAccum = _clock.Elapsed.TotalSeconds - _pauseAt; _paused = false; } Invalidate(); }
    public void RestartAnimation()
    {
        _clock.Restart(); _paused = false; _pauseAt = 0; _pausedAccum = 0;
        _animationTime = 0; _temporalTime = 0; _lastTimelineTime = 0; _previewClockInitialized = false;
        _layerClocks.Clear(); _lastSceneTimelineTime = 0; _sceneClockInitialized = false;
        Invalidate();
    }

    public void DuplicateLayerClock(Guid sourceLayerId, Guid targetLayerId)
    {
        if (!_layerClocks.TryGetValue(sourceLayerId, out var sourceClock)) return;
        _layerClocks[targetLayerId] = new LayerClockState
        {
            Animation = sourceClock.Animation,
            Temporal = sourceClock.Temporal
        };
    }

    public void ForgetLayerRuntime(Guid layerId)
    {
        _layerClocks.Remove(layerId);
        _layerRenderSettings.Remove(layerId);
    }

    public GlPreviewControl()
    {
        SetStyle(ControlStyles.Opaque | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Selectable, true);
        TabStop = true;
        BackColor = Color.Black;
    }

    protected override CreateParams CreateParams
    {
        get { var cp = base.CreateParams; cp.ClassStyle |= CS_OWNDC; return cp; }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        try { InitializeOpenGl(); StartScheduler(); }
        catch (Exception ex) { _gpuInfo = "OpenGL error: " + ex.Message; Invalidate(); }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        _loaded = false;
        StopScheduler();

        if (_dc != IntPtr.Zero && _rc != IntPtr.Zero)
        {
            NativeGl.wglMakeCurrent(_dc, _rc);

            if (_previewProgram != 0) { NativeGl.DeleteProgram(_previewProgram); _previewProgram = 0; }
            if (_captureProgram != 0) { NativeGl.DeleteProgram(_captureProgram); _captureProgram = 0; }
            if (_atlas != 0) { NativeGl.DeleteTextures(1, ref _atlas); _atlas = 0; }
            if (_intensityFbo != 0) { NativeGl.DeleteFramebuffers(1, ref _intensityFbo); _intensityFbo = 0; }
            if (_intensityTex != 0) { NativeGl.DeleteTextures(1, ref _intensityTex); _intensityTex = 0; }
            if (_colorFbo != 0) { NativeGl.DeleteFramebuffers(1, ref _colorFbo); _colorFbo = 0; }
            if (_colorTex != 0) { NativeGl.DeleteTextures(1, ref _colorTex); _colorTex = 0; }
            if (_glyphSelectFbo != 0) { NativeGl.DeleteFramebuffers(1, ref _glyphSelectFbo); _glyphSelectFbo = 0; }
            if (_glyphSelectTex != 0) { NativeGl.DeleteTextures(1, ref _glyphSelectTex); _glyphSelectTex = 0; }
            if (_glyphMapTex != 0) { NativeGl.DeleteTextures(1, ref _glyphMapTex); _glyphMapTex = 0; }
            if (_rasterExportFbo != 0) { NativeGl.DeleteFramebuffers(1, ref _rasterExportFbo); _rasterExportFbo = 0; }
            if (_rasterExportTex != 0) { NativeGl.DeleteTextures(1, ref _rasterExportTex); _rasterExportTex = 0; }
            _rasterExportW = _rasterExportH = 0;
            foreach (var resource in _titleTextures.Values)
            {
                if (resource.Texture != 0)
                {
                    uint texture = resource.Texture;
                    NativeGl.DeleteTextures(1, ref texture);
                    resource.Texture = 0;
                }
            }
            _titleTextures.Clear();
            if (_vao != 0) { NativeGl.DeleteVertexArrays(1, ref _vao); _vao = 0; }

            NativeGl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            NativeGl.wglDeleteContext(_rc);
            _rc = IntPtr.Zero;
            UniformLocationCache.Clear();
            _intensityW = _intensityH = 0;
            _atlasRamp = "";
            _atlasGlyphCount = 1;
            _sceneGlyphSignature = "";
            _layerGlyphRows.Clear();
        }

        if (_dc != IntPtr.Zero)
        {
            NativeGl.ReleaseDC(Handle, _dc);
            _dc = IntPtr.Zero;
        }

        _cameraOrbiting = false;
        Capture = false;
        Cursor = Cursors.Default;
        base.OnHandleDestroyed(e);
    }
}

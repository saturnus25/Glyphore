using System.Text;

namespace Glyphore;

internal static class DetachedWindowSmokeTest
{
    private const int GwlStyle = -16;
    private const long WsCaption = 0x00C00000L;
    private const long WsSysMenu = 0x00080000L;
    private const long WsThickFrame = 0x00040000L;
    private const long WsMinimizeBox = 0x00020000L;
    private const long WsMaximizeBox = 0x00010000L;
    private const int WmMouseWheel = 0x020A;

    public static void Run()
    {
        RunPhase("native window frame", VerifyNativeWindowBasics);
        RunPhase("themed scrolling", VerifyThemedScrolling);
        RunPhase("detached window lifecycle", VerifyDetachedWindowLifecycle);
    }

    public static void RunTitlePipeline()
    {
        RunPhase("ASCII Title pipeline invariants", VerifyTitlePipelineInvariants);
    }

    private static void RunPhase(string phase, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Self-test phase '{phase}' failed: {ex.Message}", ex);
        }
    }

    private static void VerifyTitlePipelineInvariants()
    {
        AssertGeneratedTitleUsesOnlyCharset("@", "single-glyph charset");
        AssertGeneratedTitleUsesOnlyCharset(".:+*#", "ASCII ramp charset");
        AssertGeneratedTitleUsesOnlyCharset("◇●", "Unicode charset");
        AssertLetterSpacingInvariants();

        if (Math.Abs(ParameterCatalog.Defaults.GetValueOrDefault("title_letter_spacing", -1.0)) > 0.0001)
            throw new InvalidOperationException("New ASCII Titles must default to zero letter spacing.");
        if (Math.Abs(ParameterCatalog.Defaults.GetValueOrDefault("title_mix_custom_palette", 1.0)) > 0.0001)
            throw new InvalidOperationException("New ASCII Titles must default to palette-independent custom colors.");
        ParamDesc extrusionQuality = ParameterCatalog.Specific["ASCII Title"]
            .First(desc => desc.Key == "title_extrude_quality");
        if (extrusionQuality.Max <= 16.0)
            throw new InvalidOperationException("3D extrusion quality must remain editable above the recommended 16-sample budget.");

        var settings = new EffectSettings
        {
            Effect = "ASCII Title",
            CharsetName = "Smoke charset",
            Charset = ".:+*#"
        };
        settings.Set("title_mix_custom_palette", 1.0);
        settings.Set("title_letter_spacing", 4.0);
        GlyphoreScene scene = GlyphoreScene.FromSettings(settings);
        SceneEffectLayer layer = scene.Layers[0];
        layer.Preset = "Custom";
        layer.SourcePreset = "Typewriter";
        layer.TitlePrefab = "FIGlet · Standard";
        layer.Values["title_fade_mode"] = 7.0;
        layer.Values["title_fade_progress"] = .42;
        layer.Values["title_fade_softness"] = .11;
        layer.Values["title_extrude_mode"] = 1.0;
        layer.Values["title_extrude_target_x"] = .5;
        layer.Values["title_extrude_target_y"] = 1.15;
        layer.Values["title_extrude_convergence"] = .72;
        layer.Values["title_extrude_quality"] = 32.0;
        layer.Values["title_shimmer_extrusion"] = 1.0;
        layer.Values["future_custom_setting"] = 12.345678;
        layer.Masks.Add(new SceneLayerMask
        {
            Name = "Smoke radial gradient",
            Type = SceneMaskType.RadialGradient,
            X = .41,
            Y = .57,
            Width = .62,
            Height = .38,
            Rotation = 17,
            GradientSoftness = .73,
            Strength = .83
        });
        layer.Masks.Add(new SceneLayerMask
        {
            Name = "Smoke star",
            Type = SceneMaskType.Star,
            X = .5,
            Y = .5,
            Width = .4,
            Height = .4,
            Feather = .05,
            ShapeAmount = .37,
            StarPoints = 9
        });
        layer.Masks.Add(new SceneLayerMask
        {
            Name = "Smoke polygon",
            Type = SceneMaskType.Hexagon,
            X = .4,
            Y = .6,
            Width = .3,
            Height = .25,
            PolygonSides = 11
        });
        layer.Masks.Add(new SceneLayerMask
        {
            Name = "Smoke triangle",
            Type = SceneMaskType.Triangle,
            X = .62,
            Y = .42,
            Width = .34,
            Height = .27,
            TriangleType = SceneTriangleType.Obtuse
        });

        string tempPath = Path.Combine(Path.GetTempPath(), $"glyphore-title-smoke-{Guid.NewGuid():N}.glyphore");
        try
        {
            SceneFile.Save(tempPath, scene);
            GlyphoreScene loaded = SceneFile.Load(tempPath);
            SceneEffectLayer loadedLayer = loaded.Layers[0];
            if (loadedLayer.Charset != ".:+*#")
                throw new InvalidOperationException("ASCII Title charset did not survive scene save/load.");
            if (Math.Abs(loadedLayer.Values.GetValueOrDefault("title_mix_custom_palette") - 1.0) > 0.0001)
                throw new InvalidOperationException("Custom-color palette mixing did not survive scene save/load.");
            if (Math.Abs(loadedLayer.Values.GetValueOrDefault("title_letter_spacing") - 4.0) > 0.0001)
                throw new InvalidOperationException("ASCII Title letter spacing did not survive scene save/load.");
            if (Math.Abs(loadedLayer.Values.GetValueOrDefault("title_fade_mode") - 7.0) > 0.0001 ||
                Math.Abs(loadedLayer.Values.GetValueOrDefault("title_fade_progress") - .42) > 0.0001 ||
                Math.Abs(loadedLayer.Values.GetValueOrDefault("title_fade_softness") - .11) > 0.0001)
                throw new InvalidOperationException("Fade Reveal settings did not survive scene save/load.");
            if (loadedLayer.Masks.Count != 4 || loadedLayer.Masks[0].Type != SceneMaskType.RadialGradient ||
                Math.Abs(loadedLayer.Masks[0].Rotation - 17.0) > 0.0001 ||
                Math.Abs(loadedLayer.Masks[0].GradientSoftness - .73) > 0.0001 ||
                loadedLayer.Masks[1].Type != SceneMaskType.Star ||
                Math.Abs(loadedLayer.Masks[1].ShapeAmount - .37) > 0.0001 ||
                loadedLayer.Masks[1].StarPoints != 9 ||
                loadedLayer.Masks[2].Type != SceneMaskType.Hexagon ||
                loadedLayer.Masks[2].PolygonSides != 11 ||
                loadedLayer.Masks[3].Type != SceneMaskType.Triangle ||
                loadedLayer.Masks[3].TriangleType != SceneTriangleType.Obtuse)
                throw new InvalidOperationException("Layer-space mask data did not survive scene save/load.");
            if (!string.Equals(loadedLayer.SourcePreset, "Typewriter", StringComparison.Ordinal))
                throw new InvalidOperationException("Customized layer source-preset reference did not survive scene save/load.");
            if (Math.Abs(loadedLayer.Values.GetValueOrDefault("future_custom_setting") - 12.345678) > 0.000001)
                throw new InvalidOperationException("Unknown/future numeric layer settings were discarded by scene save/load.");
            if (Math.Abs(loadedLayer.Values.GetValueOrDefault("title_extrude_mode") - 1.0) > 0.0001 ||
                Math.Abs(loadedLayer.Values.GetValueOrDefault("title_extrude_convergence") - .72) > 0.0001 ||
                Math.Abs(loadedLayer.Values.GetValueOrDefault("title_extrude_quality") - 32.0) > 0.0001 ||
                Math.Abs(loadedLayer.Values.GetValueOrDefault("title_shimmer_extrusion") - 1.0) > 0.0001)
                throw new InvalidOperationException("New title extrusion/shimmer settings did not survive scene save/load.");
            if (loaded.FormatVersion != GlyphoreScene.CurrentFormatVersion)
                throw new InvalidOperationException("Scene save did not emit the current scene format version.");

            // Simulate loading the active layer into the editor and capturing it back after a
            // custom adjustment. Unknown/future values and preset provenance must survive this
            // path too, not merely the JSON serializer round-trip.
            EffectSettings editorRoundTrip = loaded.CreateSettings(loadedLayer);
            editorRoundTrip.Preset = "Custom";
            loadedLayer.CaptureFrom(editorRoundTrip);
            if (Math.Abs(loadedLayer.Values.GetValueOrDefault("future_custom_setting") - 12.345678) > 0.000001)
                throw new InvalidOperationException("Editor capture discarded an unknown/future layer setting.");
            if (!string.Equals(loadedLayer.SourcePreset, "Typewriter", StringComparison.Ordinal))
                throw new InvalidOperationException("Editor capture discarded customized layer preset provenance.");

            // Export freezes a deep scene clone. Mutating the live editor scene after the
            // snapshot must not alter layer values, masks, provenance or common scene data.
            GlyphoreScene exportSnapshot = loaded.Clone();
            loaded.BackgroundColor = "#123456";
            loadedLayer.Values["title_fade_progress"] = .91;
            loadedLayer.SourcePreset = "Changed after snapshot";
            loadedLayer.Masks[0].Width = .13;
            SceneEffectLayer snapshotLayer = exportSnapshot.Layers[0];
            if (exportSnapshot.BackgroundColor == loaded.BackgroundColor ||
                Math.Abs(snapshotLayer.Values.GetValueOrDefault("title_fade_progress") - .42) > 0.0001 ||
                !string.Equals(snapshotLayer.SourcePreset, "Typewriter", StringComparison.Ordinal) ||
                Math.Abs(snapshotLayer.Masks[0].Width - .62) > 0.0001)
                throw new InvalidOperationException("Scene.Clone() is not an immutable export snapshot.");

            loadedLayer.Values.Remove("title_letter_spacing");
            if (Math.Abs(loaded.CreateSettings(loadedLayer).Get("title_letter_spacing")) > 0.0001)
                throw new InvalidOperationException("Legacy scenes without title_letter_spacing do not receive the zero default.");
            loadedLayer.Values.Remove("title_mix_custom_palette");
            if (Math.Abs(loaded.CreateSettings(loadedLayer).Get("title_mix_custom_palette")) > 0.0001)
                throw new InvalidOperationException("Legacy scenes without title_mix_custom_palette do not receive the OFF default.");
        }
        finally
        {
            try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
        }
    }

    private static void AssertLetterSpacingInvariants()
    {
        const string prefab = "FIGlet · Standard";
        const string sample = "GLYPH";

        string legacy = AsciiTitlePrefabGenerator.Generate(sample, prefab);
        string zero = AsciiTitlePrefabGenerator.Generate(sample, prefab, 0);
        if (!string.Equals(legacy, zero, StringComparison.Ordinal))
            throw new InvalidOperationException("Letter spacing 0 changed the legacy FIGlet rendering path.");

        string spacingOne = AsciiTitlePrefabGenerator.Generate(sample, prefab, 1);
        string spacingFour = AsciiTitlePrefabGenerator.Generate(sample, prefab, 4);
        if (AsciiArtWidth(spacingOne) < AsciiArtWidth(zero))
            throw new InvalidOperationException("Positive letter spacing unexpectedly reduced FIGlet title width.");
        if (AsciiArtWidth(spacingFour) <= AsciiArtWidth(spacingOne))
            throw new InvalidOperationException("Increasing letter spacing did not increase FIGlet title width.");

        string singleZero = AsciiTitlePrefabGenerator.Generate("W", prefab, 0);
        string singleEight = AsciiTitlePrefabGenerator.Generate("W", prefab, 8);
        if (!string.Equals(singleZero, singleEight, StringComparison.Ordinal))
            throw new InvalidOperationException("Letter spacing altered the internal geometry of a single FIGcharacter.");
    }

    private static int AsciiArtWidth(string value)
    {
        int width = 0;
        foreach (string line in value.Replace("\r", string.Empty).Split('\n'))
            width = Math.Max(width, line.EnumerateRunes().Count());
        return width;
    }

    private static void AssertGeneratedTitleUsesOnlyCharset(string charset, string context)
    {
        string rendered = AsciiTitlePrefabGenerator.GenerateWithCharset("GLYPH", "FIGlet · Standard", charset);
        var allowed = new HashSet<int>();
        foreach (Rune rune in charset.EnumerateRunes())
            if (!Rune.IsWhiteSpace(rune))
                allowed.Add(rune.Value);

        foreach (Rune rune in rendered.EnumerateRunes())
        {
            if (Rune.IsWhiteSpace(rune)) continue;
            if (!allowed.Contains(rune.Value))
                throw new InvalidOperationException(
                    $"Generated title leaked glyph U+{rune.Value:X4} outside the {context}.");
        }
    }

    private static void VerifyNativeWindowBasics()
    {
        Screen initialScreen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
        Rectangle initialWork = initialScreen.WorkingArea;
        Size initialSize = new(
            Math.Min(900, Math.Max(560, initialWork.Width * 2 / 3)),
            Math.Min(640, Math.Max(400, initialWork.Height * 2 / 3)));
        Point initialLocation = new(
            initialWork.Left + Math.Max(0, (initialWork.Width - initialSize.Width) / 2),
            initialWork.Top + Math.Max(0, (initialWork.Height - initialSize.Height) / 2));

        using var window = new GlyphoreWindow
        {
            Text = "Glyphoré native-window smoke host",
            StartPosition = FormStartPosition.Manual,
            Location = initialLocation,
            Size = initialSize,
            MinimumSize = new Size(520, 360),
            ShowInTaskbar = false,
            MinimizeBox = true,
            MaximizeBox = true,
            Resizable = true
        };
        window.ContentPanel.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Theme.Bg });
        window.Show();
        Application.DoEvents();

        if (!window.IsHandleCreated)
            throw new InvalidOperationException("Native-window smoke host did not create an HWND.");
        AssertWindowStyles(window, expectResizable: true, expectMinimize: true, expectMaximize: true);
        AssertWindowDpiSynchronized(window);
        if (window.FormBorderStyle != FormBorderStyle.Sizable)
            throw new InvalidOperationException("Resizable GlyphoreWindow is not using the native Sizable frame.");
        if (window.ContentPanel.Bounds != window.ClientRectangle)
            throw new InvalidOperationException(
                $"GlyphoreWindow client content does not fill the native client area. Content={window.ContentPanel.Bounds}, Client={window.ClientRectangle}.");
        if (window.Padding != Padding.Empty)
            throw new InvalidOperationException("GlyphoreWindow unexpectedly uses client padding around the native frame.");

        window.WindowState = FormWindowState.Minimized;
        Application.DoEvents();
        if (window.WindowState != FormWindowState.Minimized)
            throw new InvalidOperationException("Native minimize did not minimize the GlyphoreWindow.");

        window.WindowState = FormWindowState.Normal;
        Application.DoEvents();
        window.WindowState = FormWindowState.Maximized;
        Application.DoEvents();
        if (window.WindowState != FormWindowState.Maximized)
            throw new InvalidOperationException("Native maximize did not maximize the GlyphoreWindow.");
        AssertNativeMaximizedOnNearestMonitor(window);

        window.WindowState = FormWindowState.Normal;
        Application.DoEvents();
        if (window.WindowState != FormWindowState.Normal)
            throw new InvalidOperationException("Native restore did not restore the GlyphoreWindow.");

        Screen[] screens = Screen.AllScreens;
        if (screens.Length > 1)
        {
            Screen secondary = screens.First(s => !s.Primary);
            Rectangle work = secondary.WorkingArea;
            window.Location = CenterInside(work, window.Size);
            Application.DoEvents();
            AssertWindowDpiSynchronized(window);

            window.WindowState = FormWindowState.Maximized;
            Application.DoEvents();
            AssertNativeMaximizedOnMonitor(window, secondary, "secondary monitor");
            window.WindowState = FormWindowState.Normal;
            Application.DoEvents();

            Screen primary = screens.First(s => s.Primary);
            Rectangle primaryWork = primary.WorkingArea;
            window.Location = CenterInside(primaryWork, window.Size);
            Application.DoEvents();
            AssertWindowDpiSynchronized(window);
            window.WindowState = FormWindowState.Maximized;
            Application.DoEvents();
            AssertNativeMaximizedOnMonitor(window, primary, "primary monitor after returning from secondary");
            window.WindowState = FormWindowState.Normal;
            Application.DoEvents();
        }

        window.MaximizeBox = false;
        window.Resizable = false;
        Application.DoEvents();
        if (window.FormBorderStyle != FormBorderStyle.FixedSingle)
            throw new InvalidOperationException("Non-resizable GlyphoreWindow is not using a native fixed frame.");
        AssertWindowStyles(window, expectResizable: false, expectMinimize: true, expectMaximize: false);
    }

    private static void VerifyThemedScrolling()
    {
        Screen screen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
        Rectangle work = screen.WorkingArea;
        using var host = new Form
        {
            StartPosition = FormStartPosition.Manual,
            Location = CenterInside(work, new Size(420, 320)),
            ClientSize = new Size(420, 320),
            ShowInTaskbar = false
        };

        var scroll = new ThemedScrollPanel { Dock = DockStyle.Fill, BackColor = Theme.Panel };
        var content = new Panel { Height = 1200, BackColor = Theme.Panel };
        var slider = new GlyphSlider
        {
            Left = 20,
            Top = 420,
            Width = 280,
            Height = 30,
            Minimum = 0,
            Maximum = 1000,
            Value = 500,
            MouseWheelAdjustsValue = false
        };
        var numeric = new GlyphNumericUpDown
        {
            Left = 20,
            Top = 520,
            Width = 120,
            Minimum = 0,
            Maximum = 100,
            Value = 50
        };
        var list = new ListBox
        {
            Left = 20,
            Top = 650,
            Width = 220,
            Height = 100,
            IntegralHeight = false
        };
        for (int i = 0; i < 30; i++) list.Items.Add($"Layer {i + 1}");
        content.Controls.Add(slider);
        content.Controls.Add(numeric);
        content.Controls.Add(list);
        scroll.SetContent(content);
        host.Controls.Add(scroll);
        host.Show();
        Application.DoEvents();

        if (!scroll.ScrollBarVisible)
            throw new InvalidOperationException("The themed scroll host did not expose a scrollbar for overflowing content.");

        int sliderBefore = slider.Value;
        int scrollBefore = scroll.ScrollOffset;
        int wheelWParam = unchecked((-120 & 0xFFFF) << 16);
        SendMessage(slider.Handle, WmMouseWheel, (IntPtr)wheelWParam, IntPtr.Zero);
        Application.DoEvents();

        if (slider.Value != sliderBefore)
            throw new InvalidOperationException("Mouse wheel changed a GlyphSlider value even though wheel editing is disabled.");
        if (scroll.ScrollOffset <= scrollBefore)
            throw new InvalidOperationException("Mouse wheel over a GlyphSlider did not scroll its themed parent viewport.");

        decimal numericBefore = numeric.Value;
        int numericScrollBefore = scroll.ScrollOffset;
        SendMessage(numeric.Handle, WmMouseWheel, (IntPtr)wheelWParam, IntPtr.Zero);
        Application.DoEvents();
        if (numeric.Value != numericBefore)
            throw new InvalidOperationException("Mouse wheel changed a wheel-safe numeric editor value.");
        if (scroll.ScrollOffset <= numericScrollBefore)
            throw new InvalidOperationException("Mouse wheel over a numeric editor did not scroll its themed parent viewport.");

        int listScrollBefore = scroll.ScrollOffset;
        int listTopBefore = list.TopIndex;
        SendMessage(list.Handle, WmMouseWheel, (IntPtr)wheelWParam, IntPtr.Zero);
        Application.DoEvents();
        if (scroll.ScrollOffset != listScrollBefore)
            throw new InvalidOperationException("Mouse wheel over the layer-style ListBox also moved the parent inspector.");
        if (list.TopIndex <= listTopBefore)
            throw new InvalidOperationException("Mouse wheel over the ListBox did not scroll the list itself.");
    }

    private static void VerifyDetachedWindowLifecycle()
    {
        Screen smokeScreen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
        Rectangle work = smokeScreen.WorkingArea;
        Size mainSize = new(
            Math.Min(800, Math.Max(520, work.Width * 2 / 3)),
            Math.Min(600, Math.Max(360, work.Height * 2 / 3)));

        using var main = new GlyphoreWindow
        {
            Text = "Glyphoré detached-window smoke host",
            StartPosition = FormStartPosition.Manual,
            Location = CenterInside(work, mainSize),
            Size = mainSize,
            ShowInTaskbar = false
        };
        using var windows = new DetachedWindowManager(main);

        main.Show();
        Application.DoEvents();

        var about = new AboutForm(null, windows)
        {
            StartPosition = FormStartPosition.Manual
        };
        about.Location = CenterInside(work, about.Size);
        windows.ShowSingle("about", about, centerOnMain: false);
        Application.DoEvents();

        if (!main.IsHandleCreated || !about.IsHandleCreated || main.Handle == about.Handle)
            throw new InvalidOperationException("Detached About did not receive an independent HWND.");
        if (about.Owner is not null)
            throw new InvalidOperationException("Detached About unexpectedly has an Owner.");
        if (about.Modal)
            throw new InvalidOperationException("Detached About is modal.");
        if (!about.ShowInTaskbar)
            throw new InvalidOperationException("Detached About is not configured for an independent taskbar entry.");
        if (!main.Enabled)
            throw new InvalidOperationException("Opening detached About disabled the main window.");
        if (about.ContentPanel.Bounds != about.ClientRectangle)
            throw new InvalidOperationException("Detached About content does not fill the native client area.");
        AssertWindowStyles(about, expectResizable: false, expectMinimize: true, expectMaximize: false);

        about.WindowState = FormWindowState.Minimized;
        Application.DoEvents();
        if (about.WindowState != FormWindowState.Minimized)
            throw new InvalidOperationException("Native About minimize did not minimize the detached window.");
        if (main.WindowState == FormWindowState.Minimized)
            throw new InvalidOperationException("Minimizing detached About also minimized the main window.");
        about.WindowState = FormWindowState.Normal;
        Application.DoEvents();

        AboutForm duplicateAttempt = windows.ShowSingle(
            "about",
            () => new AboutForm(null, windows),
            centerOnMain: false);
        if (!ReferenceEquals(about, duplicateAttempt))
            throw new InvalidOperationException("Detached window manager created a second About instance.");

        about.WindowState = FormWindowState.Minimized;
        Application.DoEvents();
        if (!windows.TryActivate("about"))
            throw new InvalidOperationException("Detached About could not be reactivated.");
        Application.DoEvents();
        if (about.WindowState == FormWindowState.Minimized)
            throw new InvalidOperationException("Detached About remained minimized after reactivation.");

        main.WindowState = FormWindowState.Minimized;
        Application.DoEvents();
        if (about.WindowState == FormWindowState.Minimized)
            throw new InvalidOperationException("Minimizing the main window also minimized detached About.");
        main.WindowState = FormWindowState.Normal;
        Application.DoEvents();

        main.Close();
        Application.DoEvents();
        if (!about.IsDisposed && about.Visible)
            throw new InvalidOperationException("Closing the main window did not close detached About.");
    }

    private static Point CenterInside(Rectangle work, Size size)
    {
        int x = work.Left + Math.Max(0, (work.Width - size.Width) / 2);
        int y = work.Top + Math.Max(0, (work.Height - size.Height) / 2);
        return new Point(x, y);
    }

    private static void AssertNativeMaximizedOnNearestMonitor(Form window)
    {
        Screen screen = Screen.FromHandle(window.Handle);
        AssertNativeMaximizedOnMonitor(window, screen, "nearest monitor");
    }

    private static void AssertNativeMaximizedOnMonitor(Form window, Screen expectedScreen, string context)
    {
        Screen actualScreen = Screen.FromHandle(window.Handle);
        if (!string.Equals(actualScreen.DeviceName, expectedScreen.DeviceName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Maximized GlyphoreWindow moved to the wrong {context}. Expected={expectedScreen.DeviceName}, Actual={actualScreen.DeviceName}.");
        }

        Rectangle work = expectedScreen.WorkingArea;
        Rectangle outer = window.Bounds;
        int frameTolerance = Math.Max(16, (int)Math.Ceiling(window.DeviceDpi / 96.0 * 24));

        // A standard maximized Win32 window can keep an invisible resize frame a few pixels
        // outside rcWork. Accept that native geometry, but reject large/custom offsets.
        if (!outer.Contains(work))
        {
            throw new InvalidOperationException(
                $"Native maximized window does not cover the {context} work area. Bounds={outer}, WorkArea={work}, Dpi={window.DeviceDpi}.");
        }

        int excessLeft = work.Left - outer.Left;
        int excessTop = work.Top - outer.Top;
        int excessRight = outer.Right - work.Right;
        int excessBottom = outer.Bottom - work.Bottom;
        if (excessLeft < 0 || excessTop < 0 || excessRight < 0 || excessBottom < 0 ||
            excessLeft > frameTolerance || excessTop > frameTolerance ||
            excessRight > frameTolerance || excessBottom > frameTolerance)
        {
            throw new InvalidOperationException(
                $"Native maximized frame has unexpected geometry on the {context}. Bounds={outer}, WorkArea={work}, " +
                $"Excess=L{excessLeft}/T{excessTop}/R{excessRight}/B{excessBottom}, Dpi={window.DeviceDpi}.");
        }
    }

    private static long GetWindowStyle(IntPtr hwnd)
    {
        return IntPtr.Size == 8
            ? GetWindowLongPtr64(hwnd, GwlStyle).ToInt64()
            : GetWindowLong32(hwnd, GwlStyle);
    }

    private static void AssertWindowStyles(GlyphoreWindow window, bool expectResizable, bool expectMinimize, bool expectMaximize)
    {
        long style = GetWindowStyle(window.Handle);
        if ((style & WsCaption) != WsCaption)
            throw new InvalidOperationException($"GlyphoreWindow is missing the native WS_CAPTION frame: 0x{style:X}.");
        if ((style & WsSysMenu) == 0)
            throw new InvalidOperationException($"GlyphoreWindow is missing the native WS_SYSMENU: 0x{style:X}.");
        if (((style & WsThickFrame) != 0) != expectResizable)
            throw new InvalidOperationException($"GlyphoreWindow WS_THICKFRAME mismatch: 0x{style:X}.");
        if (((style & WsMinimizeBox) != 0) != expectMinimize)
            throw new InvalidOperationException($"GlyphoreWindow WS_MINIMIZEBOX mismatch: 0x{style:X}.");
        if (((style & WsMaximizeBox) != 0) != expectMaximize)
            throw new InvalidOperationException($"GlyphoreWindow WS_MAXIMIZEBOX mismatch: 0x{style:X}.");
    }

    private static void AssertWindowDpiSynchronized(GlyphoreWindow window)
    {
        uint nativeDpi = GetDpiForWindow(window.Handle);
        if (nativeDpi != 0 && window.DeviceDpi != (int)nativeDpi)
        {
            throw new InvalidOperationException(
                $"GlyphoreWindow DeviceDpi is out of sync with its HWND. WinForms={window.DeviceDpi}, USER32={nativeDpi}.");
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hwnd, int index);

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong32(IntPtr hwnd, int index);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hwnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

}

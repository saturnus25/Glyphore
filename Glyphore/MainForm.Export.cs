namespace Glyphore;

internal sealed partial class MainForm
{
    private async Task ExportDialogAsync()
    {
        if (_exporting) return;

        IReadOnlyList<RasterExportProfile> rasterProfiles = RasterAnimationExporter.GetAvailableProfiles(refresh: true);
        string[] textLabels = Localization.English
            ? ["PowerShell (pseudo-alpha)", "HTML (alpha)", "JSON (stores alpha)", "C# standalone (pseudo-alpha)", "ANSI (pseudo-alpha)", "Text (no alpha)"]
            : ["PowerShell (pseudo-alpha)", "HTML (alpha)", "JSON (guarda alpha)", "C# standalone (pseudo-alpha)", "ANSI (pseudo-alpha)", "Texto (sin alpha)"];
        string[] textPatterns = ["*.ps1", "*.html", "*.json", "*.cs", "*.ans", "*.txt"];

        var filters = new List<string>();
        foreach (var profile in rasterProfiles)
            filters.Add($"{profile.DisplayName}|*{profile.Extension}");
        for (int i = 0; i < textLabels.Length; i++)
            filters.Add($"{textLabels[i]}|{textPatterns[i]}");

        int defaultRasterIndex = Math.Max(0, rasterProfiles.ToList().FindIndex(profile => profile.Id == "mp4-h264"));
        RasterExportProfile defaultProfile = rasterProfiles[Math.Min(defaultRasterIndex, rasterProfiles.Count - 1)];
        using var dialog = new SaveFileDialog
        {
            Filter = string.Join("|", filters),
            FilterIndex = defaultRasterIndex + 1,
            AddExtension = true,
            FileName = _settings.Effect.Replace(' ', '_').ToLowerInvariant() + defaultProfile.Extension
        };

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        int chosen = Math.Max(0, dialog.FilterIndex - 1);
        RasterExportProfile? rasterProfile = chosen < rasterProfiles.Count ? rasterProfiles[chosen] : null;
        RasterExportOptions? rasterOptions = null;
        TextExportOptions textOptions = TextExportOptions.Default;
        if (rasterProfile is not null)
        {
            rasterOptions = ShowRasterExportOptions(rasterProfile, allowTransparent: !_importView.Visible);
            if (rasterOptions is null) return;
        }
        else if (ExportService.SupportsPseudoTransparency(dialog.FileName))
        {
            TextExportOptions? selected = ShowTerminalTransparencyOptions(dialog.FileName);
            if (selected is null) return;
            textOptions = selected;
        }

        _exporting = true;
        _discordPresence.SetBusyOperation(PresenceOperation.Export, true);
        _discordPresence.NotifyUserActivity();
        _left.Enabled = false;
        _exportButton.Enabled = false;
        UseWaitCursor = true;

        // Commit the editor state exactly once, then freeze a complete scene snapshot. Export must
        // never depend on whatever layer happens to be active, on a stale render cache, or on a
        // later UI event that fires while frames are being encoded.
        GlyphoreScene? exportScene = null;
        if (!_importView.Visible && _sceneReady && _scene.Layers.Count > 0)
        {
            SyncSceneFromSettings();
            exportScene = _scene.Clone();
        }

        using var progressWindow = new ExportProgressWindow();
        progressWindow.Show(this);
        progressWindow.SetProgress(0,
            Localization.English ? "Preparing export…" : "Preparando exportación…",
            Localization.English ? "Freezing the current scene state." : "Congelando el estado actual de la escena.");

        try
        {
            var exportSettings = exportScene is { Layers.Count: > 0 }
                ? exportScene.CreateSettings(exportScene.ActiveLayer ?? exportScene.Layers[0])
                : _settings.Clone();
            int exportedFrameCount;

            // Native scene raster/video exports are streamed one frame at a time. The previous
            // implementation retained every full RGBA RasterFrame until FFmpeg started; at 1080p
            // that is ~8 MiB/frame and could exceed 10 GiB on an ordinary animation.
            if (rasterOptions is not null && exportScene is not null && !_importView.Visible)
            {
                GlyphoreScene sceneSnapshot = exportScene;
                RasterExportOptions rasterSnapshot = rasterOptions;
                int frameCount = SafeExportFrameCount(exportSettings);
                exportedFrameCount = frameCount;
                bool usesFfmpeg = rasterSnapshot.Profile.RequiresFfmpeg;
                await RasterAnimationExporter.SaveGeneratedAsync(
                    dialog.FileName,
                    exportSettings,
                    frameCount,
                    (index, reusableRgba) => _preview.CaptureRasterExportFrame(
                        sceneSnapshot,
                        index / (double)exportSettings.Fps,
                        rasterSnapshot.BackgroundMode,
                        rasterSnapshot.SolidColor,
                        reusableRgba),
                    rasterSnapshot,
                    detailedProgress: info =>
                    {
                        double ratio = info.Total <= 0 ? 0.0 : Math.Clamp(info.Current / (double)info.Total, 0.0, 1.0);
                        if (info.Stage == RasterExportStage.WritingFrames)
                        {
                            int percent = 4 + (int)Math.Round(92 * ratio);
                            progressWindow.SetProgress(
                                percent,
                                usesFfmpeg
                                    ? (Localization.English ? "Rendering + encoding" : "Renderizando + codificando")
                                    : (Localization.English ? "Rendering PNG frames" : "Renderizando frames PNG"),
                                Localization.English
                                    ? $"Frame {info.Current}/{Math.Max(1, info.Total)} · streaming (bounded memory)"
                                    : $"Frame {info.Current}/{Math.Max(1, info.Total)} · streaming (memoria acotada)");
                            _status.Text = Localization.English
                                ? $"Exporting {info.Current}/{Math.Max(1, info.Total)}"
                                : $"Exportando {info.Current}/{Math.Max(1, info.Total)}";
                        }
                        else
                        {
                            progressWindow.SetProgress(
                                99,
                                Localization.English ? "Finalizing FFmpeg" : "Finalizando FFmpeg",
                                Localization.English ? "Flushing the encoder and container." : "Cerrando el codificador y el contenedor.");
                        }
                    });
            }
            else
            {
                // Native text/code exports are streamed too. Keeping Text + RGB24 + A8 for every
                // frame was just as expensive as the old raster path on large ASCII grids.
                if (rasterOptions is null && !_importView.Visible)
                {
                    int frameCount = SafeExportFrameCount(exportSettings);
                    exportedFrameCount = frameCount;
                    await ExportService.SaveGeneratedAsync(
                        dialog.FileName,
                        exportSettings,
                        frameCount,
                        index =>
                        {
                            double time = index / (double)Math.Max(1, exportSettings.Fps);
                            return exportScene is not null
                                ? _preview.CaptureExportFrame(exportScene, time)
                                : _preview.CaptureExportFrame(time);
                        },
                        textOptions,
                        progress: (current, total) =>
                        {
                            int percent = 4 + (int)Math.Round(94 * current / (double)Math.Max(1, total));
                            progressWindow.SetProgress(
                                percent,
                                Localization.English ? "Rendering + writing text frames" : "Renderizando + escribiendo frames de texto",
                                Localization.English
                                    ? $"Frame {current}/{Math.Max(1, total)} · streaming (bounded memory)"
                                    : $"Frame {current}/{Math.Max(1, total)} · streaming (memoria acotada)");
                            _status.Text = Localization.English
                                ? $"Exporting {current}/{Math.Max(1, total)}"
                                : $"Exportando {current}/{Math.Max(1, total)}";
                        });
                }
                else
                {
                    // Imported PowerShell frames already exist in memory as plain strings. Keep this
                    // compatibility path; it does not retain native RGB/A8 planes.
                    List<ExportFrame> frames;
                    if (_importView.Visible && _importFrames.Count > 0)
                    {
                        progressWindow.SetProgress(10,
                            Localization.English ? "Preparing imported frames" : "Preparando frames importados",
                            Localization.English ? $"{_importFrames.Count} frame(s) already available." : $"{_importFrames.Count} frame(s) ya disponibles.");
                        frames = _importFrames.Select(frame => new ExportFrame(frame)).ToList();
                        exportSettings.Fps = Math.Max(1, (int)Math.Round(_importFps));
                    }
                    else
                    {
                        int frameCount = SafeExportFrameCount(exportSettings);
                        frames = new List<ExportFrame>(Math.Min(frameCount, 4096));
                        for (int i = 0; i < frameCount; i++)
                        {
                            double time = i / (double)Math.Max(1, exportSettings.Fps);
                            frames.Add(exportScene is not null
                                ? _preview.CaptureExportFrame(exportScene, time)
                                : _preview.CaptureExportFrame(time));
                            if ((i & 3) == 0 || i == frameCount - 1) await Task.Yield();
                        }
                    }

                    exportedFrameCount = frames.Count;
                    if (rasterOptions is null &&
                        ExportService.ContainsNonOpaqueAlpha(frames) &&
                        !ExportService.SupportsPartialAlpha(dialog.FileName) &&
                        !textOptions.PseudoTransparency)
                    {
                        DialogResult compatibility = MessageBox.Show(
                            ExportService.PartialAlphaCompatibilityMessage(dialog.FileName, Localization.English),
                            Localization.English ? "Alpha compatibility" : "Compatibilidad de alpha",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning);
                        if (compatibility != DialogResult.OK) return;
                    }

                    if (rasterOptions is not null)
                    {
                        bool usesFfmpeg = rasterOptions.Profile.RequiresFfmpeg;
                        await RasterAnimationExporter.SaveAsync(
                            dialog.FileName,
                            exportSettings,
                            frames,
                            rasterOptions,
                            detailedProgress: info =>
                            {
                                double ratio = info.Total <= 0 ? 0.0 : Math.Clamp(info.Current / (double)info.Total, 0.0, 1.0);
                                if (info.Stage == RasterExportStage.WritingFrames)
                                {
                                    int endPercent = usesFfmpeg ? 82 : 100;
                                    int percent = 10 + (int)Math.Round((endPercent - 10) * ratio);
                                    progressWindow.SetProgress(percent,
                                        Localization.English ? "Preparing image frames" : "Preparando frames de imagen",
                                        $"{info.Current}/{Math.Max(1, info.Total)}");
                                }
                                else
                                {
                                    int percent = 82 + (int)Math.Round(18 * ratio);
                                    progressWindow.SetProgress(percent,
                                        Localization.English ? "Encoding with FFmpeg" : "Codificando con FFmpeg",
                                        $"{info.Current}/{Math.Max(1, info.Total)}");
                                }
                            });
                    }
                    else
                    {
                        await ExportService.SaveGeneratedAsync(
                            dialog.FileName,
                            exportSettings,
                            Math.Max(1, frames.Count),
                            index => frames[Math.Clamp(index, 0, frames.Count - 1)],
                            textOptions,
                            progress: (current, total) =>
                            {
                                int percent = 10 + (int)Math.Round(88 * current / (double)Math.Max(1, total));
                                progressWindow.SetProgress(percent,
                                    Localization.English ? "Writing text export" : "Escribiendo exportación de texto",
                                    $"{current}/{Math.Max(1, total)}");
                            });
                    }
                }
            }

            progressWindow.SetProgress(100,
                Localization.English ? "Export complete" : "Exportación completada",
                Path.GetFileName(dialog.FileName));
            _status.Text = Localization.English
                ? $"Exported {Path.GetFileName(dialog.FileName)} · {exportedFrameCount} frames"
                : $"Exportado {Path.GetFileName(dialog.FileName)} · {exportedFrameCount} frames";
            await Task.Delay(180);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                Localization.English ? "Export" : "Exportación",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            if (!progressWindow.IsDisposed) progressWindow.Close();
            UseWaitCursor = false;
            _left.Enabled = true;
            _exportButton.Enabled = true;
            _exporting = false;
            _discordPresence.SetBusyOperation(PresenceOperation.Export, false);
            _discordPresence.NotifyUserActivity();
            UpdateDiscordPresenceContext();
        }
    }

    private RasterExportOptions? ShowRasterExportOptions(RasterExportProfile profile, bool allowTransparent)
    {
        using var form = new Form
        {
            Text = Localization.English ? "Raster export" : "Exportación raster",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(470, 210),
            BackColor = Theme.Panel,
            ForeColor = Theme.Text
        };

        var format = new Label
        {
            Left = 16,
            Top = 15,
            Width = 438,
            Height = 24,
            Text = profile.DisplayName,
            ForeColor = Theme.AccentText,
            Font = new Font(Font, FontStyle.Bold)
        };
        var description = new Label
        {
            Left = 16,
            Top = 41,
            Width = 438,
            Height = 34,
            Text = profile.Description,
            ForeColor = Theme.Muted
        };
        var backgroundLabel = new Label
        {
            Left = 16,
            Top = 88,
            Width = 112,
            Height = 24,
            Text = Localization.English ? "Background" : "Fondo",
            TextAlign = ContentAlignment.MiddleLeft
        };
        var background = new ComboBox
        {
            Left = 130,
            Top = 87,
            Width = 202,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        Theme.Combo(background);

        var modes = new List<(string Label, RasterBackgroundMode Mode)>
        {
            (Localization.English ? "Scene Background" : "Fondo de escena", RasterBackgroundMode.SceneBackground),
            (Localization.English ? "Solid Color" : "Color sólido", RasterBackgroundMode.SolidColor)
        };
        if (profile.SupportsAlpha && allowTransparent)
            modes.Add((Localization.English ? "Transparent" : "Transparente", RasterBackgroundMode.Transparent));
        foreach (var mode in modes) background.Items.Add(mode.Label);
        int transparentIndex = modes.FindIndex(item => item.Mode == RasterBackgroundMode.Transparent);
        background.SelectedIndex = transparentIndex >= 0 ? transparentIndex : 0;

        Color solidColor = Color.Black;
        var colorButton = new Button
        {
            Left = 340,
            Top = 87,
            Width = 114,
            Height = 28,
            Text = Localization.English ? "Solid color…" : "Color sólido…"
        };
        Theme.Button(colorButton);
        colorButton.Click += (_, _) =>
        {
            using var picker = new ColorDialog { Color = solidColor, FullOpen = true };
            if (picker.ShowDialog(form) != DialogResult.OK) return;
            solidColor = picker.Color;
            colorButton.BackColor = solidColor;
            colorButton.ForeColor = solidColor.GetBrightness() < .45f ? Color.White : Color.Black;
        };

        void SyncColorButton()
        {
            if (background.SelectedIndex < 0) return;
            colorButton.Enabled = modes[background.SelectedIndex].Mode == RasterBackgroundMode.SolidColor;
        }
        background.SelectedIndexChanged += (_, _) => SyncColorButton();
        SyncColorButton();

        if (profile.SupportsAlpha && !allowTransparent)
        {
            var importedHint = new Label
            {
                Left = 16,
                Top = 121,
                Width = 438,
                Height = 24,
                Text = Localization.English
                    ? "Transparent background is unavailable for imported text frames."
                    : "El fondo transparente no está disponible para frames de texto importados.",
                ForeColor = Theme.Muted
            };
            form.Controls.Add(importedHint);
        }

        var ok = new Button
        {
            Left = 276,
            Top = 164,
            Width = 84,
            Height = 30,
            Text = Localization.English ? "Export" : "Exportar",
            DialogResult = DialogResult.OK
        };
        var cancel = new Button
        {
            Left = 370,
            Top = 164,
            Width = 84,
            Height = 30,
            Text = Localization.English ? "Cancel" : "Cancelar",
            DialogResult = DialogResult.Cancel
        };
        Theme.Button(ok, accent: true);
        Theme.Button(cancel);
        form.Controls.AddRange([format, description, backgroundLabel, background, colorButton, ok, cancel]);
        form.AcceptButton = ok;
        form.CancelButton = cancel;

        if (form.ShowDialog(this) != DialogResult.OK) return null;
        RasterBackgroundMode selected = modes[Math.Max(0, background.SelectedIndex)].Mode;
        return new RasterExportOptions(profile, selected, solidColor);
    }


    private static int SafeExportFrameCount(EffectSettings settings)
    {
        double raw = settings.Duration * Math.Max(1.0, settings.Fps);
        if (double.IsNaN(raw) || double.IsInfinity(raw) || raw <= 0)
            throw new InvalidOperationException(Localization.English
                ? "The current FPS/duration combination is invalid."
                : "La combinación actual de FPS/duración no es válida.");
        if (raw > int.MaxValue)
            throw new InvalidOperationException(Localization.English
                ? "This export would contain more than 2,147,483,647 frames. Reduce FPS or duration. The editor fields themselves are no longer artificially capped."
                : "Esta exportación tendría más de 2.147.483.647 frames. Reduce FPS o duración. Los campos del editor ya no tienen un límite artificial.");
        return Math.Max(1, (int)Math.Round(raw));
    }

    private TextExportOptions? ShowTerminalTransparencyOptions(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        string format = ext switch
        {
            ".ps1" => "PowerShell",
            ".cs" => "C# / ANSI",
            ".ans" => "ANSI",
            _ => "Terminal"
        };

        using var form = new Form
        {
            Text = Localization.English ? "Terminal transparency" : "Transparencia de terminal",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(520, 286),
            BackColor = Theme.Panel,
            ForeColor = Theme.Text
        };

        var title = new Label
        {
            Left = 16, Top = 14, Width = 488, Height = 24,
            Text = Localization.English ? $"{format} has no real per-character alpha" : $"{format} no tiene alpha real por carácter",
            Font = new Font(Font, FontStyle.Bold), ForeColor = Theme.AccentText
        };
        var info = new Label
        {
            Left = 16, Top = 42, Width = 488, Height = 42,
            Text = Localization.English
                ? "Pseudo transparency blends every glyph color toward the console background. It looks transparent only when the chosen color matches the terminal background."
                : "La pseudo-transparencia mezcla el color de cada carácter hacia el fondo de la consola. Parece transparente cuando el color elegido coincide con el fondo del terminal.",
            ForeColor = Theme.Muted
        };
        var enabled = new GlyphCheckBox
        {
            Left = 16, Top = 91, Width = 488, Height = 25,
            Text = Localization.English ? "Simulate transparency against a background color" : "Simular transparencia contra un color de fondo",
            Checked = true
        };
        Theme.CheckBox(enabled);

        var backgroundLabel = new Label
        {
            Left = 16, Top = 128, Width = 142, Height = 25,
            Text = Localization.English ? "Console background" : "Fondo de consola",
            TextAlign = ContentAlignment.MiddleLeft
        };
        var preset = new SafeComboBox { Left = 160, Top = 128, Width = 214, Height = 26 };
        SetupCombo(preset);
        var presets = new List<(string Name, Color Color)>
        {
            (Localization.English ? "Console Black" : "Negro de consola", ColorTranslator.FromHtml("#0C0C0C")),
            (Localization.English ? "PowerShell Blue" : "Azul PowerShell", ColorTranslator.FromHtml("#012456")),
            (Localization.English ? "Console Dark Blue" : "Azul oscuro consola", ColorTranslator.FromHtml("#0037DA")),
            (Localization.English ? "Console Dark Green" : "Verde oscuro consola", ColorTranslator.FromHtml("#13A10E")),
            (Localization.English ? "Console Dark Cyan" : "Cian oscuro consola", ColorTranslator.FromHtml("#3A96DD")),
            (Localization.English ? "Console Dark Red" : "Rojo oscuro consola", ColorTranslator.FromHtml("#C50F1F")),
            (Localization.English ? "Console Dark Magenta" : "Magenta oscuro consola", ColorTranslator.FromHtml("#881798")),
            (Localization.English ? "Console Dark Yellow" : "Amarillo oscuro consola", ColorTranslator.FromHtml("#C19C00")),
            (Localization.English ? "Console Gray" : "Gris consola", ColorTranslator.FromHtml("#CCCCCC")),
            (Localization.English ? "Console Dark Gray" : "Gris oscuro consola", ColorTranslator.FromHtml("#767676")),
            (Localization.English ? "Console Blue" : "Azul consola", ColorTranslator.FromHtml("#3B78FF")),
            (Localization.English ? "Console Green" : "Verde consola", ColorTranslator.FromHtml("#16C60C")),
            (Localization.English ? "Console Cyan" : "Cian consola", ColorTranslator.FromHtml("#61D6D6")),
            (Localization.English ? "Console Red" : "Rojo consola", ColorTranslator.FromHtml("#E74856")),
            (Localization.English ? "Console Magenta" : "Magenta consola", ColorTranslator.FromHtml("#B4009E")),
            (Localization.English ? "Console Yellow" : "Amarillo consola", ColorTranslator.FromHtml("#F9F1A5")),
            (Localization.English ? "Console White" : "Blanco consola", ColorTranslator.FromHtml("#F2F2F2"))
        };

        foreach (var entry in presets) preset.Items.Add(entry.Name);
        preset.SelectedIndex = ext == ".ps1" ? 1 : 0;
        Color selectedColor = presets[Math.Max(0, preset.SelectedIndex)].Color;

        var swatch = new Panel { Left = 383, Top = 128, Width = 30, Height = 26, BackColor = selectedColor };
        var custom = new GlyphButton
        {
            Left = 421, Top = 128, Width = 83, Height = 26,
            Text = Localization.English ? "Custom…" : "Personal…"
        };
        Theme.Button(custom);

        void SyncEnabled()
        {
            preset.Enabled = enabled.Checked;
            custom.Enabled = enabled.Checked;
            swatch.Enabled = enabled.Checked;
        }
        enabled.CheckedChanged += (_, _) => SyncEnabled();
        preset.SelectedIndexChanged += (_, _) =>
        {
            if (preset.SelectedIndex < 0 || preset.SelectedIndex >= presets.Count) return;
            selectedColor = presets[preset.SelectedIndex].Color;
            swatch.BackColor = selectedColor;
        };
        custom.Click += (_, _) =>
        {
            using var picker = new ColorDialog { Color = selectedColor, FullOpen = true };
            if (picker.ShowDialog(form) != DialogResult.OK) return;
            selectedColor = picker.Color;
            swatch.BackColor = selectedColor;
            preset.SelectedIndex = -1;
        };

        var hint = new Label
        {
            Left = 16, Top = 166, Width = 488, Height = 42,
            Text = Localization.English
                ? "Tip: choose the exact background used by Windows Terminal/PowerShell. This is color precomposition, not true alpha."
                : "Consejo: elige exactamente el fondo usado por Windows Terminal/PowerShell. Es composición de color, no alpha real.",
            ForeColor = Theme.Muted
        };
        var ok = new GlyphButton
        {
            Left = 326, Top = 232, Width = 84, Height = 32,
            Text = Localization.English ? "Export" : "Exportar",
            DialogResult = DialogResult.OK
        };
        var cancel = new GlyphButton
        {
            Left = 420, Top = 232, Width = 84, Height = 32,
            Text = Localization.English ? "Cancel" : "Cancelar",
            DialogResult = DialogResult.Cancel
        };
        Theme.Button(ok, accent: true); Theme.Button(cancel);
        form.Controls.AddRange([title, info, enabled, backgroundLabel, preset, swatch, custom, hint, ok, cancel]);
        form.AcceptButton = ok; form.CancelButton = cancel;
        SyncEnabled();
        if (form.ShowDialog(this) != DialogResult.OK) return null;
        return new TextExportOptions(enabled.Checked, selectedColor);
    }
}

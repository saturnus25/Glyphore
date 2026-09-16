namespace Glyphore;

internal static class Program
{
    private static readonly string CrashLogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Glyphore",
        "crash.log");

    private sealed record SelfTestDefinition(
        string Argument,
        string Stage,
        string SidecarName,
        int FailureExitCode,
        Action Run);

    private static readonly SelfTestDefinition[] SelfTests =
    [
        new("--self-test-discord-rpc", "Discord Rich Presence self-test", "discord-rpc-smoke-error.txt", 77, DiscordRichPresenceSmokeTest.Run),
        new("--self-test-alpha-export", "Transparent-export self-test", "alpha-export-smoke-error.txt", 76, TransparentExportSmokeTest.Run),
        new("--self-test-title-pipeline", "ASCII Title pipeline self-test", "title-pipeline-smoke-error.txt", 75, DetachedWindowSmokeTest.RunTitlePipeline),
        new("--self-test-detached-windows", "Detached-window self-test", "detached-window-smoke-error.txt", 74, DetachedWindowSmokeTest.Run),
        new("--self-test-third-party-licenses", "Third-party license self-test", "license-smoke-error.txt", 73, ThirdPartyLicenseUiSmokeTest.Run)
    ];

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (TryRunSelfTest(args)) return;

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        Application.ThreadException += (_, e) => ReportFatal("UI thread", e.Exception, showDialog: true);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                ReportFatal("Unhandled background exception", ex, showDialog: false);
        };

        try
        {
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            // MainForm constructor/startup failures happen before a window exists. Because this is
            // a WinExe there is normally no console, so make the failure visible and persist it.
            ReportFatal("Startup", ex, showDialog: true);
        }
    }

    private static bool TryRunSelfTest(string[] args)
    {
        foreach (SelfTestDefinition selfTest in SelfTests)
        {
            if (!args.Any(arg => arg.Equals(selfTest.Argument, StringComparison.OrdinalIgnoreCase))) continue;

            try
            {
                selfTest.Run();
                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                ReportSelfTestFailure(selfTest.Stage, ex, selfTest.SidecarName);
                Environment.ExitCode = selfTest.FailureExitCode;
            }

            return true;
        }

        return false;
    }

    private static void ReportSelfTestFailure(string stage, Exception exception, string sidecarName)
    {
        ReportFatal(stage, exception, showDialog: false);
        try
        {
            string path = Path.Combine(Environment.CurrentDirectory, sidecarName);
            File.WriteAllText(path, $"{stage}\r\n{exception}\r\n", new System.Text.UTF8Encoding(false));
        }
        catch
        {
            // Build diagnostics are best-effort and must not hide the original self-test failure.
        }
    }

    private static void ReportFatal(string stage, Exception exception, bool showDialog)
    {
        string details = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {stage}\r\n{exception}\r\n\r\n";

        try
        {
            string? directory = Path.GetDirectoryName(CrashLogPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.AppendAllText(CrashLogPath, details);
        }
        catch
        {
            // Diagnostics must never cause a second crash.
        }

        if (!showDialog) return;

        try
        {
            MessageBox.Show(
                $"Glyphoré could not continue.\n\n{exception.Message}\n\nCrash log:\n{CrashLogPath}",
                "Glyphoré 6.0.0",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch
        {
            // Nothing else can be displayed safely at this point.
        }
    }
}

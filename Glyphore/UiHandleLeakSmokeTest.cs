using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Glyphore;

internal static class UiHandleLeakSmokeTest
{
    private const int WarmupIterations = 30;
    private const int BatchIterations = 50;
    private const int BatchCount = 8;
    private const uint GrGdiObjects = 0;
    private const uint GrUserObjects = 1;

    private readonly record struct Sample(int Iteration, uint UserObjects, uint GdiObjects, int ProcessHandles);

    [DllImport("user32.dll")]
    private static extern uint GetGuiResources(IntPtr process, uint flags);

    public static void Run()
    {
        using var form = new MainForm
        {
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-30000, -30000),
            Opacity = 0
        };

        form.Show();
        Application.DoEvents();
        form.RunDynamicUiStressBatch(0, WarmupIterations);

        var samples = new List<Sample> { Capture(WarmupIterations) };
        for (int batch = 0; batch < BatchCount; batch++)
        {
            int start = WarmupIterations + batch * BatchIterations;
            try
            {
                form.RunDynamicUiStressBatch(start, BatchIterations);
                samples.Add(Capture(start + BatchIterations));
                WriteReport(samples);
            }
            catch (Exception exception)
            {
                Sample failed = Capture(start);
                samples.Add(failed);
                WriteReport(samples);
                throw new InvalidOperationException(
                    $"Dynamic UI rebuild failed near iteration {start}; last resources: " +
                    $"USER {failed.UserObjects}, GDI {failed.GdiObjects}, handles {failed.ProcessHandles}.",
                    exception);
            }
        }

        Sample first = samples[0];
        Sample last = samples[^1];
        int userGrowth = checked((int)last.UserObjects - (int)first.UserObjects);
        int gdiGrowth = checked((int)last.GdiObjects - (int)first.GdiObjects);
        int handleGrowth = last.ProcessHandles - first.ProcessHandles;

        if (userGrowth > 80 || gdiGrowth > 40 || handleGrowth > 160)
        {
            throw new InvalidOperationException(
                $"Dynamic UI resources grew without stabilizing across {BatchCount * BatchIterations} measured rebuild iterations: " +
                $"USER {first.UserObjects}->{last.UserObjects} (delta {userGrowth}), " +
                $"GDI {first.GdiObjects}->{last.GdiObjects} (delta {gdiGrowth}), " +
                $"handles {first.ProcessHandles}->{last.ProcessHandles} (delta {handleGrowth}).");
        }
    }

    private static Sample Capture(int iteration)
    {
        using Process process = Process.GetCurrentProcess();
        return new Sample(
            iteration,
            GetGuiResources(process.Handle, GrUserObjects),
            GetGuiResources(process.Handle, GrGdiObjects),
            process.HandleCount);
    }

    private static void WriteReport(IEnumerable<Sample> samples)
    {
        var report = new StringBuilder("iteration,user_objects,gdi_objects,process_handles\r\n");
        foreach (Sample sample in samples)
            report.Append(sample.Iteration).Append(',')
                .Append(sample.UserObjects).Append(',')
                .Append(sample.GdiObjects).Append(',')
                .Append(sample.ProcessHandles).Append("\r\n");
        File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "ui-handle-stress-report.csv"), report.ToString(), new UTF8Encoding(false));
    }
}

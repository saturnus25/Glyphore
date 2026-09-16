using System.Diagnostics;
namespace Glyphore;

internal sealed partial class GlPreviewControl
{
    private void StartScheduler()
    {
        StopScheduler();
        _schedulerCts = new System.Threading.CancellationTokenSource();
        var token = _schedulerCts.Token;
        _schedulerThread = new System.Threading.Thread(() => SchedulerLoop(token))
        {
            IsBackground = true,
            Name = "Glyphore frame scheduler",
            Priority = System.Threading.ThreadPriority.AboveNormal
        };
        _schedulerThread.Start();
    }

    private void StopScheduler()
    {
        var cts = _schedulerCts;
        _schedulerCts = null;
        if (cts is null) return;
        try { cts.Cancel(); } catch { }
        try
        {
            if (_schedulerThread is { IsAlive: true } thread && thread != System.Threading.Thread.CurrentThread)
                thread.Join(250);
        }
        catch { }
        _schedulerThread = null;
        cts.Dispose();
        System.Threading.Interlocked.Exchange(ref _paintQueued, 0);
    }

    private void SchedulerLoop(System.Threading.CancellationToken token)
    {
        long next = Stopwatch.GetTimestamp();
        while (!token.IsCancellationRequested)
        {
            if (!_loaded || _paused || !IsHandleCreated || IsDisposed)
            {
                System.Threading.Thread.Sleep(5);
                next = Stopwatch.GetTimestamp();
                continue;
            }

            int fps = Math.Max(1, System.Threading.Volatile.Read(ref _targetFps));
            long period = Math.Max(1L, Stopwatch.Frequency / fps);
            long now = Stopwatch.GetTimestamp();
            if (now >= next)
            {
                // Keep a stable cadence, but do not try to replay a backlog after a stall.
                next = now - next > period * 4 ? now + period : next + period;
                QueueFrame();
                continue;
            }

            double remainingMs = (next - now) * 1000.0 / Stopwatch.Frequency;
            if (remainingMs > 2.0)
                System.Threading.Thread.Sleep(Math.Max(1, (int)Math.Floor(remainingMs - 1.0)));
            else
                System.Threading.Thread.Yield();
        }
    }

    private void QueueFrame()
    {
        if (System.Threading.Interlocked.Exchange(ref _paintQueued, 1) != 0) return;
        try
        {
            BeginInvoke((Action)(() =>
            {
                if (!IsDisposed && IsHandleCreated) Invalidate();
                else System.Threading.Interlocked.Exchange(ref _paintQueued, 0);
            }));
        }
        catch (ObjectDisposedException) { System.Threading.Interlocked.Exchange(ref _paintQueued, 0); }
        catch (InvalidOperationException) { System.Threading.Interlocked.Exchange(ref _paintQueued, 0); }
    }

    private static bool IsValidWglProcAddress(IntPtr address) =>
        address != IntPtr.Zero &&
        address != new IntPtr(1) &&
        address != new IntPtr(2) &&
        address != new IntPtr(3) &&
        address != new IntPtr(-1);
}

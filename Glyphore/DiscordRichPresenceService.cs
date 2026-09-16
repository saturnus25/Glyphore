using System.Diagnostics;
using System.IO.Pipes;
using System.Text.Json;

namespace Glyphore;

internal sealed class DiscordRichPresenceService : IAsyncDisposable
{
    internal const string ApplicationId = "1549417846138863637";
    internal const string LogoAssetKey = "glyphore_logo";
    internal const string IdleAssetKey = "glyphore_idle";
    internal static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(5);
    internal static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(20);

    private static readonly TimeSpan LoopDelay = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan ConnectTimeoutPerPipe = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan ReadyTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan PreviewPresenceDuration = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan MinimumPublishInterval = TimeSpan.FromMilliseconds(650);

    private readonly object _stateGate = new();
    private readonly SemaphoreSlim _writeGate = new(1, 1);
    private readonly CancellationTokenSource _abort = new();
    private readonly TimeSpan _idleTimeout;
    private readonly long _sessionStartUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private Task? _runner;
    private NamedPipeClientStream? _pipe;
    private Task? _readerTask;
    private PresenceContext _context = PresenceContext.Scene;
    private PresenceOperation _operation;
    private bool _enabled;
    private bool _stopRequested;
    private bool _wasConnected;
    private long _lastActivityTick = Environment.TickCount64;
    private long _previewUntilTick;
    private long _nextReconnectTick;
    private long _lastPublishTick;
    private int _forceReconnect;
    private int _clearRequested;
    private string? _lastPublishedKey;

    public DiscordRichPresenceService(bool enabled, TimeSpan? idleTimeout = null)
    {
        _enabled = enabled;
        _idleTimeout = idleTimeout ?? DefaultIdleTimeout;
    }

    public bool Enabled
    {
        get { lock (_stateGate) return _enabled; }
    }

    public void Start()
    {
        lock (_stateGate)
        {
            if (_runner is not null) return;
            _runner = Task.Run(() => RunAsync(_abort.Token));
        }
    }

    public void SetEnabled(bool enabled)
    {
        bool changed;
        lock (_stateGate)
        {
            changed = _enabled != enabled;
            _enabled = enabled;
            if (enabled) _lastActivityTick = Environment.TickCount64;
        }
        if (!enabled) Interlocked.Exchange(ref _clearRequested, 1);
        if (changed && enabled) Interlocked.Exchange(ref _forceReconnect, 1);
    }

    public void SetContext(PresenceContext context)
    {
        lock (_stateGate) _context = context;
    }

    public void SetBusyOperation(PresenceOperation operation, bool active)
    {
        lock (_stateGate)
        {
            if (active)
            {
                _operation = operation;
            }
            else if (_operation == operation)
            {
                _operation = PresenceOperation.None;
            }
        }
    }

    public void NotifyPreviewing()
    {
        lock (_stateGate)
        {
            _previewUntilTick = Environment.TickCount64 + (long)PreviewPresenceDuration.TotalMilliseconds;
        }
    }

    public void NotifyUserActivity()
    {
        long now = Environment.TickCount64;
        // Mouse movement can produce hundreds of messages a second. Recording activity every
        // 100 ms is more than enough for a five-minute idle detector and avoids needless writes.
        long previous = Interlocked.Read(ref _lastActivityTick);
        if (unchecked(now - previous) >= 100 || unchecked(now - previous) < 0)
            Interlocked.Exchange(ref _lastActivityTick, now);
    }

    public async Task StopAsync(TimeSpan timeout)
    {
        Task? runner;
        lock (_stateGate)
        {
            _enabled = false;
            _stopRequested = true;
            runner = _runner;
        }

        if (runner is null) return;
        Task completed = await Task.WhenAny(runner, Task.Delay(timeout)).ConfigureAwait(false);
        if (!ReferenceEquals(completed, runner))
        {
            _abort.Cancel();
            try { _pipe?.Dispose(); } catch { }
        }
        try { await runner.ConfigureAwait(false); } catch (OperationCanceledException) { }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        _abort.Cancel();
        _abort.Dispose();
        _writeGate.Dispose();
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Interlocked.Exchange(ref _clearRequested, 0) != 0 && _pipe is not null)
                {
                    await DisconnectAsync(clearActivity: true, cancellationToken).ConfigureAwait(false);
                }

                bool enabled;
                bool stop;
                lock (_stateGate)
                {
                    enabled = _enabled;
                    stop = _stopRequested;
                }

                if (!enabled)
                {
                    if (_pipe is not null) await DisconnectAsync(clearActivity: true, cancellationToken).ConfigureAwait(false);
                    if (stop) break;
                    await DelayLoopAsync(cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if (_pipe is null)
                {
                    long now = Environment.TickCount64;
                    bool reconnectNow = Interlocked.Exchange(ref _forceReconnect, 0) != 0;
                    if (reconnectNow || now >= Interlocked.Read(ref _nextReconnectTick))
                    {
                        bool connected = await TryConnectAsync(cancellationToken).ConfigureAwait(false);
                        if (!connected)
                        {
                            Interlocked.Exchange(ref _nextReconnectTick, now + (long)ReconnectDelay.TotalMilliseconds);
                        }
                    }
                    await DelayLoopAsync(cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if (_readerTask is { IsCompleted: true })
                {
                    await DisconnectAsync(clearActivity: false, cancellationToken).ConfigureAwait(false);
                    Interlocked.Exchange(ref _nextReconnectTick, Environment.TickCount64 + (long)ReconnectDelay.TotalMilliseconds);
                    continue;
                }

                try
                {
                    await PublishIfChangedAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (IOException)
                {
                    await DisconnectAsync(clearActivity: false, cancellationToken).ConfigureAwait(false);
                    Interlocked.Exchange(ref _nextReconnectTick, Environment.TickCount64 + (long)ReconnectDelay.TotalMilliseconds);
                    continue;
                }
                catch (ObjectDisposedException)
                {
                    await DisconnectAsync(clearActivity: false, cancellationToken).ConfigureAwait(false);
                    Interlocked.Exchange(ref _nextReconnectTick, Environment.TickCount64 + (long)ReconnectDelay.TotalMilliseconds);
                    continue;
                }
                await DelayLoopAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            Trace.TraceWarning($"Discord Rich Presence unexpected error: {ex.Message}");
        }
        finally
        {
            try { await DisconnectAsync(clearActivity: true, CancellationToken.None).ConfigureAwait(false); }
            catch { }
        }
    }

    private static Task DelayLoopAsync(CancellationToken cancellationToken)
        => Task.Delay(LoopDelay, cancellationToken);

    private async Task<bool> TryConnectAsync(CancellationToken cancellationToken)
    {
        for (int index = 0; index <= 9 && !cancellationToken.IsCancellationRequested; index++)
        {
            var pipe = new NamedPipeClientStream(
                ".",
                $"discord-ipc-{index}",
                PipeDirection.InOut,
                PipeOptions.Asynchronous);
            try
            {
                using var connectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                connectCts.CancelAfter(ConnectTimeoutPerPipe);
                await pipe.ConnectAsync(connectCts.Token).ConfigureAwait(false);

                byte[] handshake = DiscordRpcProtocol.BuildHandshakePayload(ApplicationId);
                await DiscordRpcProtocol.WriteFrameAsync(pipe, DiscordRpcOpcode.Handshake, handshake, cancellationToken).ConfigureAwait(false);

                using var readyCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                readyCts.CancelAfter(ReadyTimeout);
                bool ready = false;
                while (!readyCts.IsCancellationRequested)
                {
                    DiscordRpcFrame frame = await DiscordRpcProtocol.ReadFrameAsync(pipe, readyCts.Token).ConfigureAwait(false);
                    if (frame.Opcode == DiscordRpcOpcode.Ping)
                    {
                        await DiscordRpcProtocol.WriteFrameAsync(pipe, DiscordRpcOpcode.Pong, frame.Payload, readyCts.Token).ConfigureAwait(false);
                        continue;
                    }
                    if (frame.Opcode == DiscordRpcOpcode.Close) break;
                    if (frame.Opcode != DiscordRpcOpcode.Frame) continue;
                    ready = DiscordRpcProtocol.IsReadyPayload(frame.Payload);
                    if (ready) break;
                }

                if (!ready)
                {
                    pipe.Dispose();
                    continue;
                }

                _pipe = pipe;
                _readerTask = Task.Run(() => ReadLoopAsync(pipe, cancellationToken), cancellationToken);
                _lastPublishedKey = null;
                _lastPublishTick = 0;
                _wasConnected = true;
                Trace.TraceInformation("Discord Rich Presence connected.");
                return true;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                pipe.Dispose();
            }
            catch (OperationCanceledException)
            {
                pipe.Dispose();
                throw;
            }
            catch (IOException)
            {
                pipe.Dispose();
            }
            catch (UnauthorizedAccessException)
            {
                pipe.Dispose();
            }
            catch (Exception ex)
            {
                pipe.Dispose();
                Trace.TraceWarning($"Discord Rich Presence connection error: {ex.Message}");
            }
        }
        return false;
    }

    private async Task ReadLoopAsync(NamedPipeClientStream pipe, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && pipe.IsConnected)
            {
                DiscordRpcFrame frame = await DiscordRpcProtocol.ReadFrameAsync(pipe, cancellationToken).ConfigureAwait(false);
                switch (frame.Opcode)
                {
                    case DiscordRpcOpcode.Ping:
                        await WriteFrameSafeAsync(pipe, DiscordRpcOpcode.Pong, frame.Payload, cancellationToken).ConfigureAwait(false);
                        break;
                    case DiscordRpcOpcode.Close:
                        return;
                    case DiscordRpcOpcode.Frame:
                        if (DiscordRpcProtocol.IsErrorPayload(frame.Payload, out string? message) && !string.IsNullOrWhiteSpace(message))
                            Trace.TraceWarning($"Discord Rich Presence RPC error: {message}");
                        break;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (IOException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidDataException ex)
        {
            Trace.TraceWarning($"Discord Rich Presence invalid RPC frame: {ex.Message}");
        }
    }

    private async Task PublishIfChangedAsync(CancellationToken cancellationToken)
    {
        NamedPipeClientStream? pipe = _pipe;
        if (pipe is null || !pipe.IsConnected) return;

        long now = Environment.TickCount64;
        PresenceContext context;
        PresenceOperation operation;
        long previewUntil;
        bool enabled;
        lock (_stateGate)
        {
            enabled = _enabled;
            context = _context;
            operation = _operation;
            previewUntil = _previewUntilTick;
        }
        if (!enabled) return;

        bool idle = IsIdle(operation, now, Interlocked.Read(ref _lastActivityTick), _idleTimeout);
        bool previewing = operation == PresenceOperation.None && now < previewUntil;
        DiscordPresenceDescriptor descriptor = ResolvePresence(context, operation, idle, previewing);
        string key = $"{descriptor.Details}\n{descriptor.State}\n{descriptor.LargeImage}\n{descriptor.LargeText}";
        if (string.Equals(key, _lastPublishedKey, StringComparison.Ordinal)) return;

        if (_lastPublishedKey is not null && unchecked(now - _lastPublishTick) < (long)MinimumPublishInterval.TotalMilliseconds)
            return;

        byte[] payload = DiscordRpcProtocol.BuildSetActivityPayload(
            Environment.ProcessId,
            descriptor,
            _sessionStartUnixSeconds,
            Guid.NewGuid().ToString("N"));
        await WriteFrameSafeAsync(pipe, DiscordRpcOpcode.Frame, payload, cancellationToken).ConfigureAwait(false);
        _lastPublishedKey = key;
        _lastPublishTick = now;
    }

    private async Task DisconnectAsync(bool clearActivity, CancellationToken cancellationToken)
    {
        NamedPipeClientStream? pipe = _pipe;
        _pipe = null;
        _readerTask = null;
        _lastPublishedKey = null;
        _lastPublishTick = 0;
        if (pipe is null) return;

        if (clearActivity && pipe.IsConnected)
        {
            try
            {
                byte[] clear = DiscordRpcProtocol.BuildClearActivityPayload(Environment.ProcessId, Guid.NewGuid().ToString("N"));
                await WriteFrameSafeAsync(pipe, DiscordRpcOpcode.Frame, clear, cancellationToken).ConfigureAwait(false);
            }
            catch { }
        }

        try { pipe.Dispose(); } catch { }
        if (_wasConnected)
        {
            _wasConnected = false;
            Trace.TraceInformation("Discord Rich Presence disconnected.");
        }
    }

    private async Task WriteFrameSafeAsync(
        NamedPipeClientStream pipe,
        DiscordRpcOpcode opcode,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        await _writeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!pipe.IsConnected) return;
            await DiscordRpcProtocol.WriteFrameAsync(pipe, opcode, payload, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    internal static bool IsIdle(PresenceOperation operation, long nowTick, long lastActivityTick, TimeSpan idleTimeout)
        => operation == PresenceOperation.None &&
           unchecked(nowTick - lastActivityTick) >= (long)idleTimeout.TotalMilliseconds;

    internal static DiscordPresenceDescriptor ResolvePresence(
        PresenceContext context,
        PresenceOperation operation,
        bool idle,
        bool previewing)
    {
        if (operation == PresenceOperation.Export)
            return new("Exporting an animation", "Rendering", LogoAssetKey, "Glyphoré");
        if (idle)
            return new("Idle", "Glyphoré", IdleAssetKey, "Glyphoré — Idle");
        if (context == PresenceContext.MaskEditing)
            return new("Editing a mask", "Scene Composer", LogoAssetKey, "Glyphoré");
        if (context == PresenceContext.AsciiTitleStudio)
            return new("Creating an ASCII title", "ASCII Title Studio", LogoAssetKey, "Glyphoré");
        if (previewing)
            return new("Previewing an animation", "Glyphoré", LogoAssetKey, "Glyphoré");
        return new("Editing a scene", "Glyphoré", LogoAssetKey, "Glyphoré");
    }
}

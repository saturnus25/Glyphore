using System.Buffers.Binary;
using System.Text;
using System.Text.Json;

namespace Glyphore;

internal static class DiscordRichPresenceSmokeTest
{
    public static void Run()
    {
        ValidateHandshakeAndFraming();
        ValidatePartialReads();
        ValidateSetActivityAndClear();
        ValidateResolver();
        ValidateIdleTransition();
        ValidateReconnectCadence();
    }

    private static void ValidateHandshakeAndFraming()
    {
        byte[] payload = DiscordRpcProtocol.BuildHandshakePayload(DiscordRichPresenceService.ApplicationId);
        using (JsonDocument document = JsonDocument.Parse(payload))
        {
            JsonElement root = document.RootElement;
            Require(root.GetProperty("v").GetInt32() == 1, "Handshake protocol version must be 1.");
            Require(root.GetProperty("client_id").GetString() == DiscordRichPresenceService.ApplicationId,
                "Handshake client_id does not match Glyphoré's Discord application.");
        }

        byte[] frame = DiscordRpcProtocol.EncodeFrame(DiscordRpcOpcode.Handshake, payload);
        Require(BinaryPrimitives.ReadInt32LittleEndian(frame.AsSpan(0, 4)) == 0, "Handshake opcode is not little-endian 0.");
        Require(BinaryPrimitives.ReadInt32LittleEndian(frame.AsSpan(4, 4)) == payload.Length, "Payload length is not encoded little-endian.");
        Require(frame.AsSpan(8).SequenceEqual(payload), "Encoded Discord frame payload changed.");
    }

    private static void ValidatePartialReads()
    {
        byte[] payload = Encoding.UTF8.GetBytes("{\"evt\":\"READY\"}");
        byte[] encoded = DiscordRpcProtocol.EncodeFrame(DiscordRpcOpcode.Frame, payload);
        using var stream = new ChunkedReadStream(encoded, maxChunk: 3);
        DiscordRpcFrame decoded = DiscordRpcProtocol.ReadFrameAsync(stream, CancellationToken.None).GetAwaiter().GetResult();
        Require(decoded.Opcode == DiscordRpcOpcode.Frame, "Partial-read frame opcode changed.");
        Require(decoded.Payload.SequenceEqual(payload), "Partial-read payload was not reconstructed exactly.");
        Require(DiscordRpcProtocol.IsReadyPayload(decoded.Payload), "READY payload was not recognized.");
    }

    private static void ValidateSetActivityAndClear()
    {
        var descriptor = new DiscordPresenceDescriptor("Editing a scene", "Glyphoré", "glyphore_logo", "Glyphoré");
        byte[] payload = DiscordRpcProtocol.BuildSetActivityPayload(1234, descriptor, 1000, "nonce-a");
        using (JsonDocument document = JsonDocument.Parse(payload))
        {
            JsonElement root = document.RootElement;
            Require(root.GetProperty("cmd").GetString() == "SET_ACTIVITY", "SET_ACTIVITY command missing.");
            JsonElement args = root.GetProperty("args");
            Require(args.GetProperty("pid").GetInt32() == 1234, "SET_ACTIVITY pid missing.");
            JsonElement activity = args.GetProperty("activity");
            Require(activity.GetProperty("type").GetInt32() == 0, "Activity type must be Playing/0.");
            Require(activity.GetProperty("details").GetString() == "Editing a scene", "Activity details changed.");
            Require(activity.GetProperty("state").GetString() == "Glyphoré", "Activity state changed.");
            Require(activity.GetProperty("timestamps").GetProperty("start").GetInt64() == 1000, "Session timestamp changed.");
            Require(activity.GetProperty("assets").GetProperty("large_image").GetString() == "glyphore_logo", "Unexpected large image key.");
            Require(!activity.GetProperty("assets").TryGetProperty("small_image", out _), "Small image must not be published.");
        }

        byte[] clear = DiscordRpcProtocol.BuildClearActivityPayload(1234, "nonce-b");
        using JsonDocument clearDocument = JsonDocument.Parse(clear);
        JsonElement clearActivity = clearDocument.RootElement.GetProperty("args").GetProperty("activity");
        Require(clearActivity.ValueKind == JsonValueKind.Null, "Clear Activity must send activity:null.");
    }

    private static void ValidateResolver()
    {
        DiscordPresenceDescriptor export = DiscordRichPresenceService.ResolvePresence(PresenceContext.MaskEditing, PresenceOperation.Export, idle: true, previewing: true);
        Require(export.Details == "Exporting an animation" && export.State == "Rendering", "Export must outrank every editor/idle state.");

        DiscordPresenceDescriptor idle = DiscordRichPresenceService.ResolvePresence(PresenceContext.MaskEditing, PresenceOperation.None, idle: true, previewing: true);
        Require(idle.Details == "Idle" && idle.LargeImage == "glyphore_idle", "Idle state/asset is wrong.");

        DiscordPresenceDescriptor mask = DiscordRichPresenceService.ResolvePresence(PresenceContext.MaskEditing, PresenceOperation.None, idle: false, previewing: true);
        Require(mask.Details == "Editing a mask", "Mask editing must outrank previewing.");

        DiscordPresenceDescriptor title = DiscordRichPresenceService.ResolvePresence(PresenceContext.AsciiTitleStudio, PresenceOperation.None, idle: false, previewing: true);
        Require(title.Details == "Creating an ASCII title", "Title Studio must outrank previewing.");

        DiscordPresenceDescriptor preview = DiscordRichPresenceService.ResolvePresence(PresenceContext.Scene, PresenceOperation.None, idle: false, previewing: true);
        Require(preview.Details == "Previewing an animation", "Preview state is wrong.");

        DiscordPresenceDescriptor scene = DiscordRichPresenceService.ResolvePresence(PresenceContext.Scene, PresenceOperation.None, idle: false, previewing: false);
        Require(scene.Details == "Editing a scene", "Default scene state is wrong.");
    }

    private static void ValidateIdleTransition()
    {
        long now = 1_000_000;
        long justActive = now - (long)TimeSpan.FromMinutes(4).TotalMilliseconds;
        long stale = now - (long)TimeSpan.FromMinutes(6).TotalMilliseconds;
        Require(!DiscordRichPresenceService.IsIdle(PresenceOperation.None, now, justActive, TimeSpan.FromMinutes(5)),
            "Active editor entered idle too early.");
        Require(DiscordRichPresenceService.IsIdle(PresenceOperation.None, now, stale, TimeSpan.FromMinutes(5)),
            "Inactive editor did not enter idle.");
        Require(!DiscordRichPresenceService.IsIdle(PresenceOperation.Export, now, stale, TimeSpan.FromMinutes(5)),
            "Export must suppress idle even after the timeout.");
    }

    private static void ValidateReconnectCadence()
    {
        Require(DiscordRichPresenceService.ReconnectDelay >= TimeSpan.FromSeconds(15), "Reconnect polling is too aggressive.");
        Require(DiscordRichPresenceService.ReconnectDelay <= TimeSpan.FromSeconds(30), "Reconnect polling is too slow.");
        Require(DiscordRichPresenceService.DefaultIdleTimeout == TimeSpan.FromMinutes(5), "Release idle timeout must be five minutes.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class ChunkedReadStream : MemoryStream
    {
        private readonly int _maxChunk;

        public ChunkedReadStream(byte[] data, int maxChunk) : base(data, writable: false)
            => _maxChunk = Math.Max(1, maxChunk);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => base.ReadAsync(buffer[..Math.Min(buffer.Length, _maxChunk)], cancellationToken);
    }
}

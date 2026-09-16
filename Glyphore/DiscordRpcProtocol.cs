using System.Buffers.Binary;
using System.Text;
using System.Text.Json;

namespace Glyphore;

internal readonly record struct DiscordRpcFrame(DiscordRpcOpcode Opcode, byte[] Payload);

internal static class DiscordRpcProtocol
{
    internal const int MaxPayloadBytes = 1024 * 1024;

    public static byte[] BuildHandshakePayload(string clientId)
        => JsonSerializer.SerializeToUtf8Bytes(new { v = 1, client_id = clientId });

    public static byte[] BuildSetActivityPayload(
        int pid,
        DiscordPresenceDescriptor descriptor,
        long sessionStartUnixSeconds,
        string nonce)
    {
        var payload = new
        {
            cmd = "SET_ACTIVITY",
            args = new
            {
                pid,
                activity = new
                {
                    type = 0,
                    details = descriptor.Details,
                    state = descriptor.State,
                    timestamps = new { start = sessionStartUnixSeconds },
                    assets = new
                    {
                        large_image = descriptor.LargeImage,
                        large_text = descriptor.LargeText
                    },
                    instance = false
                }
            },
            nonce
        };
        return JsonSerializer.SerializeToUtf8Bytes(payload);
    }

    public static byte[] BuildClearActivityPayload(int pid, string nonce)
        => JsonSerializer.SerializeToUtf8Bytes(new
        {
            cmd = "SET_ACTIVITY",
            args = new { pid, activity = (object?)null },
            nonce
        });

    public static byte[] EncodeFrame(DiscordRpcOpcode opcode, ReadOnlySpan<byte> payload)
    {
        if (payload.Length > MaxPayloadBytes) throw new InvalidDataException("Discord RPC payload is too large.");
        byte[] frame = new byte[8 + payload.Length];
        BinaryPrimitives.WriteInt32LittleEndian(frame.AsSpan(0, 4), (int)opcode);
        BinaryPrimitives.WriteInt32LittleEndian(frame.AsSpan(4, 4), payload.Length);
        payload.CopyTo(frame.AsSpan(8));
        return frame;
    }

    public static async Task WriteFrameAsync(
        Stream stream,
        DiscordRpcOpcode opcode,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        if (payload.Length > MaxPayloadBytes) throw new InvalidDataException("Discord RPC payload is too large.");
        byte[] header = new byte[8];
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(0, 4), (int)opcode);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(4, 4), payload.Length);
        await stream.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        if (!payload.IsEmpty) await stream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task<DiscordRpcFrame> ReadFrameAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[] header = new byte[8];
        await ReadExactlyAsync(stream, header, cancellationToken).ConfigureAwait(false);
        int opcode = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(0, 4));
        int length = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(4, 4));
        if (opcode < (int)DiscordRpcOpcode.Handshake || opcode > (int)DiscordRpcOpcode.Pong)
            throw new InvalidDataException($"Unknown Discord RPC opcode {opcode}.");
        if (length < 0 || length > MaxPayloadBytes)
            throw new InvalidDataException($"Invalid Discord RPC payload length {length}.");

        byte[] payload = new byte[length];
        if (length > 0) await ReadExactlyAsync(stream, payload, cancellationToken).ConfigureAwait(false);
        return new DiscordRpcFrame((DiscordRpcOpcode)opcode, payload);
    }

    public static bool IsReadyPayload(ReadOnlyMemory<byte> payload)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(payload);
            return document.RootElement.TryGetProperty("evt", out JsonElement evt) &&
                   string.Equals(evt.GetString(), "READY", StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static bool IsErrorPayload(ReadOnlyMemory<byte> payload, out string? message)
    {
        message = null;
        try
        {
            using JsonDocument document = JsonDocument.Parse(payload);
            JsonElement root = document.RootElement;
            if (!root.TryGetProperty("evt", out JsonElement evt) ||
                !string.Equals(evt.GetString(), "ERROR", StringComparison.OrdinalIgnoreCase))
                return false;

            if (root.TryGetProperty("data", out JsonElement data) &&
                data.TryGetProperty("message", out JsonElement text))
                message = text.GetString();
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static async Task ReadExactlyAsync(Stream stream, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int read = await stream.ReadAsync(buffer[offset..], cancellationToken).ConfigureAwait(false);
            if (read <= 0) throw new EndOfStreamException("Discord RPC pipe closed while reading a frame.");
            offset += read;
        }
    }
}

namespace Glyphore;

internal enum PresenceContext
{
    Scene,
    AsciiTitleStudio,
    MaskEditing
}

internal enum PresenceOperation
{
    None,
    Export
}

internal enum DiscordRpcOpcode
{
    Handshake = 0,
    Frame = 1,
    Close = 2,
    Ping = 3,
    Pong = 4
}

internal readonly record struct DiscordPresenceDescriptor(
    string Details,
    string State,
    string LargeImage,
    string LargeText);

/// <summary>
/// Minimal Discord local RPC client used by Glyphoré. This deliberately implements only the
/// legacy IPC pieces needed by Rich Presence: handshake, SET_ACTIVITY, ping/pong and reconnect.
/// It has no OAuth, HTTP, bot, token, Social SDK or native dependency.
/// </summary>

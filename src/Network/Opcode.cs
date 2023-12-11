namespace Owop.Network;

/// <summary>OWOP Websocket protocol opcode.</summary>
public enum Opcode
{
    /// <summary>Sets the client player's ID.</summary>
    SetId,
    /// <summary>Updates players, pixels, and disconnects within a world.</summary>
    WorldUpdate,
    /// <summary>Loads a world chunk.</summary>
    ChunkLoad,
    /// <summary>Teleports the client player.</summary>
    Teleport,
    /// <summary>Sets the client player's <see cref="PlayerRank"/>.</summary>
    SetRank,
    /// <summary>Updates captcha status.</summary>
    Captcha,
    /// <summary>Sets the client player's ...something. TODO: update this when I understand it better</summary>
    SetPixelQuota,
    /// <summary>Protects a chunk within a world.</summary>
    ChunkProtect,
    /// <summary>Sets the maximum amount of players that can connect to a world.</summary>
    MaxPlayerCount,
    /// <summary>Updates the duration of donation boost remaining.</summary>
    DonationTimer
}

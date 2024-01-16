using System.Drawing;
using Microsoft.Extensions.Logging;
using Owop.Client;
using Owop.Network;
using Owop.Util;

namespace Owop.Game;

/// <summary>Represents a world that a client has connected to.</summary>
public interface IWorld
{
    // TODO: Move this when make chunk system
    /// <summary>The width and height, in pixels, of a chunk.</summary>
    public const int ChunkSize = 16;

    /// <summary>The world's name.</summary>
    string Name { get; }

    /// <summary>The world's connected players (including the client player).</summary>
    IReadOnlyDictionary<int, IPlayer> Players { get; }

    /// <summary>The client player connected to the world.</summary>
    IClientPlayer ClientPlayer { get; }

    /// <summary>Whether the client can safely send chat messages to the world.</summary>
    bool IsChatReady { get; }

    /// <summary>Whether the world requires a password to be able to draw.</summary>
    bool IsPasswordProtected { get; }

    /// <summary>The world's client connection.</summary>
    IWorldConnection Connection { get; }

    /// <summary>The world's own logger. Uses the same logging factory as provided to the <see cref="OwopClient"/>.</summary>
    ILogger Logger { get; }

    Task SendChatMessage(string message);

    void QueueChatMessage(string message);

    Task RunCommand(string command, params object[] args);

    Task TellPlayer(int id, string message);

    Task LogIn(string password);

    Task MovePlayer(int id, Position worldPos);

    Task SetPlayerRank(int id, PlayerRank rank);

    Task KickPlayer(int id);

    Task SetPlayerMuted(int id, bool muted);

    Task MutePlayer(int id);

    Task UnmutePlayer(int id);

    Task<WhoisData> QueryWhois(int playerId);

    Task SetPassword(string password);

    Task RemovePassword();

    Task SetRestricted(bool restricted);

    Task Restrict();

    Task Unrestrict();

    Task<bool> PlacePixel(Position? worldPos = null, Color? color = null, bool sneaky = false);

    Task Disconnect();
}

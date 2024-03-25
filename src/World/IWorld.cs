using System.Drawing;
using Microsoft.Extensions.Logging;
using Owop.Client;
using Owop.Network;
using Owop.Util;

namespace Owop.Game;

/// <summary>Represents a world that a client has connected to.</summary>
public interface IWorld
{
    /// <summary>The world's name.</summary>
    string Name { get; }

    /// <summary>The world's connected players (including the client player).</summary>
    IReadOnlyDictionary<int, IPlayer> Players { get; }

    /// <summary>The world's available chunks.</summary>
    IWorldChunks Chunks { get; }

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

    /// <summary>Sends a chat message.</summary>
    /// <param name="message">The message to send.</param>
    Task SendChatMessage(string message);

    /// <summary>Queues a chat message.</summary>
    /// <param name="message">The message to queue.</param>
    void QueueChatMessage(string message);

    /// <summary>Runs a server command.</summary>
    /// <param name="command">The command name.</param>
    /// <param name="args">The command arguments, to be space-separated.</param>
    Task RunCommand(string command, params object[] args);

    /// <summary>Privately sends a message to a player.</summary>
    /// <param name="id">The player ID.</param>
    /// <param name="message">The message to send.</param>
    Task TellPlayer(int id, string message);

    /// <summary>Logs into a <see cref="PlayerRank"/>.</summary>
    /// <param name="password">The rank password.</param>
    Task LogIn(string password);

    /// <summary>Moves a player to a world position.</summary>
    /// <param name="id">The player ID.</param>
    /// <param name="worldPos">The player's new position.</param>
    Task MovePlayer(int id, Position worldPos);

    /// <summary>Sets a player's rank.</summary>
    /// <param name="id">The player ID.</param>
    /// <param name="rank">The player's new rank.</param>
    Task SetPlayerRank(int id, PlayerRank rank);

    /// <summary>Kicks a player from the world.</summary>
    /// <param name="id">The player ID.</param>
    Task KickPlayer(int id);

    /// <summary>Sets the mute state of a player.</summary>
    /// <param name="id">The player ID.</param>
    /// <param name="muted">Whether to mute the player.</param>
    Task SetPlayerMuted(int id, bool muted);

    /// <summary>Mutes a player.</summary>
    /// <param name="id">The player ID.</param>
    Task MutePlayer(int id);

    /// <summary>Unmutes a player.</summary>
    /// <param name="id">The player ID.</param>
    Task UnmutePlayer(int id);

    /// <summary>Queries the connection data of a player.</summary>
    /// <param name="playerId">The player ID.</param>
    /// <returns>The connection data.</returns>
    Task<WhoisData> QueryWhois(int playerId);

    /// <summary>Sets the <see cref="PlayerRank.Player"/> world password.</summary>
    /// <param name="password">The new world password.</param>
    Task SetPassword(string password);

    /// <summary>Removes the world password.</summary>
    Task RemovePassword();

    /// <summary>Sets whether this world is restricted (immutable for new users).</summary>
    /// <param name="restricted">The restricted state.</param>
    Task SetRestricted(bool restricted);

    /// <summary>Restricts the world.</summary>
    /// <seealso cref="SetRestricted"/>
    Task Restrict();

    /// <summary>Unrestricts the world.</summary>
    /// <seealso cref="SetRestricted"/>
    Task Unrestrict();

    /// <summary>Places a pixel at a world position.</summary>
    /// <param name="worldPos">The pixel position.</param>
    /// <param name="color">The pixel color.</param>
    /// <param name="sneaky">Whether to return to the previous location after placement.</param>
    Task<bool> PlacePixel(Position? worldPos = null, Color? color = null, bool sneaky = false);

    /// <summary>Disconnects from the world.</summary>
    // TODO: what happens if try reconnect
    Task Disconnect();
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Owop.Game;
using Owop.Network;

namespace Owop.Client;

/// <summary>Represents the main client interface to OWOP world connections.</summary>
public interface IOwopClient : IDisposable
{
    /// <summary>The client options.</summary>
    ClientOptions Options { get; }

    /// <summary>The server API values. Re-fetched on each <see cref="Connect"/> call. </summary>
    ServerInfo? ServerInfo { get; }

    /// <summary>A mapping of world names to their established client connections.</summary>
    IReadOnlyDictionary<string, IReadOnlySet<IWorldConnection>> Connections { get; }

    /// <summary>The central client logger.</summary>
    ILogger Logger { get; }

    /// <summary>Connects to a world.</summary>
    /// <param name="world">The world name.</param>
    /// <param name="options">The world connection options.</param>
    /// <returns>
    /// Whether the connection was successful. If <c>false</c>, the connecting IP has reached the 
    /// <see cref="ServerInfo.MaxConnectionsPerIp"/>.
    /// </returns>
    Task<bool> Connect(string world = "main", ConnectionOptions? options = null);

    /// <summary>Terminates all connections to a world with the given name.</summary>
    /// <param name="world">The world name.</param>
    /// <returns>Whether there were any terminated connections.</returns>
    Task<bool> DisconnectAll(string world = "main");

    /// <summary>Terminates all connections to all worlds.</summary>
    Task Destroy();

    /// <summary>Fires when the client connects to a world.</summary>
    event Action<ConnectEventArgs>? Connected;

    /// <summary>Fires when the client is fully initialized within a world.</summary>
    event Action<IWorld>? Ready;

    /// <summary>Fires when the client can safely send chat messages within a world.</summary>
    event Action<IWorld>? ChatReady;

    /// <summary>Fires when a player sends a chat message.</summary>
    event Action<ChatEventArgs>? Chat;

    /// <summary>Fires when a player sends a private message to the client.</summary>
    event Action<TellEventArgs>? Tell;

    /// <summary>Fires when the client receives a generic server message.</summary>
    event Action<ServerMessage>? ServerMessage;

    /// <summary>Fires when a player connects to a world.</summary>
    event Action<IPlayer>? PlayerConnected;

    /// <summary>Fires when a player disconnects from a world.</summary>
    event Action<IPlayer>? PlayerDisconnected;

    /// <summary>Fired when the client player is teleported.</summary>
    event Action<TeleportEventArgs>? Teleported;

    /// <summary>Fired when a '/whois' command is processed.</summary>
    event Action<WhoisEventArgs>? Whois;

    /// <summary>Fired when a pixel is placed in the world.</summary>
    event Action<PixelPlacedEventArgs>? PixelPlaced;

    /// <summary>Fired when a chunk is loaded.</summary>
    event Action<ChunkEventArgs>? ChunkLoaded;

    /// <summary>Fired when a chunk's protection state changes.</summary>
    event Action<ChunkEventArgs>? ChunkProtectionChanged;

    /// <summary>Fired when the client disconnects from a world.</summary>
    event Action<IWorld>? Disconnecting;

    /// <summary>Fired when the client is destroyed.</summary>
    event Action? Destroying;

    /// <summary>Creates an OWOP client.</summary>
    /// <param name="options">The client options.</param>
    /// <param name="loggerFactory">The client logger factory.</param>
    /// <returns>The created client.</returns>
    static IOwopClient Create(ClientOptions? options = null, ILoggerFactory? loggerFactory = null)
        => new OwopClient(options, loggerFactory);
}

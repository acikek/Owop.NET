using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Owop.Game;
using Owop.Network;
using Websocket.Client;

namespace Owop.Client;

/// <summary>An <see cref="IWorldConnection"/> implementation.</summary>
public class WorldConnection : IWorldConnection
{
    /// <summary>Verification bytes to append to the connection message.</summary>
    public const short WorldVerification = 25565;

    /// <summary>Internal connection thread exit event.</summary>
    private readonly ManualResetEvent _exitEvent = new(false);

    /// <summary>Internal client instance.</summary>
    public readonly OwopClient _client;

    /// <summary>Internal world instance.</summary>
    public readonly World _world;

    /// <summary>Connection message to generate and (re)send.</summary>
    public readonly byte[] ConnectionMessage;

    /// <inheritdoc/>
    public WebsocketClient Socket { get; }

    /// <inheritdoc/>
    public IOwopClient Client => _client;

    /// <inheritdoc/>
    public ConnectionOptions? Options { get; }

    /// <inheritdoc/>
    public IWorld World => _world;

    /// <inheritdoc/>
    public ILogger Logger { get; }

    /// <summary>Constructs a new <see cref="WorldConnection"/>.</summary>
    /// <param name="name">The world name to connect to.</param>
    /// <param name="options">The connection options.</param>
    /// <param name="client">The client connecting to the world.</param>
    /// <param name="index">The number of previous connections to the world.</param>
    public WorldConnection(string name, ConnectionOptions? options, OwopClient client, int index)
    {
        Socket = new(new Uri(client.Options.SocketUrl));
        Options = options;
        _client = client;
        _world = new(name, this);
        ConnectionMessage = GetConnectionMessage(name);
        Logger = client.LoggerFactory.CreateLogger($"Owop.Net.{name}.{index} ");
    }

    /// <summary>Creates a connection message from a world name.</summary>
    /// <param name="world">The cleaned world name.</param>
    /// <returns>The connection message.</returns>
    private static byte[] GetConnectionMessage(string world)
    {
        string fixedLength = world[..Math.Min(world.Length, 24)];
        var bytes = Encoding.ASCII.GetBytes(fixedLength);
        var list = new List<byte>(bytes);
        list.AddRange(BitConverter.GetBytes(WorldVerification));
        return [.. list];
    }

    /// <inheritdoc/>
    public async Task Send(byte[] message) => await Task.Run(() => Socket.Send(message));

    /// <inheritdoc/>
    public async Task Send(string message) => await Task.Run(() => Socket.Send(message));

    /// <summary>Connects to the world, handling reconnects when necessary.</summary>
    public void Connect()
    {
        try
        {
            //Socket.ReconnectTimeout = TimeSpan.FromSeconds(5);
            Socket.ReconnectionHappened.Subscribe(info =>
            {
                Logger.LogDebug($"Reconnecting... ({info.Type})");
                Socket.SendInstant(ConnectionMessage);
                if (info.Type == ReconnectionType.Lost)
                {
                    //Socket.MessageReceived.Subscribe(msg => _client.HandleMessage(msg, _world));
                }
            });
            Socket.MessageReceived.Subscribe(msg => _client.HandleMessage(msg, _world));
            Socket.Start();
            Task.Run(() => Socket.Send(ConnectionMessage));
            _exitEvent.WaitOne();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error while connecting to world '{_world.Name}':");
        }
    }

    /// <summary>Checks whether the client player can perform an operation.</summary>
    /// <param name="rank">The lowest rank required for the operation.</param>
    /// <exception cref="InvalidOperationException">If the socket isn't running or the player's rank is insufficient.</exception>
    public void CheckInteraction(PlayerRank rank = PlayerRank.None)
    {
        if (!Socket.IsRunning)
        {
            throw new InvalidOperationException($"Interaction failed in world '{World.Name}': socket is not connected");
        }
        if (World.ClientPlayer.Rank < rank)
        {
            throw new InvalidOperationException($"Insufficient permissions in world '{World.Name}': must be rank {rank} or higher (is {World.ClientPlayer.Rank})");
        }
    }

    /// <summary>Sends the local client player data to the server.</summary>
    public async Task SendPlayerData() => await Send(OwopProtocol.EncodePlayer(_world._clientPlayer));

    /// <summary>Terminates the client connection.</summary>
    public async Task DisconnectInternal()
    {
        _client.InvokeDisconnect(_world);
        await Socket.Stop(WebSocketCloseStatus.NormalClosure, "Client.Disconnect()");
        _exitEvent.Set();
    }

    /// <inheritdoc/>
    public async Task Disconnect()
    {
        await DisconnectInternal();
        if (_client.Connections[_world.Name] is HashSet<IWorldConnection> mutable)
        {
            mutable.Remove(this);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Socket.Dispose();
        _exitEvent.Set();
        GC.SuppressFinalize(this);
    }
}

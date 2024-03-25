using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Owop.Game;
using Owop.Network;
using Websocket.Client;

namespace Owop.Client;

public class WorldConnection : IWorldConnection
{
    public const short WorldVerification = 25565;

    private readonly ManualResetEvent _exitEvent = new(false);
    public readonly OwopClient _client;
    public readonly World _world;
    public readonly byte[] ConnectionMessage;

    public WebsocketClient Socket { get; }
    public IOwopClient Client => _client;
    public ConnectionOptions? Options { get; }
    public IWorld World => _world;
    public ILogger Logger { get; }

    public WorldConnection(string name, ConnectionOptions? options, OwopClient client)
    {
        Socket = new(new Uri(client.Options.SocketUrl));
        Options = options;
        _client = client;
        _world = new(name, this);
        ConnectionMessage = GetConnectionMessage(name);
        Logger = client.LoggerFactory.CreateLogger($"Owop.Net.{name}");
    }

    private static byte[] GetConnectionMessage(string world)
    {
        string fixedLength = world[..Math.Min(world.Length, 24)];
        var bytes = Encoding.ASCII.GetBytes(fixedLength);
        var list = new List<byte>(bytes);
        list.AddRange(BitConverter.GetBytes(WorldVerification));
        return [.. list];
    }

    public async Task Send(byte[] message) => await Task.Run(() => Socket.Send(message));
    public async Task Send(string message) => await Task.Run(() => Socket.Send(message));

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

    public async Task SendPlayerData()
        => await Send(OwopProtocol.EncodePlayer(_world._clientPlayer));

    public async Task Disconnect()
    {
        _client.InvokeDisconnect(_world);
        await Socket.Stop(WebSocketCloseStatus.NormalClosure, "Client.Disconnect()");
        _exitEvent.Set();
    }

    public void Dispose()
    {
        Socket.Dispose();
        _exitEvent.Set();
        GC.SuppressFinalize(this);
    }
}

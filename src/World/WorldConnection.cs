using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Owop.Client;
using Owop.Network;
using Websocket.Client;

namespace Owop;

// TODO: Expose more correctly
public class WorldConnection : IWorldConnection
{
    private readonly ManualResetEvent _exitEvent = new(false);
    private readonly OwopClient _client;
    private readonly World _world;

    public WebsocketClient Socket { get; }
    public IOwopClient Client => _client;
    public IWorld World => _world;
    public ILogger Logger { get; }

    public WorldConnection(string name, OwopClient client)
    {
        Socket = new(new Uri(client.Options.SocketUrl));
        _client = client;
        _world = new(name, this);
        Logger = client.LoggerFactory.CreateLogger($"Owop.Net.{name}");
    }

    private byte[] GetConnectionMessage(string world)
    {
        string fixedLength = world[..Math.Min(world.Length, 24)];
        var bytes = Encoding.ASCII.GetBytes(fixedLength);
        var list = new List<byte>(bytes);
        list.AddRange(BitConverter.GetBytes(Client.Options.WorldVerification));
        return [.. list];
    }

    public async Task Send(byte[] message) => await Task.Run(() => Socket.Send(message));
    public async Task Send(string message) => await Task.Run(() => Socket.Send(message));

    public void Connect(string world)
    {
        var connectMsg = GetConnectionMessage(world);
        //Socket.ReconnectTimeout = TimeSpan.FromSeconds(5);
        Socket.ReconnectionHappened.Subscribe(info =>
        {
            Logger.LogDebug($"Reconnecting... ({info.Type})");
            Socket.SendInstant(connectMsg);
            if (info.Type == ReconnectionType.Lost)
            {
                Socket.MessageReceived.Subscribe(msg => _client.HandleMessage(msg, _world));
            }
        });
        Socket.MessageReceived.Subscribe(msg => _client.HandleMessage(msg, _world));
        Socket.Start();
        Task.Run(() => Socket.Send(connectMsg));
        _exitEvent.WaitOne();
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

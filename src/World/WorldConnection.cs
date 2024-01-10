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
public class WorldConnection : IDisposable
{
    public WebsocketClient Socket { get; private set; }
    private readonly ManualResetEvent _exitEvent = new(false);

    public readonly OwopClient Client;
    public readonly ILogger Logger;

    private readonly WorldData _world;
    private readonly Action<ResponseMessage, WorldData> _messageHandler;
    private readonly Action<World> _disconnectHandler;

    public World World => _world;

    public WorldConnection(string name, OwopClient client, ILoggerFactory loggerFactory, Action<ResponseMessage, WorldData> messageHandler, Action<World> disconnectHandler)
    {
        Socket = new(new Uri(client.Options.SocketUrl));
        Client = client;
        Logger = loggerFactory.CreateLogger($"Owop.Net.{name}");
        _world = new(name, this);
        _messageHandler = messageHandler;
        _disconnectHandler = disconnectHandler;
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
                Socket.MessageReceived.Subscribe(msg => _messageHandler(msg, _world));
            }
        });
        Socket.MessageReceived.Subscribe(msg => _messageHandler(msg, _world));
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
        if (_world.ClientPlayerData.Rank < rank)
        {
            throw new InvalidOperationException($"Insufficient permissions in world '{World.Name}': must be rank {rank} or higher (is {_world.ClientPlayerData.Rank})");
        }
    }

    public async Task SendPlayerData()
        => await Send(OwopProtocol.EncodePlayer(_world.ClientPlayerData));

    public async Task Disconnect()
    {
        _disconnectHandler(World);
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

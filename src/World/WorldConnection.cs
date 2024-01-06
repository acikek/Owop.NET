using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Owop.Client;
using Owop.Network;
using Websocket.Client;

namespace Owop;

public class WorldConnection : IDisposable
{
    public readonly WebsocketClient Socket;
    private readonly ManualResetEvent _exitEvent = new(false);

    private readonly WorldData _world;
    public readonly OwopClient Client;
    private readonly Action<ResponseMessage, WorldData> _messageHandler;

    public WorldConnection(string name, OwopClient client, Action<ResponseMessage, WorldData> messageHandler)
    {
        Socket = new(new Uri(client.Options.SocketUrl));
        _world = new(name, this);
        Client = client;
        _messageHandler = messageHandler;
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
            Console.WriteLine(info.Type);
            Client.Logger.LogDebug($"Reconnecting to world... (type: {info.Type})");
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

    public void CheckRank(PlayerRank rank)
    {
        if (_world.ClientPlayerData.Rank < rank)
        {
            throw new InvalidOperationException($"Insufficient permissions: must be rank '{rank}' or higher (is {_world.ClientPlayerData.Rank})");
        }
    }

    public async Task SendPlayerData()
        => await Send(OwopProtocol.EncodePlayer(_world.ClientPlayerData));

    public async Task Disconnect()
    {
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

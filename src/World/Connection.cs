using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace Owop;

public partial class WorldConnection : IDisposable
{
    public readonly WebsocketClient Socket;
    private readonly ManualResetEvent ExitEvent = new(false);

    private readonly WorldData World;
    private readonly OwopClient Client;
    private readonly Action<ResponseMessage, WorldData> MessageHandler;

    public WorldConnection(string name, OwopClient client, Action<ResponseMessage, WorldData> messageHandler)
    {
        Socket = new(new Uri(client.Options.Url));
        World = new(name, this);
        Client = client;
        MessageHandler = messageHandler;
    }

    public static string CleanId(string world)
    {
        var span = world.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '.');
        return new(span.ToArray());
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
        Socket.ReconnectionHappened.Subscribe(info =>
        {
            Client.Logger.LogDebug($"Reconnecting to world... (type: {info.Type})");
            Socket.Send(connectMsg);
        });
        Socket.MessageReceived.Subscribe(msg => MessageHandler(msg, World));
        Socket.Start();
        Task.Run(() => Socket.Send(connectMsg));
        ExitEvent.WaitOne();
    }

    public async Task Disconnect()
    {
        await Socket.Stop(WebSocketCloseStatus.NormalClosure, "Client.Disconnect()");
        ExitEvent.Set();
    }

    public void Dispose()
    {
        Socket.Dispose();
        ExitEvent.Set();
        GC.SuppressFinalize(this);
    }
}

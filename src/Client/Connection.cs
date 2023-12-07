using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace Owop.Client;

public class ConnectionManager : IDisposable
{
    public const string URL = "wss://ourworldofpixels.com";
    public const ushort WORLD_VERIFICATION = 25565;

    public readonly WebsocketClient Socket;
    private readonly ManualResetEvent ExitEvent = new(false);

    private readonly OwopClient Client;
    private readonly Action<ResponseMessage> MessageHandler;

    // TODO: connection options (keep this constructor)
    public ConnectionManager(OwopClient client, Action<ResponseMessage> messageHandler)
    {
        Socket = new WebsocketClient(new Uri(URL));
        Client = client;
        MessageHandler = messageHandler;
    }

    public static string GetValidWorldId(string world)
    {
        var span = world.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '.');
        return new(span.ToArray());
    }

    public static byte[] GetConnectionMessage(string world = "main")
    {
        string clean = GetValidWorldId(world);
        string fixedLength = clean[..Math.Min(world.Length, 24)];
        var bytes = Encoding.ASCII.GetBytes(fixedLength);
        var list = new List<byte>(bytes);
        list.AddRange(BitConverter.GetBytes(WORLD_VERIFICATION));
        return [.. list];
    }

    public void Connect(string world = "main")
    {
        var connectMsg = GetConnectionMessage(world);
        Socket.ReconnectionHappened.Subscribe(info =>
        {
            Client.Logger.LogDebug($"Reconnecting to world... (type: {info.Type})");
            Socket.Send(connectMsg);
        });
        Socket.MessageReceived.Subscribe(MessageHandler);
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

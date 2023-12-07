using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace Owop;

public partial class Client : IDisposable
{
    public const string URL = "wss://ourworldofpixels.com";
    public const ushort WORLD_VERIFICATION = 25565;

    public readonly WebsocketClient Connection;
    private readonly ManualResetEvent ExitEvent = new(false);
    public bool Connected { get; private set; } = false;

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

    // TODO: connection options
    public void Connect(string world = "main")
    {
        var connectMsg = GetConnectionMessage(world);
        Connection.ReconnectionHappened.Subscribe(info =>
        {
            Logger.LogDebug($"Reconnecting to world... (type: {info.Type})");
            Connection.Send(connectMsg);
        });
        Connection.MessageReceived.Subscribe(OnMessageReceived);
        Connection.Start();
        Task.Run(() => Connection.Send(connectMsg));
        ExitEvent.WaitOne();
    }

    public async Task Disconnect()
    {
        await Connection.Stop(WebSocketCloseStatus.NormalClosure, "Client.Disconnect()");
        ExitEvent.Set();
    }

    public void Dispose()
    {
        Connection.Dispose();
        ExitEvent.Set();
        GC.SuppressFinalize(this);
    }
}

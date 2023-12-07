using Microsoft.Extensions.Logging;
using Owop.Client;

namespace Owop;

public partial class OwopClient : IDisposable
{
    public ILogger Logger;

    private PlayerData PlayerData = PlayerData.Empty;
    public Player Player => PlayerData;
    public string? Nickname { get; private set; }

    public readonly ConnectionManager Connection;
    public bool Connected { get; private set; }

    public event EventHandler? Ready;
    public event EventHandler? ChatReady;
    public event EventHandler<ChatMessage>? ChatEvent;

    public World? World { get; private set; }

    public OwopClient()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        Logger = factory.CreateLogger("OWOP.NET");
        Connection = new ConnectionManager(this, HandleMessage);
    }

    public void Connect(string world = "main") => Connection.Connect(world);
    public async Task Disconnect() => await Connection.Disconnect();

    public void Dispose()
    {
        Connection.Dispose();
        GC.SuppressFinalize(this);
    }
}

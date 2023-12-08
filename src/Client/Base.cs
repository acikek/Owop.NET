using Microsoft.Extensions.Logging;

namespace Owop;

public partial class OwopClient : IDisposable
{
    public ILogger Logger;

    private PlayerData PlayerData = PlayerData.Empty;
    public Player Player => PlayerData;
    public string? Nickname { get; private set; }
    public PlayerRank Rank { get; private set; } = PlayerRank.None;

    public readonly ConnectionManager Connection;
    public bool Connected { get; private set; }

    public event EventHandler? Ready;
    public event EventHandler? ChatReady;
    public event EventHandler<ChatMessage>? Chat;

    public string? World;
    private readonly Dictionary<uint, PlayerData> WorldPlayerData = [];
    public readonly Dictionary<uint, Player> Players = [];

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

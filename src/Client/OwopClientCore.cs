using Microsoft.Extensions.Logging;

namespace Owop.Client;

public partial class OwopClient : IOwopClient
{
    public readonly ILoggerFactory LoggerFactory;
    public ClientOptions Options { get; }
    public Dictionary<string, IWorldConnection> Connections { get; } = [];
    public ILogger Logger { get; }

    public OwopClient(ClientOptions? options, ILoggerFactory? loggerFactory)
    {
        Options = options ?? new();
        LoggerFactory = loggerFactory ?? Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
        Logger = LoggerFactory.CreateLogger("Owop.Net");
        _httpClient = new();
        _messageBuffer = [];
    }

    private string CleanWorldId(string world)
    {
        string fixedLength = world[..Math.Min(world.Length, Options.MaxWorldNameLength)];
        string lower = fixedLength.ToLower();
        var span = lower.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '.');
        return new(span.ToArray());
    }

    public async Task<ConnectResult> Connect(string world = "main")
    {
        var serverInfo = await FetchServerInfo();
        if (serverInfo is null || serverInfo.ClientConnections >= serverInfo.MaxConnectionsPerIp)
        {
            return ConnectResult.LimitReached;
        }
        string clean = CleanWorldId(world);
        if (Connections.ContainsKey(clean))
        {
            return ConnectResult.Exists;
        }
        Logger.LogDebug($"Connecting to world '{clean}'...");
        WorldConnection connection = new(clean, this);
        Connections[clean] = connection;
        connection.Connect(clean);
        return ConnectResult.Activated;
    }

    public async Task<bool> Disconnect(string world = "main")
    {
        string clean = CleanWorldId(world);
        if (Connections.Remove(clean, out var connection))
        {
            await connection.Disconnect();
            return true;
        }
        return false;
    }

    public async Task Destroy()
    {
        Destroying?.Invoke(this, EventArgs.Empty);
        foreach (var connection in Connections.Values)
        {
            await connection.Disconnect();
        }
    }

    public void Dispose()
    {
        Destroying?.Invoke(this, EventArgs.Empty);
        foreach (var connection in Connections.Values)
        {
            ((WorldConnection)connection).Dispose();
        }
        LoggerFactory.Dispose();
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}

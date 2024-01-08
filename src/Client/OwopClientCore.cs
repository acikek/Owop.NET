using Microsoft.Extensions.Logging;

namespace Owop.Client;

public enum ConnectResult
{
    LimitReached,
    Exists,
    Activated
}

public partial class OwopClient : IDisposable
{
    private readonly ILoggerFactory _loggerFactory;
    public ILogger Logger;

    public readonly ClientOptions Options;
    public readonly Dictionary<string, WorldConnection> Connections = [];

    public OwopClient(ClientOptions? options = null, ILoggerFactory? loggerFactory = null)
    {
        if (loggerFactory is null)
        {
            using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            loggerFactory = factory;
        }
        _loggerFactory = loggerFactory;
        Logger = _loggerFactory.CreateLogger("Owop.Net");
        Options = options ?? new ClientOptions();
        _httpClient = new();
        _messageBuffer = [];
    }

    private string CleanWorldId(string world)
    {
        string fixedLength = world[..Math.Min(world.Length, Options.MaxWorldNameLength)];
        var span = fixedLength.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '.');
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
        WorldConnection connection = new(clean, this, _loggerFactory, HandleMessage, world => Disconnecting?.Invoke(this, world));
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
            connection.Dispose();
        }
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}

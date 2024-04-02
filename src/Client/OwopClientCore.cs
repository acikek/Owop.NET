using Microsoft.Extensions.Logging;

namespace Owop.Client;

public partial class OwopClient : IOwopClient
{
    public const int MaxWorldNameLength = 24;

    private readonly Dictionary<string, IReadOnlySet<IWorldConnection>> _connections = [];

    public readonly ILoggerFactory LoggerFactory;
    public ClientOptions Options { get; }
    public IReadOnlyDictionary<string, IReadOnlySet<IWorldConnection>> Connections => _connections;
    public ILogger Logger { get; }

    public OwopClient(ClientOptions? options, ILoggerFactory? loggerFactory)
    {
        Options = options ?? new();
        LoggerFactory = loggerFactory ?? Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
        Logger = LoggerFactory.CreateLogger("Owop.Net");
        _httpClient = new();
        _messageBuffer = [];
    }

    private static string CleanWorldId(string world)
    {
        string fixedLength = world[..Math.Min(world.Length, MaxWorldNameLength)];
        string lower = fixedLength.ToLower();
        var span = lower.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '.');
        return new(span.ToArray());
    }

    public async Task<bool> Connect(string world = "main", ConnectionOptions? options = null)
    {
        var serverInfo = await FetchServerInfo();
        if (serverInfo is null || serverInfo.ClientConnections >= serverInfo.MaxConnectionsPerIp)
        {
            return false;
        }
        string clean = CleanWorldId(world);
        Logger.LogDebug($"Connecting to world '{clean}'...");
        var connections = Connections.TryGetValue(clean, out IReadOnlySet<IWorldConnection>? set) ? set : null;
        WorldConnection connection = new(clean, options, this, connections?.Count ?? 0);
        if (connections is HashSet<IWorldConnection> mutable)
        {
            mutable.Add(connection);
        }
        else
        {
            _connections.Add(clean, new HashSet<IWorldConnection>([connection]));
        }
        connection.Connect();
        return true;
    }

    public async Task<bool> DisconnectAllInternal(string world)
    {
        if (!_connections.Remove(world, out var connections))
        {
            return false;
        }
        foreach (var connection in connections)
        {
            if (connection is WorldConnection mutable)
            {
                await mutable.DisconnectInternal();
            }
        }
        return true;
    }

    public async Task<bool> DisconnectAll(string world = "main") => await DisconnectAllInternal(CleanWorldId(world));

    public async Task Destroy()
    {
        Destroying?.Invoke();
        foreach (string world in Connections.Keys.ToList())
        {
            await DisconnectAllInternal(world);
        }
    }

    public void Dispose()
    {
        Destroying?.Invoke();
        foreach (var connection in Connections.Values)
        {
            ((WorldConnection)connection).Dispose();
        }
        LoggerFactory.Dispose();
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}

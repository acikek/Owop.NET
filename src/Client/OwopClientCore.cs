using Microsoft.Extensions.Logging;

namespace Owop.Client;

/// <summary>Core implementation of an <see cref="IOwopClient"/>.</summary>
public partial class OwopClient : IOwopClient
{
    /// <summary>The length cap for world names.</summary>
    public const int MaxWorldNameLength = 24;

    /// <summary>Internal world connections.</summary>
    private readonly Dictionary<string, IReadOnlySet<IWorldConnection>> _connections = [];

    /// <summary>Internal world connection count tracker.</summary>
    private readonly Dictionary<string, int> _connectionCounts = [];

    /// <summary>The client logger factory.</summary>
    public readonly ILoggerFactory LoggerFactory;

    /// <inheritdoc/>
    public ClientOptions Options { get; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IReadOnlySet<IWorldConnection>> Connections => _connections;

    /// <inheritdoc/>
    public ILogger Logger { get; }

    /// <summary>Constructs an <see cref="OwopClient"/>.</summary>
    /// <param name="options">The client options.</param>
    /// <param name="loggerFactory">The client logger factory.</param>
    public OwopClient(ClientOptions? options, ILoggerFactory? loggerFactory)
    {
        Options = options ?? new();
        LoggerFactory = loggerFactory ?? Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
        Logger = LoggerFactory.CreateLogger("Owop.NET");
        _httpClient = new();
        _messageBuffer = [];
    }

    /// <summary>Cleans a world name to be OWOP-safe.</summary>
    /// <param name="world">The input name.</param>
    /// <returns>The cleaned name.</returns>
    private static string CleanWorldName(string world)
    {
        string fixedLength = world[..Math.Min(world.Length, MaxWorldNameLength)];
        string lower = fixedLength.ToLower();
        var span = lower.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '.');
        return new(span.ToArray());
    }

    /// <inheritdoc/>
    public async Task<bool> Connect(string world = "main", ConnectionOptions? options = null)
    {
        var serverInfo = await FetchServerInfo();
        if (serverInfo is null || serverInfo.ClientConnections >= serverInfo.MaxConnectionsPerIp)
        {
            return false;
        }
        string clean = CleanWorldName(world);
        Logger.LogDebug($"Connecting to world '{clean}'...");
        int index = _connectionCounts.GetValueOrDefault(clean, -1) + 1;
        WorldConnection connection = new(clean, options, this, index);
        if (Connections.TryGetValue(clean, out IReadOnlySet<IWorldConnection>? set) && set is HashSet<IWorldConnection> mutable)
        {
            mutable.Add(connection);
        }
        else
        {
            _connections.Add(clean, new HashSet<IWorldConnection>([connection]));
            _connectionCounts.Add(clean, 0);
        }
        connection.Connect();
        return true;
    }

    /// <summary>Terminates all connections to a world with the given name.</summary>
    /// <param name="world">The world name.</param>
    /// <returns>Whether there were any terminated connections.</returns>
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

    /// <inheritdoc/>
    public async Task<bool> DisconnectAll(string world = "main") => await DisconnectAllInternal(CleanWorldName(world));

    /// <inheritdoc/>
    public async Task Destroy()
    {
        Destroying?.Invoke();
        foreach (string world in Connections.Keys.ToList())
        {
            await DisconnectAllInternal(world);
        }
    }

    /// <inheritdoc/>
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

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Owop.Network;

namespace Owop;

public partial class World : IWorld
{
    public ClientPlayer _clientPlayer;
    public WorldConnection _connection;
    public ConcurrentDictionary<int, TaskCompletionSource<WhoisData>> WhoisQueue = [];

    public string Name { get; }
    public Dictionary<int, IPlayer> Players { get; } = [];
    public IClientPlayer ClientPlayer => _clientPlayer;
    public bool IsChatReady { get; set; }
    public bool IsPasswordProtected { get; set; } // TODO: Implement
    public IWorldConnection Connection => _connection;
    public ILogger Logger => Connection.Logger;

    public bool Connected = false;
    public bool Initialized = false;

    public World(string name, WorldConnection connection)
    {
        Name = name;
        _connection = connection;
        _clientPlayer = new(this);
    }
}

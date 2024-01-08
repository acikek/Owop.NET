using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Owop.Network;

namespace Owop;

public class WorldData
{
    public readonly string Name;
    public Dictionary<int, WorldPlayerData<Player>> PlayerData = [];
    public Dictionary<int, Player> Players = [];
    public ClientPlayerData ClientPlayerData;
    public ConcurrentDictionary<int, TaskCompletionSource<WhoisData>> WhoisQueue = [];

    public WorldConnection Connection;
    public World World;
    public bool Connected = false;
    public bool PlayersInitialized = false;
    public bool Initialized = false;

    public WorldData(string name, WorldConnection connection)
    {
        Name = name;
        ClientPlayerData = new(this);
        Connection = connection;
        World = new(this);
    }
}

using Microsoft.Extensions.Logging;

namespace Owop;

public partial class World(WorldData data)
{
    public const int ChunkSize = 16;

    private readonly WorldData _instance = data;

    public string Name => _instance.Name;
    public Dictionary<int, Player> Players => _instance.Players;
    public ClientPlayer ClientPlayer => _instance.ClientPlayerData;
    public ILogger Logger => _instance.Connection.Logger;

    public IPlayer GetPlayerById(int id) => Players.TryGetValue(id, out var player) ? player : ClientPlayer;

    public static implicit operator World(WorldData data) => data.World;
}

namespace Owop;

public class WorldData
{
    public readonly string Name;
    public Dictionary<int, PlayerData> PlayerData = [];
    public Dictionary<int, Player> Players = [];
    public string? ClientNickname;
    public PlayerRank ClientRank = PlayerRank.Player;
    public PlayerData ClientPlayerData;

    public WorldConnection Connection;
    public bool Connected = false;

    public WorldData(string name, WorldConnection connection)
    {
        Name = name;
        ClientPlayerData = new PlayerData(this);
        Connection = connection;
    }
}

public partial struct World
{
    private WorldData Instance;

    public readonly string Name => Instance.Name;
    public readonly Dictionary<int, Player> Players => Instance.Players;
    public readonly string? ClientNickname => Instance.ClientNickname;
    public readonly PlayerRank ClientRank => Instance.ClientRank;
    public readonly Player ClientPlayer => Instance.ClientPlayerData;

    public static implicit operator World(WorldData data) => new() { Instance = data };
}

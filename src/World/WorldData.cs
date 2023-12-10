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

namespace Owop;

public partial struct World
{
    public const int CHUNK_SIZE = 16;

    private WorldData Instance;

    public readonly string Name => Instance.Name;
    public readonly Dictionary<int, Player> Players => Instance.Players;
    public readonly string? ClientNickname => Instance.ClientNickname;
    public readonly PlayerRank ClientRank => Instance.ClientRank;
    public readonly Player ClientPlayer => Instance.ClientPlayerData;

    public static implicit operator World(WorldData data) => new() { Instance = data };
}

namespace Owop;

public readonly partial struct World(WorldData data)
{
    public const int CHUNK_SIZE = 16;

    private readonly WorldData Instance = data;

    public readonly string Name => Instance.Name;
    public readonly Dictionary<int, Player> Players => Instance.Players;
    public readonly ClientPlayer ClientPlayer => Instance.ClientPlayerData;

    public static implicit operator World(WorldData data) => new(data);
}

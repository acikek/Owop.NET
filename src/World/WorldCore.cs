namespace Owop;

public readonly partial struct World(WorldData data)
{
    public const int CHUNK_SIZE = 16;

    private readonly WorldData _instance = data;

    public readonly string Name => _instance.Name;
    public readonly Dictionary<int, Player> Players => _instance.Players;
    public readonly ClientPlayer ClientPlayer => _instance.ClientPlayerData;

    public static implicit operator World(WorldData data) => new(data);
}

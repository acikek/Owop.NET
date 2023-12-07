namespace Owop.Game;

public record World(string Name)
{
    private readonly Dictionary<uint, PlayerData> PlayerData = [];
    public readonly Dictionary<uint, Player> Players = [];
}

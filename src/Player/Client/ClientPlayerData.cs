namespace Owop;

public class ClientPlayerData(WorldData worldData) : WorldPlayerData(worldData)
{
    public string? Nickname;
    public PlayerRank Rank;
}

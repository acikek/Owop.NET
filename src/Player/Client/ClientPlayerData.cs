namespace Owop;

public class ClientPlayerData : WorldPlayerData<ClientPlayer>
{
    public string? Nickname;
    public PlayerRank Rank;

    public ClientPlayerData(WorldData worldData) : base(worldData, null!)
    {
        Player = new ClientPlayer(this);
    }
}

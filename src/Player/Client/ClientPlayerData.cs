namespace Owop;

public class ClientPlayerData : WorldPlayerData<ClientPlayer>
{
    public string? Nickname;
    public PlayerRank Rank;
    public PixelBucketData BucketData;

    public ClientPlayerData(WorldData worldData) : base(worldData, null!)
    {
        Player = new ClientPlayer(this);
        BucketData = PixelBucketData.Empty;
    }
}

using Owop.Util;

namespace Owop;

public class ClientPlayerData : WorldPlayerData<ClientPlayer>
{
    public string? Nickname;
    public PlayerRank Rank;
    public BucketData PixelBucketData;

    public ClientPlayerData(WorldData worldData) : base(worldData, null!)
    {
        Player = new ClientPlayer(this);
        PixelBucketData = BucketData.Empty;
    }

    public void SetRank(PlayerRank rank)
    {
        Rank = rank;
        bool admin = Rank == PlayerRank.Admin;
        PixelBucketData.Infinite = admin;
    }
}

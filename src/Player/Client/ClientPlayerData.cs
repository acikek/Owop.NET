using System.Collections.Concurrent;
using Owop.Util;

namespace Owop;

public class ClientPlayerData : WorldPlayerData<ClientPlayer>
{
    public string? Nickname;
    private PlayerRank _rank;
    public BucketData PixelBucketData;
    public BucketData ChatBucketData;

    public ClientPlayerData(WorldData worldData) : base(worldData, null!)
    {
        Player = new ClientPlayer(this);
        PixelBucketData = BucketData.Empty;
        ChatBucketData = new(4, 6, false, true);
    }

    public PlayerRank Rank
    {
        get => _rank;
        set
        {
            _rank = value;
            bool admin = Rank == PlayerRank.Admin;
            PixelBucketData.Infinite = admin;
            ChatBucketData.Infinite = admin;
        }
    }
}

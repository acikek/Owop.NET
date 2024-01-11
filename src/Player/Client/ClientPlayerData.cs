using System.Collections.Concurrent;
using Owop.Util;

namespace Owop;

public class ClientPlayerData : WorldPlayerData<ClientPlayer>
{
    public string? Nickname;
    private PlayerRank _rank;
    public Bucket PixelBucket;
    public Bucket ChatBucket;

    public ClientPlayerData(WorldData worldData) : base(worldData, null!)
    {
        Player = new ClientPlayer(this);
        PixelBucket = Bucket.Empty;
        ChatBucket = new(4, 6, false, true);
    }

    public PlayerRank Rank
    {
        get => _rank;
        set
        {
            _rank = value;
            bool admin = Rank == PlayerRank.Admin;
            PixelBucket.Infinite = admin;
            ChatBucket.Infinite = admin;
        }
    }
}

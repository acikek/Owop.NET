using System.Drawing;
using Owop.Util;

namespace Owop;

public partial class ClientPlayer(ClientPlayerData data) : IPlayer
{
    private readonly ClientPlayerData _instance = data;

    public World World => _instance.WorldData;
    public Position Pos => _instance.Pos;
    public Position WorldPos => _instance.WorldPos;
    public PlayerTool Tool => _instance.Tool;
    public PlayerRank Rank => _instance.Rank;
    public int Id => _instance.Id;
    public string? Nickname => _instance.Nickname;
    public Color Color => _instance.Color;
    public Bucket PixelBucket => _instance.PixelBucketData;
    public bool IsClient => true;

    public static implicit operator ClientPlayer(ClientPlayerData data) => data.Player;

    private async Task Send()
        => await _instance.WorldData.Connection.SendPlayerData();
}

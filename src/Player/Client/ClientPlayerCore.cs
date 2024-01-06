using System.Drawing;

namespace Owop;

public partial class ClientPlayer(ClientPlayerData data) : IPlayer
{
    private readonly ClientPlayerData _instance = data;

    public World World => _instance.WorldData;
    public Point Pos => _instance.Pos;
    public Point WorldPos => _instance.WorldPos;
    public PlayerTool Tool => _instance.Tool;
    public PlayerRank Rank => _instance.Rank;
    public int Id => _instance.Id;
    public string? Nickname => _instance.Nickname;
    public Color Color => _instance.Color;

    public static implicit operator ClientPlayer(ClientPlayerData data) => data.Player;
}

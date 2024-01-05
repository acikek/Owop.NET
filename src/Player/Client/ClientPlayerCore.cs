using System.Drawing;

namespace Owop;

public readonly partial struct ClientPlayer(ClientPlayerData data)
{
    private readonly ClientPlayerData _instance = data;

    public readonly World World => _instance.WorldData;
    public readonly Point Pos => _instance.Pos;
    public readonly Point WorldPos => _instance.WorldPos;
    public readonly PlayerTool Tool => _instance.Tool;
    public readonly PlayerRank Rank => _instance.Rank;
    public readonly int Id => _instance.Id;
    public readonly string? Nickname => _instance.Nickname;
    public readonly Color Color => _instance.Color;

    public static implicit operator ClientPlayer(ClientPlayerData data) => new(data);
    public static implicit operator Player(ClientPlayer player) => new(player._instance);
}

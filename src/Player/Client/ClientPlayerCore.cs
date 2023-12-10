using System.Drawing;

namespace Owop;

public readonly partial struct ClientPlayer(ClientPlayerData data)
{
    private readonly ClientPlayerData Instance = data;

    public readonly World World => Instance.World;
    public readonly Point Pos => Instance.Pos;
    public readonly Point WorldPos => Instance.WorldPos;
    public readonly PlayerTool Tool => Instance.Tool;
    public readonly PlayerRank Rank => Instance.Rank;
    public readonly int Id => Instance.Id;
    public readonly string? Nickname => Instance.Nickname;
    public readonly Color Color => Instance.Color;

    public static implicit operator ClientPlayer(ClientPlayerData data) => new(data);
    public static implicit operator Player(ClientPlayer player) => new(player.Instance);
}

using System.Drawing;
using System.Numerics;

namespace Owop;

public enum PlayerRank
{
    None,
    Player,
    Moderator,
    Admin
}

public enum PlayerTool
{
    Cursor,
    Move,
    Pipette,
    Eraser,
    Zoom,
    Export,
    Fill,
    Line,
    Protect,
    AreaProtect,
    Paste,
    Copy
}

public static class PlayerRanks
{
    public static int GetMaxMessageLength(this PlayerRank rank)
        => rank switch
        {
            PlayerRank.Moderator => 512,
            PlayerRank.Admin => 16384,
            _ => 128
        };

    public static PlayerRank Parse(char c)
        => c switch
        {
            'M' => PlayerRank.Moderator,
            'A' => PlayerRank.Admin,
            _ => PlayerRank.None
        };
}

public class PlayerData(World world)
{
    public readonly World World = world;
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int WorldX { get; set; } = 0;
    public int WorldY { get; set; } = 0;
    public PlayerTool Tool { get; set; } = PlayerTool.Cursor;
    public uint Id { get; set; } = 0;
    public Color Color { get; set; } = Color.Black;
}

public struct Player
{
    private PlayerData Instance;

    public readonly World World => Instance.World;
    public readonly int X => Instance.X;
    public readonly int Y => Instance.Y;
    public readonly Vector2 Pos => new(X, Y);
    public readonly int WorldX => Instance.WorldX;
    public readonly int WorldY => Instance.WorldY;
    public readonly Vector2 WorldPos => new(WorldX, WorldY);
    public readonly PlayerTool Tool => Instance.Tool;
    public readonly uint Id => Instance.Id;
    public readonly Color Color => Instance.Color;

    public static implicit operator Player(PlayerData data) => new() { Instance = data };
}

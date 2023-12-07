using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;

namespace Owop.Game;

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

public record PlayerData
{
    public static PlayerData Empty => new()
    {
        X = 0,
        Y = 0,
        WorldX = 0,
        WorldY = 0,
        Tool = PlayerTool.Cursor,
        Rank = PlayerRank.None,
        Id = 0,
        Color = Color.Black
    };

    public required int X { get; set; }
    public required int Y { get; set; }
    public required int WorldX { get; set; }
    public required int WorldY { get; set; }
    public required PlayerTool Tool { get; set; }
    public required PlayerRank Rank { get; set; }
    public required uint Id { get; set; }
    public required Color Color { get; set; }
}

public record struct Player
{
    private PlayerData Instance;

    public readonly int X => Instance.X;
    public readonly int Y => Instance.Y;
    public readonly Vector2 Pos => new(X, Y);
    public readonly int WorldX => Instance.WorldX;
    public readonly int WorldY => Instance.WorldY;
    public readonly Vector2 WorldPos => new(WorldX, WorldY);
    public readonly PlayerTool Tool => Instance.Tool;
    public readonly PlayerRank Rank => Instance.Rank;
    public readonly uint Id => Instance.Id;
    public readonly Color Color => Instance.Color;

    public static implicit operator Player(PlayerData data) => new() { Instance = data };
}

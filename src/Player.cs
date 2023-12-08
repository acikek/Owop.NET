using System.Drawing;
using System.Numerics;

namespace Owop;

/// <summary>A player's permission rank within a world.</summary>
public enum PlayerRank
{
    /// <summary>No permissions.</summary>
    None,
    /// <summary>Normal placement permissions.</summary>
    Player,
    /// <summary>Elevated placement and tool permissions.</summary>
    Moderator,
    /// <summary>Top-level administrative permissions.</summary>
    Admin
}

/// <summary>A player's selected tool.</summary>
public enum PlayerTool
{
    /// <summary>Default cursor. Places one pixel at a time.</summary>
    Cursor,
    /// <summary>View panning tool.</summary>
    Move,
    /// <summary>Color selection tool.</summary>
    Pipette,
    /// <summary>
    /// <b>Moderator-only</b>. 
    /// Erases an entire chunk, setting its pixels to <see cref="Color.White"/>.
    /// </summary>
    Eraser,
    /// <summary>Changes the view resolution.</summary>
    Zoom,
    /// <summary>Canvas screenshotting tool.</summary>
    Export,
    /// <summary>Continuously replaces adjacent pixels of a certain color.</summary>
    Fill,
    /// <summary>Places pixels in a rasterized line.</summary>
    Line,
    /// <summary>
    /// <b>Moderator-only.</b>
    /// Protects a chunk, meaning that only players ranked <see cref="PlayerRank.Moderator"/>
    /// or above will be able to modify it.
    /// </summary>
    Protect,
    /// <summary>
    /// <b>Moderator-only.</b>
    /// Protects a set of chunks in some rectangular bounds.
    /// </summary>
    AreaProtect,
    /// <summary>
    /// <b>Moderator-only.</b>
    /// Pastes an image onto the canvas.
    /// </summary>
    Paste,
    /// <summary>
    /// <b>Moderator-only.</b>
    /// Copies a section of the canvas.
    /// </summary>
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

public record ChatPlayer(PlayerRank Rank, uint? Id, string? Nickname)
{
    public string Display => Nickname ?? Id?.ToString() ?? string.Empty;

    public static ChatPlayer ParseHeader(string str)
    {
        var rank = PlayerRank.Player;
        uint? id = null;
        string? nickname = null;
        bool restNick = true;
        if (str.StartsWith('('))
        {
            rank = PlayerRanks.Parse(str.ElementAt(1));
            str = str[4..];
        }
        else if (str.StartsWith('['))
        {
            int end = str.IndexOf(']');
            id = uint.Parse(str[1..end]);
            str = str[(end + 2)..];
        }
        else if (uint.TryParse(str, out uint result))
        {
            id = result;
            restNick = false;
        }
        if (restNick)
        {
            nickname = str;
        }
        return new ChatPlayer(rank, id, nickname);
    }
}

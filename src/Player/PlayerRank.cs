using System.Drawing;
using Owop.Util;

namespace Owop.Game;

/// <summary>Enumeration of player permission ranks.</summary>
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

/// <summary>Extension methods for <see cref="PlayerRank"/>.</summary>
public static class PlayerRankExtensions
{
    /// <summary>Returns the maximum chat message length that players with a rank can send.</summary>
    /// <param name="rank">The player rank.</param>
    public static int GetMaxMessageLength(this PlayerRank rank)
        => rank switch
        {
            PlayerRank.Moderator => 512,
            PlayerRank.Admin => 16384,
            _ => 128
        };

    /// <summary>Returns the chat color of players with a specific rank.</summary>
    /// <param name="rank">The player rank.</param>
    public static Color GetChatColor(this PlayerRank rank)
        => rank switch
        {
            PlayerRank.Moderator => ChatColors.Moderator,
            PlayerRank.Admin => ChatColors.Admin,
            _ => ChatColors.Nickname
        };

    /// <summary>Converts a chat 'emblem' character into a player rank.</summary>
    /// <param name="c">The character to convert.</param>
    public static PlayerRank Parse(char c)
        => c switch
        {
            'M' => PlayerRank.Moderator,
            'A' => PlayerRank.Admin,
            _ => PlayerRank.None
        };
}

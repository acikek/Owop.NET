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

public static class PlayerRankExtensions
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

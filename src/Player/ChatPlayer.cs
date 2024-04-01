namespace Owop.Game;

/// <summary>Represents the sender of a chat message.</summary>
/// <param name="Rank">The chat player's rank.</param>
/// <param name="Id">The chat player's ID, if any.</param>
/// <param name="Nickname">The chat player's nickname, if any.</param>
/// <param name="IsDiscord">Whether the message was relayed from Discord.</param>
/// <param name="Header">The chat player's string header before the content.</param>
public record ChatPlayer(PlayerRank Rank, int? Id, string? Nickname, bool IsDiscord, string Header)
{
    /// <summary>The chat player's display name: either their nickname or their ID.</summary>
    public string Display => Nickname ?? Id?.ToString() ?? string.Empty;

    /// <summary>Converts a string header into a <see cref="ChatPlayer"/> object. </summary>
    /// <param name="str">The string header.</param>
    public static ChatPlayer ParseHeader(string str)
    {
        var copy = new string(str);
        var rank = PlayerRank.Player;
        int? id = null;
        string? nickname = null;
        bool discord = false;
        bool restNick = true;
        if (copy.StartsWith('('))
        {
            rank = PlayerRankExtensions.Parse(copy.ElementAt(1));
            copy = copy[4..];
        }
        else if (copy.StartsWith("[D]"))
        {
            rank = PlayerRank.None;
            discord = true;
            copy = copy[4..];
        }
        else if (copy.StartsWith('['))
        {
            int end = copy.IndexOf(']');
            id = int.Parse(copy[1..end]);
            copy = copy[(end + 2)..];
        }
        else if (int.TryParse(copy, out int result))
        {
            id = result;
            restNick = false;
        }
        if (restNick)
        {
            nickname = copy;
        }
        return new ChatPlayer(rank, id, nickname, discord, str);
    }
}

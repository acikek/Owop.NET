namespace Owop;

public record ChatPlayer(PlayerRank Rank, int? Id, string? Nickname)
{
    public string Display => Nickname ?? Id?.ToString() ?? string.Empty;

    public static ChatPlayer ParseHeader(string str)
    {
        var rank = PlayerRank.Player;
        int? id = null;
        string? nickname = null;
        bool restNick = true;
        if (str.StartsWith('('))
        {
            rank = PlayerRankExtensions.Parse(str.ElementAt(1));
            str = str[4..];
        }
        else if (str.StartsWith('['))
        {
            int end = str.IndexOf(']');
            id = int.Parse(str[1..end]);
            str = str[(end + 2)..];
        }
        else if (int.TryParse(str, out int result))
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

namespace Owop.Game;

public record ChatPlayer(PlayerRank Rank, int? Id, string? Nickname, string Header)
{
    public string Display => Nickname ?? Id?.ToString() ?? string.Empty;

    public static ChatPlayer ParseHeader(string str)
    {
        var copy = new string(str);
        var rank = PlayerRank.Player;
        int? id = null;
        string? nickname = null;
        bool restNick = true;
        if (copy.StartsWith('('))
        {
            rank = PlayerRankExtensions.Parse(copy.ElementAt(1));
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
        return new ChatPlayer(rank, id, nickname, str);
    }
}

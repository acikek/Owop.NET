namespace Owop.Game;

public record ChatPlayer(PlayerRank Rank, uint? Id, string? Nickname)
{
    public string Display => Nickname ?? Id?.ToString() ?? string.Empty;

    public static ChatPlayer Parse(string str)
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
};

public record ChatMessage(ChatPlayer Player, string Content)
{
    public static ChatMessage Parse(string str)
    {
        int sep = str.IndexOf(": ");
        var player = ChatPlayer.Parse(str[0..sep]);
        string content = str[(sep + 2)..];
        return new ChatMessage(player, content);
    }
}

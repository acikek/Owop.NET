namespace Owop;

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
};

public record ChatMessage(World World, ChatPlayer Player, string Content)
{
    public static ChatMessage Create(World world, string str)
    {
        int sep = str.IndexOf(": ");
        var player = ChatPlayer.ParseHeader(str[0..sep]);
        string content = str[(sep + 2)..];
        return new ChatMessage(world, player, content);
    }
}

namespace Owop;

public record ChatEventArgs(World World, ChatPlayer Player, string Content)
{
    public static ChatEventArgs Create(World world, string str)
    {
        int sep = str.IndexOf(": ");
        var player = ChatPlayer.ParseHeader(str[0..sep]);
        string content = str[(sep + 2)..];
        return new ChatEventArgs(world, player, content);
    }
}

public record TellEventArgs(World World, int PlayerId, string Content)
{
}

public partial class OwopClient
{
    public event EventHandler<World>? Ready;
    public event EventHandler<World>? ChatReady;
    public event EventHandler<ChatEventArgs>? Chat;
    public event EventHandler<TellEventArgs>? Tell;
}

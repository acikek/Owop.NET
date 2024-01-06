using Owop.Network;

namespace Owop.Client;

public record ChatEventArgs(World World, ChatPlayer Player, string Content)
{
    public IPlayer? WorldPlayer => Player.Id is int id ? World.GetPlayerById(id) : null;
}

public record TellEventArgs(World World, IPlayer Player, string Content);

public partial class OwopClient
{
    public event EventHandler<World>? Ready;
    public event EventHandler<World>? ChatReady;
    public event EventHandler<ChatEventArgs>? Chat;
    public event EventHandler<TellEventArgs>? Tell;
    public event EventHandler<Player>? PlayerConnected;
    public event EventHandler<Player>? PlayerDisconnected;

    private void InvokeChat(ServerMessage message, WorldData world)
    {
        string str = message.Args[0];
        int sep = str.IndexOf(": ");
        var player = ChatPlayer.ParseHeader(str[0..sep]);
        string content = str[(sep + 2)..];
        Chat?.Invoke(this, new(world, player, content));
    }

    private void InvokeTell(ServerMessage message, WorldData world)
    {
        if (int.TryParse(message.Args[0], out int id) /*|| world.Players.TryGetValue(id, out Player? player) || player is null*/)
        {
            Tell?.Invoke(this, new(world, world.Players[id], message.Args[1]));
        }
    }
}

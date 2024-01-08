using Owop.Network;
using Owop.Util;

namespace Owop.Client;

public record ConnectEventArgs(World World, bool IsReconnect);

public record ChatEventArgs(World World, ChatPlayer Player, string Content)
{
    public IPlayer? WorldPlayer => Player.Id is int id ? World.GetPlayerById(id) : null;
}

public record TellEventArgs(World World, IPlayer Player, string Content);

public record TeleportEventArgs(World World, Position Pos, Position WorldPos);

public record WhoisEventArgs(World World, WhoisData Data);

public partial class OwopClient
{
    public event EventHandler<ConnectEventArgs>? Connected;
    public event EventHandler<World>? Ready;
    public event EventHandler<World>? ChatReady;
    public event EventHandler<ChatEventArgs>? Chat;
    public event EventHandler<TellEventArgs>? Tell;
    public event EventHandler<Player>? PlayerConnected;
    public event EventHandler<Player>? PlayerDisconnected;
    public event EventHandler<TeleportEventArgs>? Teleported;
    public event EventHandler<WhoisEventArgs>? Whois;
    public event EventHandler<World>? Disconnecting;
    public event EventHandler? Destroying;

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
        if (int.TryParse(message.Args[0], out int id) && world.World.GetPlayerById(id) is IPlayer player /*|| world.Players.TryGetValue(id, out Player? player) || player is null*/)
        {
            Tell?.Invoke(this, new(world, player, message.Args[1]));
        }
    }

    private void InvokeWhois(ServerMessage message, WorldData world)
    {
        if (WhoisData.Parse(message.Args) is WhoisData data)
        {
            Whois?.Invoke(this, new(world, data));
            if (world.WhoisQueue.Remove(data.PlayerId, out var task))
            {
                task.SetResult(data);
            }
        }
    }
}

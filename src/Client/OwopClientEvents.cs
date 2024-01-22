using System.Drawing;
using Owop.Game;
using Owop.Network;
using Owop.Util;

namespace Owop.Client;

public record ConnectEventArgs(IWorld World, bool IsReconnect);

public record ChatEventArgs(IWorld World, ChatPlayer Player, string Content)
{
    public IPlayer? WorldPlayer => Player.Id is int id ? World.Players[id] : null;

    public Color HeaderColor => Player.IsDiscord ? ChatColors.Discord : Player.Rank.GetChatColor();

    public Color? ContentColor => Player.Rank >= PlayerRank.Moderator ? HeaderColor : null;

    public string GetColoredMessage(Func<string, Color, string> styler)
    {
        var header = styler($"{Player.Header}:", HeaderColor);
        var content = ContentColor is Color color ? styler(Content, color) : Content;
        return $"{header} {content}";
    }
}

public record TellEventArgs(IWorld World, IPlayer Player, string Content);

public record TeleportEventArgs(IWorld World, Position Pos, Position WorldPos);

public record WhoisEventArgs(IWorld World, WhoisData Data);

public record PixelPlacedEventArgs(IWorld World, IPlayer Player, Color Color, Position WorldPos, IChunk Chunk);

public record ChunkLoadedEventArgs(IWorld World, IChunk Chunk);

public partial class OwopClient
{
    public event EventHandler<ConnectEventArgs>? Connected;
    public event EventHandler<IWorld>? Ready;
    public event EventHandler<IWorld>? ChatReady;
    public event EventHandler<ChatEventArgs>? Chat;
    public event EventHandler<TellEventArgs>? Tell;
    public event EventHandler<IPlayer>? PlayerConnected;
    public event EventHandler<IPlayer>? PlayerDisconnected;
    public event EventHandler<TeleportEventArgs>? Teleported;
    public event EventHandler<WhoisEventArgs>? Whois;
    public event EventHandler<PixelPlacedEventArgs>? PixelPlaced;
    public event EventHandler<ChunkLoadedEventArgs>? ChunkLoaded;
    public event EventHandler<IWorld>? Disconnecting;
    public event EventHandler? Destroying;

    private void InvokeChat(ServerMessage message, World world)
    {
        string str = message.Args[0];
        int sep = str.IndexOf(": ");
        var player = ChatPlayer.ParseHeader(str[0..sep]);
        string content = str[(sep + 2)..];
        Chat?.Invoke(this, new(world, player, content));
    }

    private void InvokeTell(ServerMessage message, World world)
    {
        if (int.TryParse(message.Args[0], out int id))
        {
            Tell?.Invoke(this, new(world, world.Players[id], message.Args[1]));
        }
    }

    private void InvokeWhois(ServerMessage message, World world)
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

    public void InvokeDisconnect(World world) => Disconnecting?.Invoke(this, world);
}

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

// TODO: prev color
public record PixelPlacedEventArgs(IWorld World, IPlayer Player, Color Color, Position WorldPos, IChunk Chunk);

public record ChunkEventArgs(IWorld World, IChunk Chunk);

public partial class OwopClient
{
    public event Action<ConnectEventArgs>? Connected;
    public event Action<IWorld>? Ready;
    public event Action<IWorld>? ChatReady;
    public event Action<ChatEventArgs>? Chat;
    public event Action<TellEventArgs>? Tell;
    public event Action<IPlayer>? PlayerConnected;
    public event Action<IPlayer>? PlayerDisconnected;
    public event Action<TeleportEventArgs>? Teleported;
    public event Action<WhoisEventArgs>? Whois;
    public event Action<PixelPlacedEventArgs>? PixelPlaced;
    public event Action<ChunkEventArgs>? ChunkLoaded;
    public event Action<ChunkEventArgs>? ChunkProtectionChanged;
    public event Action<IWorld>? Disconnecting;
    public event Action? Destroying;

    private void InvokeChat(ServerMessage message, World world)
    {
        string str = message.Args[0];
        int sep = str.IndexOf(": ");
        var player = ChatPlayer.ParseHeader(str[0..sep]);
        string content = str[(sep + 2)..];
        Chat?.Invoke(new(world, player, content));
    }

    private void InvokeTell(ServerMessage message, World world)
    {
        if (int.TryParse(message.Args[0], out int id))
        {
            Tell?.Invoke(new(world, world.Players[id], message.Args[1]));
        }
    }

    private void InvokeWhois(ServerMessage message, World world)
    {
        if (WhoisData.Parse(message.Args) is WhoisData data)
        {
            Whois?.Invoke(new(world, data));
            if (world.WhoisQueue.Remove(data.PlayerId, out var task))
            {
                task.SetResult(data);
            }
        }
    }

    public void InvokeDisconnect(World world) => Disconnecting?.Invoke(world);
}

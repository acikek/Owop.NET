using System.Drawing;
using Owop.Game;
using Owop.Network;
using Owop.Util;

namespace Owop.Client;

/// <summary>Arguments for the <see cref="IOwopClient.Connected"/> event.</summary>
/// <param name="World">The world the client connected to.</param>
/// <param name="IsReconnect">Whether the client is reconnecting to the world.</param>
public record ConnectEventArgs(IWorld World, bool IsReconnect);

/// <summary>Arguments for the <see cref="IOwopClient.Chat"/> event.</summary>
/// <param name="World">The world the chat message was sent in.</param>
/// <param name="Player">The player who sent the chat message.</param>
/// <param name="Content">The content of the chat message.</param>
public record ChatEventArgs(IWorld World, ChatPlayer Player, string Content)
{
    /// <summary>
    /// If the chat player has an ID, returns the corresponding <see cref="IPlayer"/> in the
    /// world. Otherwise, returns <c>null</c>.
    /// </summary>
    public IPlayer? WorldPlayer => Player.Id is int id ? World.Players[id] : null;

    /// <summary>The color of the player's header text.</summary>
    public Color HeaderColor => Player.IsDiscord ? ChatColors.Discord : Player.Rank.GetChatColor();

    /// <summary>The color of the message content.</summary>
    public Color? ContentColor => Player.Rank >= PlayerRank.Moderator ? HeaderColor : null;

    /// <summary>Formats the chat message with the corresponding header and content colors.</summary>
    /// <param name="styler">A function that applies the color to the input text.</param>
    /// <returns>The colored message.</returns>
    public string GetColoredMessage(Func<string, Color, string> styler)
    {
        var header = styler($"{Player.Header}:", HeaderColor);
        var content = ContentColor is Color color ? styler(Content, color) : Content;
        return $"{header} {content}";
    }
}

/// <summary>Arguments for the <see cref="IOwopClient.Tell"/> event.</summary>
/// <param name="World">The world the private message was sent in.</param>
/// <param name="Player">The player who sent the private message.</param>
/// <param name="Content">The private message content.</param>
public record TellEventArgs(IWorld World, IPlayer Player, string Content);

/// <summary>Arguments for the <see cref="IOwopClient.Teleported"/> event.</summary>
/// <param name="World">The world the client player was teleported in.</param>
/// <param name="Pos">The raw position the client player was teleported to.</param>
/// <param name="PreviousPos">The previous position of the client player.</param>
public record TeleportEventArgs(IWorld World, Position Pos, Position PreviousPos);

/// <summary>Arguments for the <see cref="IOwopClient.Whois"/> event.</summary>
/// <param name="World">The world the data was received in.</param>
/// <param name="Data">The queried whois data.</param>
public record WhoisEventArgs(IWorld World, WhoisData Data);

/// <summary>Arguments for the <see cref="IOwopClient.PixelPlaced"/> event.</summary>
/// <param name="World">The world the pixel was placed in.</param>
/// <param name="Player">The player who placed the pixel.</param>
/// <param name="Color">The new pixel color.</param>
/// <param name="PreviousColor">The previous pixel color.</param>
/// <param name="WorldPos">The pixel position.</param>
/// <param name="Chunk">The chunk the pixel was placed in.</param>
public record PixelPlacedEventArgs(IWorld World, IPlayer Player, Color Color, Color PreviousColor, Position WorldPos, IChunk Chunk);

/// <summary>Arguments for the <see cref="IOwopClient.ChunkLoaded"/> and <see cref="IOwopClient.ChunkProtectionChanged"/> events.</summary>
/// <param name="World">The chunk's world.</param>
/// <param name="Chunk">The chunk.</param>
public record ChunkEventArgs(IWorld World, IChunk Chunk);

/// <summary>Handles all events for the client.</summary>
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

    /// <summary>Fires <see cref="Chat"/>.</summary>
    /// <param name="message">The server message to decode.</param>
    /// <param name="world">The world the message was received in.</param>
    private void InvokeChat(ServerMessage message, World world)
    {
        string str = message.Args[0];
        int sep = str.IndexOf(": ");
        var player = ChatPlayer.ParseHeader(str[0..sep]);
        string content = str[(sep + 2)..];
        Chat?.Invoke(new(world, player, content));
    }

    /// <summary>Fires <see cref="Tell"/>.</summary>
    /// <param name="message">The server message to decode.</param>
    /// <param name="world">The world the private message was received in.</param>
    private void InvokeTell(ServerMessage message, World world)
    {
        if (int.TryParse(message.Args[0], out int id))
        {
            Tell?.Invoke(new(world, world.Players[id], message.Args[1]));
        }
    }

    /// <summary>Fires <see cref="Whois"/>.</summary>
    /// <param name="message">The server message to decode.</param>
    /// <param name="world">The world the whois data was received in.</param>
    private void InvokeWhois(ServerMessage message, World world)
    {
        if (WhoisData.Parse(message.Args, world) is WhoisData data)
        {
            Whois?.Invoke(new(world, data));
            if (world.WhoisQueue.Remove(data.Player.Id, out var task))
            {
                task.SetResult(data);
            }
        }
    }

    /// <summary>Invokes <see cref="Disconnecting"/>.</summary>
    /// <param name="world">The world to disconnect from.</param>
    public void InvokeDisconnect(World world) => Disconnecting?.Invoke(world);
}

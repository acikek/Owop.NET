using System.Collections.Concurrent;
using System.Drawing;
using Microsoft.Extensions.Logging;
using Owop.Client;
using Owop.Network;
using Owop.Util;

namespace Owop.Game;

/// <summary>An <see cref="IWorld"/> implementation.</summary>
public partial class World : IWorld
{
    /// <summary>The internal player dictionary.</summary>
    public readonly Dictionary<int, IPlayer> _players = [];

    /// <summary>The internal chunk dictionary.</summary>
    public readonly WorldChunks _chunks;

    /// <summary>The internal connected client player.</summary>
    public readonly ClientPlayer _clientPlayer;

    /// <summary>The internal world connection.</summary>
    public readonly WorldConnection _connection;

    /// <summary>An interaction queue for the <see cref="ClientPlayer.ChatBucket"/>.</summary>
    public readonly BucketQueue<string> ChatQueue;

    /// <summary>An interaction queue for the <see cref="ClientPlayer.PixelBucket"/>. </summary>
    public readonly BucketQueue<(Position?, Color?, bool)> PixelQueue;

    /// <summary>An interaction queue for <see cref="ServerMessageType.Whois"/> messages.</summary>
    public ConcurrentDictionary<int, TaskCompletionSource<WhoisData>> WhoisQueue = [];

    public string Name { get; }
    public IReadOnlyDictionary<int, IPlayer> Players => _players;
    public IWorldChunks Chunks => _chunks;
    public IClientPlayer ClientPlayer => _clientPlayer;
    public bool IsChatReady { get; set; }
    public bool IsPasswordProtected { get; set; }
    public IWorldConnection Connection => _connection;
    public ILogger Logger => Connection.Logger;

    /// <summary>Whether the client has made any connection to this world.</summary>
    public bool Connected = false;

    /// <summary>Whether the client has initialized the world data while connecting (including reconnects).</summary>
    public bool Initialized = false;

    /// <summary>Constructs a <see cref="World"/>.</summary>
    /// <param name="name">The cleaned world name.</param>
    /// <param name="connection">The connection to the created world.</param>
    public World(string name, WorldConnection connection)
    {
        Name = name;
        _chunks = new(this);
        _connection = connection;
        _clientPlayer = new(this);
        ChatQueue = new(_clientPlayer._chatBucket, "chat", this, SendChatMessageInternal);
        PixelQueue = new(_clientPlayer._pixelBucket, "pixel", this, PlacePixelInternal);
    }
}

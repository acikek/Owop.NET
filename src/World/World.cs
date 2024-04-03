using System.Collections.Concurrent;
using System.Drawing;
using Microsoft.Extensions.Logging;
using Owop.Client;
using Owop.Network;
using Owop.Util;

namespace Owop.Game;

public partial class World : IWorld
{
    public readonly Dictionary<int, IPlayer> _players = [];
    public readonly WorldChunks _chunks;
    public readonly ClientPlayer _clientPlayer;
    public readonly WorldConnection _connection;

    public readonly BucketQueue<string> ChatQueue;
    public readonly BucketQueue<(Position?, Color?, bool)> PixelQueue;

    public ConcurrentDictionary<int, TaskCompletionSource<WhoisData>> WhoisQueue = [];

    public string Name { get; }
    public IReadOnlyDictionary<int, IPlayer> Players => _players;
    public IWorldChunks Chunks => _chunks;
    public IClientPlayer ClientPlayer => _clientPlayer;
    public bool IsChatReady { get; set; }
    public bool IsPasswordProtected { get; set; }
    public IWorldConnection Connection => _connection;
    public ILogger Logger => Connection.Logger;

    public bool Connected = false;
    public bool Initialized = false;

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

using System.Collections.Concurrent;
using Owop.Util;

namespace Owop.Game;

/// <summary>An <see cref="IClientPlayer"/> implementation.</summary>
/// <param name="world">The client player's connected world.</param>
public partial class ClientPlayer(World world) : Player(world), IClientPlayer
{
    /// <summary>The internal rank value.</summary>
    private PlayerRank _rank;

    /// <summary>The internal pixel bucket.</summary>
    public Bucket _pixelBucket = Bucket.Empty;

    /// <summary>The internal chat bucket.</summary>
    public Bucket _chatBucket = new(4, 6, false, true);

    /// <inheritdoc/>
    public string? Nickname { get; set; }

    /// <inheritdoc/>
    public IBucket PixelBucket => _pixelBucket;

    /// <inheritdoc/>
    public IBucket ChatBucket => _chatBucket;

    /// <inheritdoc/>
    public override bool IsClient => true;

    /// <inheritdoc/>
    public PlayerRank Rank
    {
        get => _rank;
        set
        {
            _rank = value;
            bool admin = Rank == PlayerRank.Admin;
            _pixelBucket.Infinite = admin;
            _chatBucket.Infinite = admin;
        }
    }

    /// <summary>Sends the client player's data to the server.</summary>
    private async Task Send() => await _world._connection.SendPlayerData();
}

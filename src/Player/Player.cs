using System.Drawing;
using Owop.Network;
using Owop.Util;

namespace Owop.Game;

/// <summary>An <see cref="IPlayer"/> implementation.</summary>
/// <param name="world">The player's connected world.</param>
public class Player(World world) : PlayerData, IPlayer
{
    /// <summary>The internal world instance.</summary>
    protected World _world = world;

    /// <inheritdoc/>
    public IWorld World => _world;

    /// <inheritdoc/>
    public virtual bool IsClient => false;

    /// <inheritdoc/>
    public virtual async Task Move(Position pos) => await MoveWorld(pos.ToWorldPos());

    /// <inheritdoc/>
    public virtual async Task MoveWorld(Position worldPos) => await World.MovePlayer(Id, worldPos);

    /// <inheritdoc/>
    public virtual async Task Kick() => await World.KickPlayer(Id);

    /// <inheritdoc/>
    public virtual async Task Mute() => await World.MutePlayer(Id);

    /// <inheritdoc/>
    public virtual async Task Unmute() => await World.MutePlayer(Id);

    /// <inheritdoc/>
    public virtual async Task TeleportTo() => await World.ClientPlayer.TeleportToPlayer(Id);

    /// <inheritdoc/>
    public virtual async Task Tell(string message) => await World.TellPlayer(Id, message);

    /// <inheritdoc/>
    public virtual async Task<WhoisData> QueryWhois() => await World.QueryWhois(Id);

    /// <inheritdoc/>
    public virtual async Task SetRank(PlayerRank rank) => await World.SetPlayerRank(Id, rank);
}

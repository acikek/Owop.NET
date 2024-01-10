using Owop.Network;
using Owop.Util;

namespace Owop;

public partial class Player
{
    public async Task Move(Position pos) => await MoveWorld(pos / World.ChunkSize);

    public async Task MoveWorld(Position worldPos) => await World.MovePlayer(Id, worldPos);

    public async Task Kick() => await World.KickPlayer(Id);

    public async Task Mute() => await World.MutePlayer(Id);

    public async Task Unmute() => await World.MutePlayer(Id);

    public async Task TeleportTo() => await World.ClientPlayer.TeleportToPlayer(Id);

    public async Task Tell(string message) => await World.TellPlayer(Id, message);

    public async Task<WhoisData> QueryWhois() => await World.QueryWhois(Id);

    public async Task SetRank(PlayerRank rank) => await World.SetPlayerRank(Id, rank);
}

using Owop.Util;

namespace Owop;

public partial class Player
{
    public async Task Move(Position pos) => await MoveWorld(pos / World.ChunkSize);

    public async Task MoveWorld(Position worldPos) => await World.MovePlayer(Id, worldPos);

    public async Task TeleportTo() => await World.ClientPlayer.TeleportToPlayer(Id);

    public async Task Tell(string message) => await World.TellPlayer(Id, message);
}

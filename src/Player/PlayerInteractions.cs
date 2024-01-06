namespace Owop;

public partial class Player
{
    public async Task Tell(string message)
        => await World.TellPlayer(Id, message);

    public async Task Move(int x, int y)
        => await World.MovePlayer(Id, x, y);

    public async Task TeleportTo()
        => await World.ClientPlayer.TeleportToPlayer(Id);
}

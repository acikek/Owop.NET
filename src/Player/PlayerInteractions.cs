namespace Owop;

public partial struct Player
{
    public readonly async Task Tell(string message)
        => await World.TellPlayer(Id, message);

    public readonly async Task Move(int x, int y)
        => await World.MovePlayer(Id, x, y);

    public readonly async Task TeleportTo()
        => await World.ClientPlayer.TeleportToPlayer(Id);
}

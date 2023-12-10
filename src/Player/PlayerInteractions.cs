namespace Owop;

public partial struct Player
{
    public readonly async Task Tell(string message)
    {
        await World.Tell(Id, message);
    }
}
